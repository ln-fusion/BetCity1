using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public class Explorer_MapController : MonoBehaviour
    {
        public static Node[] node = new Node[35];
        public RectTransform[] node_obj = new RectTransform[35];
        private static bool Initial = false;
        public Explorer_PlayerController playerController;
        /// 初始化地图
        private void Start()
        {
            if (!Initial)
            {
                Initial = true;
                RectTransform map = node_obj[0].transform.parent.parent.gameObject.GetComponent<RectTransform>();
                float x = map.localScale.x;
                float y = map.localScale.y;
                for (int i = 0; i < 35; i++)
                {
                    node[i] = new Node();
                    node[i].id = i;
                    node[i].things = "this is " + i + " node";
                    node[i].Xposition = node_obj[i].anchoredPosition.x * x;
                    node[i].Yposition = node_obj[i].anchoredPosition.y * y;
                    //之后添加其他的node逻辑
                }
                //我在代码里面使用，也可以在编辑器里面进行赋值，可能增加开销，tl
                node[0].connectedNodes = new int[] { 1, 2 };
                node[1].connectedNodes = new int[] { 3 };
                node[2].connectedNodes = new int[] { 6 };
                node[3].connectedNodes = new int[] { 4, 7 };
                node[4].connectedNodes = new int[] { 5 };
                node[5].connectedNodes = new int[] { 8 };
                node[6].connectedNodes = new int[] { 5, 9 };
                node[7].connectedNodes = new int[] { 10 };
                node[8].connectedNodes = new int[] { 13, 15 };
                node[9].connectedNodes = new int[] { 8 };
                node[10].connectedNodes = new int[] { 11 };
                node[11].connectedNodes = new int[] { 12 };
                node[12].connectedNodes = new int[] { 18 };
                node[13].connectedNodes = new int[] { 14 };
                node[14].connectedNodes = new int[] { 18 };
                node[15].connectedNodes = new int[] { 16 };
                node[16].connectedNodes = new int[] { 17 };
                node[17].connectedNodes = new int[] { 18 };
                node[18].connectedNodes = new int[] { 19, 22, 24 };
                node[19].connectedNodes = new int[] { 20 };
                node[20].connectedNodes = new int[] { 21 };
                node[21].connectedNodes = new int[] { };
                node[22].connectedNodes = new int[] { 23 };
                node[23].connectedNodes = new int[] { 27, 29 };
                node[24].connectedNodes = new int[] { 25 };
                node[25].connectedNodes = new int[] { 26 };
                node[26].connectedNodes = new int[] { 28 };
                node[27].connectedNodes = new int[] { 28 };
                node[28].connectedNodes = new int[] { 33 };
                node[29].connectedNodes = new int[] { 30 };
                node[30].connectedNodes = new int[] { 31 };
                node[31].connectedNodes = new int[] { 32 };
                node[32].connectedNodes = new int[] { 34 };
                node[33].connectedNodes = new int[] { 34 };
                node[34].connectedNodes = new int[] { };

            }
            playerController.ToNodeInstant(node[PlayerNature.currentNodeNum]);
        }
        public void ToNode(int nodenum)
        {
            //Explorer_ScreenController.CreateMessage("点击了按钮");
            //判断
            bool canreach = false;
            foreach (int j in node[PlayerNature.currentNodeNum].connectedNodes)
            {
                if (j == nodenum)
                {
                    canreach = true;
                    break;
                }
            }
            if (!canreach)
            {
                Explorer_ScreenController.CreateMessage("无法到达");
                return;
            }
            playerController.ToNode(node[PlayerNature.currentNodeNum], node[nodenum]);
        }
        public void Travel(string scenename)
        {
            SceneManager.LoadScene(scenename);
        }
    }
}
