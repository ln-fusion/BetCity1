using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using BetCity.GamePlay.Explorer; // 必须添加这个命名空间引用

[ExecuteAlways]
public class ExplorerDice : MonoBehaviour
{
    // 安全获取玩家数据：加入问号 (?) 防止在编辑器下崩溃
    public int[] Num => ExplorerPlayerController.Instance?.PlayerData?.Dice;

    [Header("资源设置")]
    public Texture[] diceTextures; // 拖入 6 张数字贴图

    [Header("引用设置")]
    public MeshRenderer[] faceRenderers; // 拖入 6 个面的 MeshRenderer

    private MaterialPropertyBlock _propBlock;

    private void OnEnable() => _propBlock = new MaterialPropertyBlock();

    private void Start() => UpdateAllFaces();

    // 当在 Inspector 修改贴图或拖入面片时立即刷新预览
    private void OnValidate() => UpdateAllFaces();

    public void UpdateAllFaces()
    {
        // 基础检查
        if (faceRenderers == null || faceRenderers.Length != 6 || _propBlock == null) return;

        // 仅在运行时获取真实数据
        int[] currentNums = Application.isPlaying ? Num : null;

        for (int i = 0; i < 6; i++)
        {
            if (faceRenderers[i] == null) continue;

            int textureIndex;
            // 运行时显示玩家数据，编辑器下按 0-5 序列预览 1-6 数字
            if (currentNums != null && i < currentNums.Length)
            {
                textureIndex = Mathf.Clamp(currentNums[i] - 1, 0, diceTextures.Length - 1);
            }
            else
            {
                textureIndex = Mathf.Clamp(i, 0, diceTextures.Length - 1);
            }

            if (diceTextures != null && textureIndex < diceTextures.Length)
            {
                faceRenderers[i].GetPropertyBlock(_propBlock);
                // 兼容不同 Shader：Standard 用 _MainTex, URP 用 _BaseMap
                _propBlock.SetTexture("_MainTex", diceTextures[textureIndex]);
                _propBlock.SetTexture("_BaseMap", diceTextures[textureIndex]);
                faceRenderers[i].SetPropertyBlock(_propBlock);
            }
        }
    }


    // 这里的 index 对应 Num 数组的下标 (0-5)
    private Vector3 GetRotationByIndex(int index)
    {
        switch (index)
        {
            case 0: return new Vector3(180, 0, 0);    // 1点
            case 1: return new Vector3(0, 0, 0);      // 2点
            case 2: return new Vector3(90, 0, 90);    // 3点
            case 3: return new Vector3(90, -90, 180); // 4点
            case 4: return new Vector3(90, -180, 0);  // 5点
            case 5: return new Vector3(90, 0, 0);     // 6点
            default: return new Vector3(0, 45, 0);    // 如果出错，转到一个奇怪角度方便排查
        }
    }

    public async UniTask<int> Throw(CancellationToken cancellationToken)
    {
        // --- 测试: 强制给定测试数据 ---
        int[] data = new int[] { 1, 2, 3, 4, 5, 6 };

        // 随机选一个索引 (0-5)
        int randomIndex = Random.Range(0, data.Length);
        int finalResult = data[randomIndex];

        // 获取目标旋转角度
        Vector3 targetEuler = GetRotationByIndex(randomIndex);

        // 【关键 Log】: 看看这里打印的是不是 (0,0,0)？
        Debug.Log($"<color=cyan>随机到的索引: {randomIndex}, 目标角度: {targetEuler}</color>");

        float duration = 1.5f;
        float elapsed = 0f;
        Vector3 randomAxis = new Vector3(Random.value, Random.value, Random.value).normalized;

        // A. 乱转阶段
        while (elapsed < duration * 0.8f)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float speed = 1200f * (1 - (elapsed / duration)); // 逐渐减速
            transform.Rotate(randomAxis * speed * Time.deltaTime, Space.Self);
            elapsed += Time.deltaTime;
            await UniTask.Yield();
        }

        // B. 对齐阶段
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = Quaternion.Euler(targetEuler); // 【检查点】确保 targetEuler 不是零

        float alignElapsed = 0f;
        float alignDuration = 0.3f; // 固定 0.3 秒对齐

        while (alignElapsed < alignDuration)
        {
            alignElapsed += Time.deltaTime;
            float t = alignElapsed / alignDuration;
            // 使用 Slerp 平滑插值
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
            await UniTask.Yield();
        }

        // C. 最终锁定
        transform.localRotation = endRotation;

        return finalResult;
    }

    private async UniTask WaitIfPaused(CancellationToken token)
    {
        // 如果项目有统一的暂停逻辑可以在这里接入
        await UniTask.CompletedTask;
    }
}