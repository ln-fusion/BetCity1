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
        public NodeID Id;
        [Header("连接节点")]
        public int[] ConnectedNodes;
        [Header("节点类型")]
        public TypeOfEvent EventType;
        [field: SerializeField, Header("节点事件Id若无固定事件设为-1")]
        public int EventId { get; private set; } = -1;
        public float Xposition => NodePosition.anchoredPosition.x;
        public float Yposition => NodePosition.anchoredPosition.y;
        [NonSerialized]
        public RectTransform NodePosition;
        [Serializable]
        public class NodeID
        {
            public int MapId;
            public int Id;
        }
    }
}
