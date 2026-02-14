using UnityEngine;
using BetCity.Card;

namespace BetCity.Tools.Test
{
    /// <summary>
    /// 简单测试脚本：Play时在指定位置生一张卡，调用CardManager.SpawnCardView
    /// </summary>
    public class TestCardSpawner : MonoBehaviour
    {
        [SerializeField] private int cardId = 0;
        [SerializeField] private Vector3 spawnPosition = new Vector3(0f, 0f, 1f);
        [SerializeField] private Transform parent = null;

        private void Start()
        {
            if (CardManager.Instance == null)
            {
                Debug.LogError("[TestCardSpawner] CardManager.Instance 为 null，请确保场景中存在 CardManager 单例对象。");
                return;
            }

            GameObject go = CardManager.Instance.SpawnCardView(cardId, spawnPosition, parent);
            if (go == null)
            {
                Debug.LogError($"[TestCardSpawner] 无法生成卡牌，id={cardId} 可能不存在或 prefab 未设置。");
            }
            else
            {
                Debug.Log($"[TestCardSpawner] 已生成卡牌 id={cardId} 在位置 {spawnPosition}");
            }
        }
    }
}
