using BetCity.GamePlay.Explorer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Data.ConfigModels
{
    /// <summary>
    /// 单个节点的类
    /// </summary>
    [CreateAssetMenu(fileName = "Node", menuName = "Map/Node")]
    [Serializable]
    public class Node : ScriptableObject
    {
        [Header("填写id")]
        public NodeID ID;
        [Header("连接节点")]
        public int[] connectedNodes;
        [Header("节点类型")]
        public EventType eventType;
        public float Xposition => NodePosition.anchoredPosition.x;
        public float Yposition => NodePosition.anchoredPosition.y;
        [Header("这个不需要填写")]
        public RectTransform NodePosition;
    }
}
