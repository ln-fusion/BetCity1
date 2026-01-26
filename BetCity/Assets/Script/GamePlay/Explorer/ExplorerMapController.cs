using BetCity.Core.Tools;
using System.Collections.Generic;
using BetCity.Data.ConfigModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Diagnostics;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 地图控制器
    /// </summary>
    public class ExplorerMapController : MonoSingleton<ExplorerMapController>
    {
        /// <summary>
        /// 存入当前所有结点的类信息
        /// </summary>
        public Node[] MapNode { get; private set; }
        /// <summary>
        /// 存入所有结点的位置信息
        /// </summary>
        public RectTransform[] NodeTransform { get; private set; }
        /// <summary>
        /// 获取场景中的画布
        /// </summary>
        [field: SerializeField]
        public RectTransform Canvas { get; private set; }
        /// <summary>
        /// 获取场景中的地图父物体
        /// </summary>
        [field: SerializeField] 
        public RectTransform MapTransform { get; private set; }
        private bool _initial = false;
        private ExplorerPlayerController PlayerController => ExplorerPlayerController.Instance;
        private ExplorerResourceController ResourceController => ExplorerResourceController.Instance;
        private ExplorerScreenController ScreenController => ExplorerScreenController.Instance;
        private PlayerData PlayerData=>PlayerController.PlayerData;
        /// <summary>
        /// 实例化后的地图
        /// </summary>
        private MapData CurrentMapData { get; set; }
        private GameObject currentMap;


        // 初始化地图
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            if (!_initial)
            {
                _initial = true;
                //RectTransform map = NodeObj[0].transform.parent.parent.gameObject.GetComponent<RectTransform>();
                //float x = map.localScale.x;
                //float y = map.localScale.y;
            }
            
        }
        /// <summary>
        /// 创建地图
        /// </summary>
        public void MapCreate()
        {
            GameObject mapPrefab = ResourceController.GetMap(PlayerData.MapID);

            currentMap = Instantiate(mapPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
            currentMap.GetComponent<RectTransform>().SetParent(MapTransform.GetComponent<RectTransform>(), false);
            RectTransform uiRect = currentMap.GetComponent<RectTransform>();
            uiRect.anchoredPosition = new Vector2(0, 0);
            CurrentMapData=currentMap.GetComponent<MapData>();
            CurrentMapData.MapInital(this);
            MapNode = CurrentMapData.MapNode;
            //NodeTransform = CurrentMapData.NodeObject;
            //player
            GameObject player= Instantiate(ResourceController.Player, Vector3.zero, Quaternion.Euler(Vector3.zero));
            player.GetComponent<RectTransform>().SetParent(currentMap.GetComponent<RectTransform>(), false);
            PlayerController.SetPlayer(player);
            ScreenController.Mapintial(currentMap);
        }
        /// <summary>
        /// 按钮绑定函数，前往对应结点
        /// </summary>
        public void ToNode(int nodenum)
        {
            PlayerController.UseNodeChange(MapNode[PlayerData.CurrentNodeNum] ,MapNode[nodenum]);
        }
        /// <summary>
        /// 检查结点是否可到达
        /// </summary>
        public bool CheckNode(int currentnodenum,int targetnodenum)
        {
            foreach (int j in MapNode[currentnodenum].connectedNodes)
            {
                if (j == targetnodenum)
                {
                    return true;
                }
            }
            //无法到达
            return false;
        }
        /// <summary>
        /// 废弃函数，暂时不用
        /// </summary>
        public void Travel(string scenename)
        {
            Debug.Log($"切换到空白场景");
            PlayerController.ManualSave();
            SceneManager.LoadScene(scenename);
        }
    }
}
