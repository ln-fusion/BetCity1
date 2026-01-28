using BetCity.Core.ActionSystem;
using BetCity.Data.ConfigModels;
using BetCity.Core.Tools;
using BetCity.Data.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BetCity.GamePlay.Explorer
{
    public class ExplorerPlayerController :MonoSingleton<ExplorerPlayerController>,ISubmitArchive<PlayerDataDTO>
    {
        /// <summary>
        /// 玩家物体，用于获取位置信息
        /// </summary>
        public GameObject Player {  get; private set; }
        private RectTransform playerTransform;
        private bool _initial = false;
        private Animator animator;
        [Header("玩家状态")]
        /// <summary>
        /// 地图状态，也可以用枚举结构
        /// </summary>
        public int PlayerStatus = 0;//0空闲 1行走 2 丢骰子
        /// <summary>
        /// 接口，返回当前金币值
        /// </summary>
        public int Coin=>PlayerData.CurrentCoin;

        private ExplorerScreenController ScreenController => ExplorerScreenController.Instance;
        private ExplorerDiceController DiceController=>ExplorerDiceController.Instance;
        private ExplorerMapController MapController=>ExplorerMapController.Instance;
        private StorageManager StorageManager => StorageManager.Instance;
        private ActionManager ActionManager=>ActionManager.Instance;
        public PlayerData PlayerData {  get; private set; }


        private const float MOVE_SPEED=300f;//玩家移动速度
        // 玩家显示点位
        Vector2 showPosition { get; set; } = new Vector2(-20, 20);

        protected override void Awake()
        {
            base.Awake();
            PlayerData = new PlayerData();
        }
        void Start()
        {

            
            if (!_initial)
            {
                //强制设置初值，不用事件系统
                _initial = true;
                
            }
            else
            {
                
                //强制设置初值，不用事件系统
                PlayerData.Load(StorageManager.ArchiveDataContainer.PlayerDataDTO);
                ScreenController.printPlayerNature();
                RefreshPlayerPosition();
            }
            
            MapController.MapCreate();
            playerTransform = Player.GetComponent<RectTransform>();
            animator = Player.GetComponent<Animator>();
            ToNodeInstant(MapController.MapNode[PlayerData.CurrentNodeNum]);
            SaveArchive();
        }
        public void SetPlayer(GameObject player)
        {
            Player= player;
        }
        /// <summary>
        /// 从storagemanager中读取数据
        /// </summary>
        public void Lode()
        {
            PlayerData.Load(StorageManager.ArchiveDataContainer.PlayerDataDTO);
            ScreenController.printPlayerNature();
            RefreshPlayerPosition() ;
        }
        /// <summary>
        /// 刷新面板显示
        /// </summary>
        public void RenewScreen()
        {
            ScreenController.printPlayerNature();
        }
        /// <summary>
        /// 立刻更新玩家位置，用于存档读取
        /// </summary>
        public void RefreshPlayerPosition()
        {
            ToNodeInstant(MapController.MapNode[ PlayerData.CurrentNodeNum]);
        }
        public bool NodeJudge(Node sourcenode, Node targetnode)
        {
            Debug.LogWarning("Judge");
            // 判断当前是否能进行操作
            if (PlayerStatus != 0)
            {
                //当前无法操作
                Debug.LogWarning("[" + this.name + "]当前无法操作，无法进行结点移动");
                return false ;
            }

            // 判断玩家AP点
            if (PlayerData.CurrentActionPoints <= 0)
            {
                //AP点不足
                Debug.LogWarning("[" + this.name + "]AP点不足，无法进行结点移动");
                return false;
            }

            Node currentnode = MapController.MapNode[PlayerData.CurrentNodeNum];
            //判断目标节点是否可到达
            if (!MapController.CheckNode(currentnode.Id.Id, targetnode.Id.Id))
            {

                //无法到达
                Debug.LogWarning("[" + this.name + "]目标结点与当前结点不连接，无法进行结点移动");
                return false;
            }


            return true;
        }
        /// <summary>
        /// 玩家移动Action的实际逻辑
        /// </summary>
        public  UniTask ExitNode(Node targetNode)
        {
            Debug.LogWarning("Exit");
            PlayerStatus = 1;
            return UniTask.CompletedTask;

        }
        /// <summary>
        /// 玩家移动Action的实际逻辑
        /// </summary>
        public UniTask EnterNode(Node targetnode)
        {
            Debug.LogWarning("Enter");

            ScreenController.ScreenFocus(targetnode);

            UseActionPointChange(-1);
            UseNodeNumChange(targetnode.Id.Id);
            PlayerStatus = 1;
            animator.SetBool("move", true);

            UseNodeNumChange(targetnode.Id.Id);
            PlayerStatus = 0;
            return UniTask.CompletedTask;

        }

        /// <summary>
        /// 玩家移动Action的实际逻辑
        /// </summary>
        public async UniTask Move(Node targetnode)
        {
            Debug.LogWarning("move");

            animator.SetBool("move", true);

            Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) + showPosition;
            Vector2 moveframe = (movetarget - playerTransform.anchoredPosition).normalized;
            float distance = Vector2.Distance(playerTransform.anchoredPosition, movetarget); // 优化：复用movetarget，减少重复计算

            // 移动循环逻辑
            while (distance > 10)
            {
                playerTransform.anchoredPosition += moveframe * MOVE_SPEED * Time.deltaTime;
                distance = Vector2.Distance(playerTransform.anchoredPosition, movetarget); // 优化：复用movetarget
                await UniTask.Yield(); // 替代yield return null，等待下一帧
            }

            // 修正位置到目标点
            playerTransform.anchoredPosition = movetarget;
            animator.SetBool("move", false);
            

            await UniTask.Yield(); // 替代yield return null，等待下一帧
            
        }
        /// <summary>
        /// 立刻更新玩家位置到指定Node
        /// </summary>
        public void ToNodeInstant(Node targetnode)
        {
            UseNodeNumChange(targetnode.Id.Id);
            playerTransform.anchoredPosition = new Vector2(targetnode.Xposition, targetnode.Yposition)+showPosition;
            ScreenController.ScreenFocusInstant(targetnode);
        }

        #region 存储
        /// <summary>
        ///提交保存申请
        /// </summary>
        private void SaveArchive()
        {
            List<PlayerDataDTO> saveData = new List<PlayerDataDTO>();
            PlayerDataDTO playerDTO = new PlayerDataDTO(
                PlayerData.MaxSanity,
                PlayerData.CurrentSanity,
                PlayerData.MaxActionPoints,
                PlayerData.CurrentActionPoints,
                PlayerData.CurrentNodeNum,
                PlayerData.Coin,
                PlayerData.MapID,
                PlayerData.SouvenirMaxSlot,
                (int[])PlayerData.Dice.Clone()
            
                );
            saveData.Add(playerDTO);
            SubmitArchive(saveData);
        }
        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
        }
        /// <summary>
        /// 提交存档申请到storagemanager
        /// </summary>
        public void SubmitArchive(List<PlayerDataDTO> dTOs)
        {
            StorageManager.ModifyArchive(dTOs, this);
        }
        #endregion
        #region 调用接口函数
        /// <summary>
        /// 创建更改金币动作
        /// </summary>
        public void UseCoinChange(int i)
        {
            GameActionContext context = new(this, this, null);
            var CoinAction = new CoinChangeAction(context, i);
            ActionManager.Instance.Perform(CoinAction);
        }
        /// <summary>
        /// 创建更改当前理智值动作
        /// </summary>
        public void UseSanityChange(int i)
        {
            GameActionContext context = new(this, this, null);
            var currentSanityAction = new CurrentSanityChangeAction(context, i);
            ActionManager.Instance.Perform(currentSanityAction);
        }
        /// <summary>
        /// 创建更改当前AP点动作
        /// </summary>
        public void UseActionPointChange(int i)
        {
            GameActionContext context = new(this, this, null);
            var currentActionPointAction = new CurrentActionPointChangeAction(context, i);

            ActionManager.Instance.Perform(currentActionPointAction);
        }
        /// <summary>
        /// 更改当前结点编号接口
        /// </summary>
        public void UseNodeNumChange(int i)
        {
            GameActionContext context = new(this, this, null);
            var currentNodeNumAction = new CurrentNodeNumChangeAction(context, i);

            ActionManager.Instance.Perform(currentNodeNumAction);
        }
        /// <summary>
        /// 创建更改当前节点位置动作
        /// </summary>
        public void UseNodeChange(Node sourcenode, Node targetnode)
        {
            GameActionContext context = new(sourcenode, targetnode, null);
            var currentNodeChange = new ExplorerNodeChangeAction(context);
            ActionManager.Instance.Perform(currentNodeChange);

        }
        /// <summary>
        /// 创建更改当前最大纪念品动作
        /// </summary>
        public void UseSouvenirMaxChange(int i)
        {
            GameActionContext context = new(this, this, null);
            var soucenirMaxSlotAction = new SouvenirMaxSlotChangeAction(context, i);

            ActionManager.Instance.Perform(soucenirMaxSlotAction);
        }
        #endregion
    }

}
