using BetCity.Core.Tools;
using System;
using UnityEngine;
using BetCity.Data.ConfigModels;
using UnityEngine.UI;

namespace BetCity.GamePlay.Explorer
{
    [Serializable]
    public class NodeID
    { 
        public int MapID;
        public int ID;
    }

    [Serializable]
    public class MapData:MonoBehaviour
    {
        [Header("这些需要填")]
        public int MapID;
        [Header("地图名称")]
        public string MapName;
        public RectTransform[] NodeObject;
        [Header("地图图像像素比")]
        public float BackWidth;
        public float BackHeight;
        /// <summary>
        /// 自定义屏幕缩放比
        /// </summary>
        public float MapScale;
        [Header("节点个数")]
        public int NodeNumber;
        public Node[] MapNode;
        public void MapInital(ExplorerMapController mapController)
        {
            switch (MapID)
            {
                case 0:
                    for (int i = 0; i < NodeNumber; i++)
                    {
                        int index = i;
                        NodeObject[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            mapController.ToNode(index);
                        });
                        MapNode[i].NodePosition = NodeObject[i];
                    }

                    break;
                case 1:
                    for (int i = 0; i < NodeNumber; i++)
                    {
                        int index = i;
                        NodeObject[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            mapController.ToNode(index);
                        });
                        MapNode[i].NodePosition = NodeObject[i];
                    }
                    //MapNode[0].connectedNodes = new int[] { 1, 2 };
                    //MapNode[1].connectedNodes = new int[] { 3 };
                    //MapNode[2].connectedNodes = new int[] { 6 };
                    //MapNode[3].connectedNodes = new int[] { 4, 7 };
                    //MapNode[4].connectedNodes = new int[] { 5 };
                    //MapNode[5].connectedNodes = new int[] { 8 };
                    //MapNode[6].connectedNodes = new int[] { 5, 9 };
                    //MapNode[7].connectedNodes = new int[] { 10 };
                    //MapNode[8].connectedNodes = new int[] { 13, 15 };
                    //MapNode[9].connectedNodes = new int[] { 8 };
                    //MapNode[10].connectedNodes = new int[] { 11 };
                    //MapNode[11].connectedNodes = new int[] { 12 };
                    //MapNode[12].connectedNodes = new int[] { 18 };
                    //MapNode[13].connectedNodes = new int[] { 14 };
                    //MapNode[14].connectedNodes = new int[] { 18 };
                    //MapNode[15].connectedNodes = new int[] { 16 };
                    //MapNode[16].connectedNodes = new int[] { 17 };
                    //MapNode[17].connectedNodes = new int[] { 18 };
                    //MapNode[18].connectedNodes = new int[] { 19, 22, 24 };
                    //MapNode[19].connectedNodes = new int[] { 20 };
                    //MapNode[20].connectedNodes = new int[] { 21 };
                    //MapNode[21].connectedNodes = new int[] { };
                    //MapNode[22].connectedNodes = new int[] { 23 };
                    //MapNode[23].connectedNodes = new int[] { 27, 29 };
                    //MapNode[24].connectedNodes = new int[] { 25 };
                    //MapNode[25].connectedNodes = new int[] { 26 };
                    //MapNode[26].connectedNodes = new int[] { 28 };
                    //MapNode[27].connectedNodes = new int[] { 28 };
                    //MapNode[28].connectedNodes = new int[] { 33 };
                    //MapNode[29].connectedNodes = new int[] { 30 };
                    //MapNode[30].connectedNodes = new int[] { 31 };
                    //MapNode[31].connectedNodes = new int[] { 32 };
                    //MapNode[32].connectedNodes = new int[] { 34 };
                    //MapNode[33].connectedNodes = new int[] { 34 };
                    //MapNode[34].connectedNodes = new int[] { };


                    //各个结点随即绑定事件的逻辑
                    break;
            }
        }
    }
}
