using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.Tools
{
    /// <summary>
    /// Unity 通用优先队列（基于最小堆实现，优先级值越小，优先级越高）
    /// </summary>
    /// <typeparam name="T">队列元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型（需实现 IComparable）</typeparam>
    public class PriorityQueue<T, TPriority> : IReadOnlyPriorityQueue<T, TPriority>, IEnumerable<T>
        where TPriority : IComparable<TPriority>
    {
        // 存储堆元素（元素 + 优先级）
        private readonly List<(T Element, TPriority Priority)> heap;
        // 元素到索引的映射（用于快速更新优先级）
        private readonly Dictionary<T, int> elementIndexMap;
        // 自定义比较器（可选，默认用 TPriority 的 CompareTo）
        private readonly IComparer<TPriority> comparer;

        /// <summary>
        /// 队列元素数量
        /// </summary>
        public int Count => heap.Count;

        /// <summary>
        /// 是否为空
        /// </summary>
        public bool IsEmpty => heap.Count == 0;

        /// <summary>
        /// 构造函数（默认比较器）
        /// </summary>
        public PriorityQueue() : this(Comparer<TPriority>.Default) { }

        /// <summary>
        /// 构造函数（自定义比较器）
        /// </summary>
        /// <param name="comparer">优先级比较器</param>
        public PriorityQueue(IComparer<TPriority> comparer)
        {
            heap = new List<(T, TPriority)>();
            elementIndexMap = new Dictionary<T, int>();
            this.comparer = comparer ?? Comparer<TPriority>.Default;
        }

        /// <summary>
        /// 入队（添加元素 + 优先级）
        /// </summary>
        /// <param name="element">元素</param>
        /// <param name="priority">优先级（值越小，优先级越高）</param>
        public void Enqueue(T element, TPriority priority)
        {
            if (element == null)
            {
                Debug.LogWarning("优先队列不支持 null 元素！");
                return;
            }

            // 元素已存在则更新优先级
            if (elementIndexMap.ContainsKey(element))
            {
                UpdatePriority(element, priority);
                return;
            }

            // 新增元素到堆尾，然后上浮调整堆
            heap.Add((element, priority));
            int lastIndex = heap.Count - 1;
            elementIndexMap[element] = lastIndex;
            UpHeap(lastIndex);
        }

        /// <summary>
        /// 出队（移除并返回优先级最高的元素）
        /// </summary>
        /// <returns>优先级最高的元素</returns>
        /// <exception cref="InvalidOperationException">队列为空时抛出</exception>
        public T Dequeue()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("优先队列为空，无法出队！");
            }

            // 取堆顶元素（优先级最高）
            var top = heap[0];
            RemoveAt(0);
            return top.Element;
        }

        /// <summary>
        /// 查看队首元素（不移除）
        /// </summary>
        /// <returns>优先级最高的元素</returns>
        /// <exception cref="InvalidOperationException">队列为空时抛出</exception>
        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("优先队列为空，无法查看队首！");
            }
            return heap[0].Element;
        }

        /// <summary>
        /// 更新指定元素的优先级
        /// </summary>
        /// <param name="element">要更新的元素</param>
        /// <param name="newPriority">新优先级</param>
        /// <exception cref="KeyNotFoundException">元素不存在时抛出</exception>
        public void UpdatePriority(T element, TPriority newPriority)
        {
            if (!elementIndexMap.TryGetValue(element, out int index))
            {
                throw new KeyNotFoundException($"元素 {element} 不存在于优先队列中！");
            }

            // 记录旧优先级，更新堆中的优先级
            TPriority oldPriority = heap[index].Priority;
            heap[index] = (element, newPriority);

            // 根据新旧优先级的大小，决定上浮或下沉
            int compareResult = comparer.Compare(newPriority, oldPriority);
            if (compareResult < 0)
            {
                UpHeap(index); // 新优先级更高（值更小），上浮
            }
            else if (compareResult > 0)
            {
                DownHeap(index); // 新优先级更低（值更大），下沉
            }
        }

        /// <summary>
        /// 移除指定元素
        /// </summary>
        /// <param name="element">要移除的元素</param>
        /// <returns>是否移除成功</returns>
        public bool Remove(T element)
        {
            if (!elementIndexMap.TryGetValue(element, out int index))
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            heap.Clear();
            elementIndexMap.Clear();
        }

        /// <summary>
        /// 检查元素是否存在
        /// </summary>
        /// <param name="element">要检查的元素</param>
        /// <returns>是否存在</returns>
        public bool Contains(T element)
        {
            return elementIndexMap.ContainsKey(element);
        }

        /// <summary>
        /// 获取枚举器（遍历队列中的所有元素）
        /// </summary>
        /// <returns>元素枚举器</returns>
        public IEnumerator<T> GetEnumerator()
        {
            // 遍历堆中的元素，仅返回元素本身（忽略优先级）
            foreach (var item in heap)
            {
                yield return item.Element;
            }
        }

        /// <summary>
        /// 非泛型枚举器实现
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region 堆核心操作（私有）
        /// <summary>
        /// 上浮：将指定索引的元素向上调整，维护堆结构
        /// </summary>
        private void UpHeap(int index)
        {
            var current = heap[index];
            int parentIndex = (index - 1) / 2;

            // 向上比较，直到父节点优先级更低 或 到达堆顶
            while (index > 0 && comparer.Compare(current.Priority, heap[parentIndex].Priority) < 0)
            {
                // 父节点下移
                heap[index] = heap[parentIndex];
                elementIndexMap[heap[index].Element] = index;

                // 继续向上
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }

            // 最终位置放入当前元素
            heap[index] = current;
            elementIndexMap[current.Element] = index;
        }

        /// <summary>
        /// 下沉：将指定索引的元素向下调整，维护堆结构
        /// </summary>
        private void DownHeap(int index)
        {
            int lastIndex = heap.Count - 1;
            var current = heap[index];

            while (true)
            {
                int leftChildIndex = index * 2 + 1;
                int rightChildIndex = index * 2 + 2;
                int smallestChildIndex = index;

                // 找优先级最高的子节点
                if (leftChildIndex <= lastIndex && comparer.Compare(heap[leftChildIndex].Priority, heap[smallestChildIndex].Priority) < 0)
                {
                    smallestChildIndex = leftChildIndex;
                }
                if (rightChildIndex <= lastIndex && comparer.Compare(heap[rightChildIndex].Priority, heap[smallestChildIndex].Priority) < 0)
                {
                    smallestChildIndex = rightChildIndex;
                }

                // 没有更小的子节点，停止下沉
                if (smallestChildIndex == index)
                {
                    break;
                }

                // 子节点上移
                heap[index] = heap[smallestChildIndex];
                elementIndexMap[heap[index].Element] = index;

                // 继续向下
                index = smallestChildIndex;
            }

            // 最终位置放入当前元素
            heap[index] = current;
            elementIndexMap[current.Element] = index;
        }

        /// <summary>
        /// 移除指定索引的元素
        /// </summary>
        private void RemoveAt(int index)
        {
            int lastIndex = heap.Count - 1;
            var removedElement = heap[index].Element;

            // 移除映射
            elementIndexMap.Remove(removedElement);

            // 如果是最后一个元素，直接移除
            if (index == lastIndex)
            {
                heap.RemoveAt(lastIndex);
                return;
            }

            // 将最后一个元素移到当前索引，然后调整堆
            heap[index] = heap[lastIndex];
            elementIndexMap[heap[index].Element] = index;
            heap.RemoveAt(lastIndex);

            // 调整堆（上浮或下沉）
            DownHeap(index);
            UpHeap(index);
        }
        #endregion
    }
}
