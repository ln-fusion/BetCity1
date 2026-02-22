using BetCity.Core.Tools;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BetCity.Core.ResourceSystem
{
    /// <summary>
    /// Addressable资源管理器（UniTask版本）
    /// </summary>
    public class AddressableManager : MonoSingleton<AddressableManager>
    {
        // 资源加载句柄缓存（避免重复加载）
        private readonly Dictionary<string, AsyncOperationHandle> _handleCache = new();
        // 引用计数（处理多地方引用同一资源的释放问题）
        private readonly Dictionary<string, int> _refCount = new();
        // 加载锁（防止同一资源并发加载）
        private readonly Dictionary<string, UniTaskCompletionSource> _loadLocks = new();

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 异步加载资源（UniTask通用型）
        /// </summary>
        /// <typeparam name="T">资源类型（GameObject, Sprite, AudioClip等）</typeparam>
        /// <param name="address">资源地址（Addressables分组中设置的地址）</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>加载完成的资源实例</returns>
        /// <exception cref="ArgumentNullException">地址为空时抛出</exception>
        /// <exception cref="InvalidCastException">类型转换失败时抛出</exception>
        /// <exception cref="Exception">加载失败时抛出（包含原始错误信息）</exception>
        public async UniTask<T> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default)
            where T : UnityEngine.Object
        {
            // 空地址校验
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address), "资源地址不能为空");
            }

            // 检查取消状态
            cancellationToken.ThrowIfCancellationRequested();

            // 加载锁：防止同一资源被并发请求重复加载
            if (_loadLocks.TryGetValue(address, out var existingTcs))
            {
                // 等待已有加载任务完成
                await existingTcs.Task.AttachExternalCancellation(cancellationToken);
            }
            else
            {
                // 无加载任务，创建新的加载锁
                var newTcs = new UniTaskCompletionSource();
                _loadLocks[address] = newTcs;

                try
                {
                    // 首次加载：检查缓存中是否已有完成的句柄
                    if (!_handleCache.ContainsKey(address))
                    {
                        var handle = Addressables.LoadAssetAsync<T>(address);
                        _handleCache[address] = handle;

                        // 等待加载完成（响应取消）
                        await handle.ToUniTask(cancellationToken: cancellationToken);
                    }
                    else
                    {
                        // 缓存中已有句柄，等待其完成（可能还在加载中）
                        var handle = _handleCache[address];
                        if (!handle.IsDone)
                        {
                            await handle.ToUniTask(cancellationToken: cancellationToken);
                        }
                    }

                    // 加载完成，释放锁
                    newTcs.TrySetResult();
                }
                catch (Exception e)
                {
                    // 加载失败：清理锁和缓存
                    newTcs.TrySetException(e);
                    _handleCache.Remove(address);
                    _refCount.Remove(address);
                    throw;
                }
                finally
                {
                    _loadLocks.Remove(address);
                }
            }

            // 校验加载结果
            var resultHandle = _handleCache[address];
            if (resultHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception($"加载资源{address}失败：{resultHandle.OperationException?.Message ?? "未知错误"}");
            }

            // 类型转换校验
            if (!(resultHandle.Result is T result))
            {
                throw new InvalidCastException($"资源{address}类型转换失败，目标类型：{typeof(T).Name}");
            }

            // 引用计数+1
            _refCount[address] = _refCount.TryGetValue(address, out int count) ? count + 1 : 1;

            return result;
        }

        /// <summary>
        /// 异步加载并实例化预制体
        /// </summary>
        /// <param name="address">预制体地址</param>
        /// <param name="parent">父节点</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>实例化后的游戏对象</returns>
        public async UniTask<GameObject> InstantiatePrefabAsync(string address, Transform parent = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address), "预制体地址不能为空");
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 加载锁逻辑
            if (_loadLocks.TryGetValue(address, out var existingTcs))
            {
                await existingTcs.Task.AttachExternalCancellation(cancellationToken);
            }
            else
            {
                var newTcs = new UniTaskCompletionSource();
                _loadLocks[address] = newTcs;

                try
                {
                    var handle = Addressables.InstantiateAsync(address, parent);
                    _handleCache[address] = handle;

                    // 等待实例化完成
                    await handle.ToUniTask(cancellationToken: cancellationToken);
                    newTcs.TrySetResult();
                }
                catch (Exception e)
                {
                    newTcs.TrySetException(e);
                    _handleCache.Remove(address);
                    _refCount.Remove(address);
                    throw;
                }
                finally
                {
                    _loadLocks.Remove(address);
                }
            }

            var resultHandle = _handleCache[address];
            if (resultHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception($"实例化预制体{address}失败：{resultHandle.OperationException?.Message ?? "未知错误"}");
            }

            var instance = resultHandle.Result as GameObject;
            if (instance == null)
            {
                throw new InvalidCastException($"预制体{address}实例化结果不是GameObject类型");
            }

            // 引用计数+1
            _refCount[address] = _refCount.TryGetValue(address, out int count) ? count + 1 : 1;

            return instance;
        }

        /// <summary>
        /// 释放单个资源
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>是否成功释放（true=释放/引用计数-1；false=资源未加载）</returns>
        public bool ReleaseAsset(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                Debug.LogWarning("释放资源失败：地址为空");
                return false;
            }

            lock (_handleCache) // 线程安全锁
            {
                if (!_handleCache.ContainsKey(address) || !_refCount.ContainsKey(address))
                {
                    Debug.LogWarning($"资源{address}未加载，无需释放");
                    return false;
                }

                // 引用计数-1
                _refCount[address]--;
                if (_refCount[address] > 0)
                {
                    Debug.Log($"资源{address}引用计数减1，当前计数：{_refCount[address]}");
                    return true;
                }

                // 引用计数为0，释放资源
                var handle = _handleCache[address];
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    Debug.Log($"资源{address}已释放（引用计数为0）");
                }

                // 清理缓存
                _handleCache.Remove(address);
                _refCount.Remove(address);
                return true;
            }
        }

        /// <summary>
        /// 释放实例化的预制体对象
        /// </summary>
        /// <param name="obj">实例化的对象</param>
        /// <param name="address">预制体地址</param>
        /// <param name="immediate">是否立即销毁（false则走Unity正常销毁流程）</param>
        public void ReleaseInstance(GameObject obj, string address, bool immediate = false)
        {
            if (obj == null)
            {
                Debug.LogWarning("释放实例失败：对象为空");
                return;
            }

            // 释放实例句柄
            if (Addressables.ReleaseInstance(obj))
            {
                Debug.Log($"预制体实例{obj.name}已释放");
            }

            // 释放资源引用计数
            ReleaseAsset(address);

            // 销毁对象
            if (immediate)
            {
                DestroyImmediate(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        /// <summary>
        /// 释放所有已加载资源
        /// </summary>
        public void ReleaseAll()
        {
            lock (_handleCache)
            {
                foreach (var handle in _handleCache.Values)
                {
                    if (handle.IsValid())
                    {
                        Addressables.Release(handle);
                    }
                }

                _handleCache.Clear();
                _refCount.Clear();
                _loadLocks.Clear();
                Debug.Log("所有Addressable资源已释放");
            }
        }

        /// <summary>
        /// 检查资源是否已加载完成
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>是否已加载完成</returns>
        public bool IsAssetLoaded(string address)
        {
            lock (_handleCache)
            {
                return _handleCache.TryGetValue(address, out var handle) && handle.IsDone;
            }
        }

        /// <summary>
        /// 获取资源当前引用计数
        /// </summary>
        /// <param name="address">资源地址</param>
        /// <returns>引用计数（未加载返回0）</returns>
        public int GetRefCount(string address)
        {
            _refCount.TryGetValue(address, out int count);
            return count;
        }

        // 单例销毁时释放所有资源
        protected void OnDestroy()
        {
            ReleaseAll();
        }
    }

}