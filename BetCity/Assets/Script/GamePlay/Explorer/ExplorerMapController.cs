using BetCity.Core.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 单个节点的类
/// </summary>
public class Node
{
    public int id;
    public int[] connectedNodes;
    public string things;
    public float Xposition;
    public float Yposition;
}
namespace BetCity.Explorer
{
    /// <summary>
    /// 地图控制器
    /// </summary>
    public class ExplorerMapController : MonoSingleton<ExplorerMapController>
    {
        /// <summary>
        /// 静态属性，存入当前所有结点的类信息
        /// </summary>
        public static Node[] MapNodes = new Node[35];
        /// <summary>
        /// 静态属性，存入当前所有结点的位置信息
        /// </summary>
        public RectTransform[] NodeObj = new RectTransform[35];
        private static bool _initial = false;
        private ExplorerPlayerController playerController;
        private data.PlayerData PlayerData;

        // 初始化地图
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            playerController=ExplorerPlayerController.Instance;
            PlayerData = data.PlayerData.Instance;
            if (!_initial)
            {
                _initial = true;
                RectTransform map = NodeObj[0].transform.parent.parent.gameObject.GetComponent<RectTransform>();
                float x = map.localScale.x;
                float y = map.localScale.y;
                for (int i = 0; i < 35; i++)
                {
                    MapNodes[i] = new Node();
                    MapNodes[i].id = i;
                    MapNodes[i].things = "this is " + i + " node";
                    MapNodes[i].Xposition = NodeObj[i].anchoredPosition.x * x;
                    MapNodes[i].Yposition = NodeObj[i].anchoredPosition.y * y;
                    //之后添加其他的node逻辑
                }
                //我在代码里面使用，也可以在编辑器里面进行赋值，可能增加开销，tl
                MapNodes[0].connectedNodes = new int[] { 1, 2 };
                MapNodes[1].connectedNodes = new int[] { 3 };
                MapNodes[2].connectedNodes = new int[] { 6 };
                MapNodes[3].connectedNodes = new int[] { 4, 7 };
                MapNodes[4].connectedNodes = new int[] { 5 };
                MapNodes[5].connectedNodes = new int[] { 8 };
                MapNodes[6].connectedNodes = new int[] { 5, 9 };
                MapNodes[7].connectedNodes = new int[] { 10 };
                MapNodes[8].connectedNodes = new int[] { 13, 15 };
                MapNodes[9].connectedNodes = new int[] { 8 };
                MapNodes[10].connectedNodes = new int[] { 11 };
                MapNodes[11].connectedNodes = new int[] { 12 };
                MapNodes[12].connectedNodes = new int[] { 18 };
                MapNodes[13].connectedNodes = new int[] { 14 };
                MapNodes[14].connectedNodes = new int[] { 18 };
                MapNodes[15].connectedNodes = new int[] { 16 };
                MapNodes[16].connectedNodes = new int[] { 17 };
                MapNodes[17].connectedNodes = new int[] { 18 };
                MapNodes[18].connectedNodes = new int[] { 19, 22, 24 };
                MapNodes[19].connectedNodes = new int[] { 20 };
                MapNodes[20].connectedNodes = new int[] { 21 };
                MapNodes[21].connectedNodes = new int[] { };
                MapNodes[22].connectedNodes = new int[] { 23 };
                MapNodes[23].connectedNodes = new int[] { 27, 29 };
                MapNodes[24].connectedNodes = new int[] { 25 };
                MapNodes[25].connectedNodes = new int[] { 26 };
                MapNodes[26].connectedNodes = new int[] { 28 };
                MapNodes[27].connectedNodes = new int[] { 28 };
                MapNodes[28].connectedNodes = new int[] { 33 };
                MapNodes[29].connectedNodes = new int[] { 30 };
                MapNodes[30].connectedNodes = new int[] { 31 };
                MapNodes[31].connectedNodes = new int[] { 32 };
                MapNodes[32].connectedNodes = new int[] { 34 };
                MapNodes[33].connectedNodes = new int[] { 34 };
                MapNodes[34].connectedNodes = new int[] { };

            }
            playerController.ToNodeInstant(MapNodes[PlayerData.CurrentNodeNum]);
        }
        /// <summary>
        /// 按钮绑定函数，前往对应结点
        /// </summary>
        public void ToNode(int nodenum)
        {
            playerController.UseNodeChange(MapNodes[nodenum]);
        }
        /// <summary>
        /// 检查结点是否可到达
        /// </summary>
        public bool CheckNode(int currentnodenum,int targetnodenum)
        {
            foreach (int j in MapNodes[currentnodenum].connectedNodes)
            {
                if (j == targetnodenum)
                {
                    return true;
                }
            }
            ExplorerScreenController.CreateMessage("无法到达");
            return false;
        }
        /// <summary>
        /// 废弃函数，暂时不用
        /// </summary>
        public void Travel(string scenename)
        {
            Debug.Log($"切换到空白场景");

            playerController.ManualSave();
            SceneManager.LoadScene(scenename);
        }
    }
}
