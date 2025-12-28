using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 单个节点的类
    /// </summary>
    public class Node
    {
        public int id;
        public int[] connectedNodes;
        public string things;
        public float Xposition=>nodePosition.anchoredPosition.x;
        public float Yposition => nodePosition.anchoredPosition.y;
        public RectTransform nodePosition;
    }
    [Serializable]
    public class MapData:MonoBehaviour
            //public class MapData : MonoSingleton<MapData>
    {
        [Header("这些需要填")]
        public int MapID;
        public string MapName;
        public RectTransform[] NodeObject;
        public float BackWidth;
        public float BackHeight;
        /// <summary>
        /// 自定义屏幕缩放比
        /// </summary>
        public float MapScale;
        [Header("这些不用填")]
        public int NodeNumber;
        public Node[] MapNode;
        [Header("这些变量初始化的时候自动填写")]
        public MapEvent[] Action;
        //protected override void Awake()
        //{
        //    base.Awake();
        //}
        public void MapInital(ExplorerMapController mapController)
        {
            switch (MapID)
            {
                case 0:
                    NodeNumber = 7;
                    MapNode = new Node[NodeNumber];
                    Action = new MapEvent[NodeNumber];
                    for (int i = 0; i < NodeNumber; i++)
                    {
                        MapNode[i] = new Node();
                        MapNode[i].id = i;
                        Action[i] = new MapEvent();
                        MapNode[i].nodePosition = NodeObject[i];
                        int index = i;
                        NodeObject[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            mapController.ToNode(index);
                        });
                    }
                    MapNode[0].connectedNodes = new int[] { 1 };
                    MapNode[1].connectedNodes = new int[] { 2 };
                    MapNode[2].connectedNodes = new int[] { 3 };
                    MapNode[3].connectedNodes = new int[] { 4 };
                    MapNode[4].connectedNodes = new int[] { 5 };
                    MapNode[5].connectedNodes = new int[] { 6 };
                    MapNode[6].connectedNodes = new int[] {  };
                    Action[0].actionType = EventType.Dialogue;
                    Action[1].actionType = EventType.Battle;
                    Action[2].actionType = EventType.Shop;
                    Action[3].actionType = EventType.Battle;
                    Action[4].actionType = EventType.Home;
                    Action[5].actionType = EventType.Battle;
                    Action[6].actionType = EventType.Warehouse;
                    break;
                case 1:
                    NodeNumber = 35;
                    MapNode = new Node[NodeNumber];
                    Action=new MapEvent[NodeNumber];
                    for (int i = 0; i < NodeNumber; i++)
                    {
                        MapNode[i] = new Node();
                        MapNode[i].id = i;
                        MapNode[i].nodePosition = NodeObject[i];
                        NodeObject[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            mapController.ToNode(i);
                        });
                    }
                    MapNode[0].connectedNodes = new int[] { 1, 2 };
                    MapNode[1].connectedNodes = new int[] { 3 };
                    MapNode[2].connectedNodes = new int[] { 6 };
                    MapNode[3].connectedNodes = new int[] { 4, 7 };
                    MapNode[4].connectedNodes = new int[] { 5 };
                    MapNode[5].connectedNodes = new int[] { 8 };
                    MapNode[6].connectedNodes = new int[] { 5, 9 };
                    MapNode[7].connectedNodes = new int[] { 10 };
                    MapNode[8].connectedNodes = new int[] { 13, 15 };
                    MapNode[9].connectedNodes = new int[] { 8 };
                    MapNode[10].connectedNodes = new int[] { 11 };
                    MapNode[11].connectedNodes = new int[] { 12 };
                    MapNode[12].connectedNodes = new int[] { 18 };
                    MapNode[13].connectedNodes = new int[] { 14 };
                    MapNode[14].connectedNodes = new int[] { 18 };
                    MapNode[15].connectedNodes = new int[] { 16 };
                    MapNode[16].connectedNodes = new int[] { 17 };
                    MapNode[17].connectedNodes = new int[] { 18 };
                    MapNode[18].connectedNodes = new int[] { 19, 22, 24 };
                    MapNode[19].connectedNodes = new int[] { 20 };
                    MapNode[20].connectedNodes = new int[] { 21 };
                    MapNode[21].connectedNodes = new int[] { };
                    MapNode[22].connectedNodes = new int[] { 23 };
                    MapNode[23].connectedNodes = new int[] { 27, 29 };
                    MapNode[24].connectedNodes = new int[] { 25 };
                    MapNode[25].connectedNodes = new int[] { 26 };
                    MapNode[26].connectedNodes = new int[] { 28 };
                    MapNode[27].connectedNodes = new int[] { 28 };
                    MapNode[28].connectedNodes = new int[] { 33 };
                    MapNode[29].connectedNodes = new int[] { 30 };
                    MapNode[30].connectedNodes = new int[] { 31 };
                    MapNode[31].connectedNodes = new int[] { 32 };
                    MapNode[32].connectedNodes = new int[] { 34 };
                    MapNode[33].connectedNodes = new int[] { 34 };
                    MapNode[34].connectedNodes = new int[] { };


                    //各个结点随即绑定事件的逻辑
                    break;
            }
        }
    }
}
