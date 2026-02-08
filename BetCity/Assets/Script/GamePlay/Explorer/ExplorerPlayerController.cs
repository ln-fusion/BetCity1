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
using UnityEditor;
using static UnityEditor.Timeline.TimelinePlaybackControls;

namespace BetCity.GamePlay.Explorer
{
    public class ExplorerPlayerController :MonoSingleton<ExplorerPlayerController>,ISubmitArchive<PlayerDataDTO>
    {
        /// <summary>
        /// 玩家物体，用于获取位置信息
        /// </summary>
        public GameObject Player {  get; private set; }
        private RectTransform playerTransform;
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

        Guid refreshPostSubGuid_1;
        Guid refreshPostSubGuid_2;
        Guid refreshPostSubGuid_3;
        Guid refreshPostSubGuid_4;
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

            //强制设置初值，不用事件系统
            //PlayerData.Load(StorageManager.ArchiveDataContainer.PlayerDataDTO);
            //ScreenController.printPlayerNature();
            //RefreshPlayerPosition();
            //refreshPostSubGuid = ActionManager.SubscribeReaction<ExplorerNodeChangeAction>(Renew, ReactionTiming.POST, 0);



            MapController.MapCreate();
            playerTransform = Player.GetComponent<RectTransform>();
            animator = Player.GetComponent<Animator>();
            ToNodeInstant(MapController.MapNode[PlayerData.CurrentNodeNum]);
            SaveArchive();
            refreshPostSubGuid_1 = ActionManager.SubscribeReaction<CurrentActionPointChangeAction>(Renew_1, ReactionTiming.POST, 0);
            refreshPostSubGuid_2 = ActionManager.SubscribeReaction<CurrentSanityChangeAction>(Renew_2, ReactionTiming.POST, 0);
            refreshPostSubGuid_3 = ActionManager.SubscribeReaction<CurrentNodeNumChangeAction>(Renew_3, ReactionTiming.POST, 0);
            refreshPostSubGuid_4 = ActionManager.SubscribeReaction<CoinChangeAction>(Renew_4, ReactionTiming.POST, 0);

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
        public void Renew_1(CurrentActionPointChangeAction display)
        {
            Debug.LogWarning("REFRESH");
            RenewScreen();
        }
        public void Renew_2(CurrentSanityChangeAction display)
        {
            Debug.LogWarning("REFRESH");
            RenewScreen();
        }
        public void Renew_3(CurrentNodeNumChangeAction display)
        {
            Debug.LogWarning("REFRESH");
            RenewScreen();
        }
        public void Renew_4(CoinChangeAction display)
        {
            Debug.LogWarning("REFRESH");
            RenewScreen();
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
        /// 玩家移动Action的实际逻辑-离开结点，写成异步是因为我感觉之后会用到await
        /// </summary>
        public async UniTask ExitNode(Node targetNode, CancellationToken cancellationToken)
        {
            PlayerStatus = 1;
            return;

        }
        /// <summary>
        /// 玩家移动Action的实际逻辑-进入新结点，写成异步是因为我感觉之后会用到await
        /// </summary>
        public async UniTask EnterNode(Node targetnode, CancellationToken cancellationToken)
        {

            ScreenController.ScreenFocus(targetnode);

            animator.SetBool("move", true);

            PlayerStatus = 0;
            return ;

        }

        /// <summary>
        /// 玩家移动Action的实际逻辑
        /// </summary>
        public async UniTask Move(Node targetnode, CancellationToken cancellationToken)
        {

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
            
            
        }
        /// <summary>
        /// 立刻更新玩家位置到指定Node
        /// </summary>
        public void ToNodeInstant(Node targetnode)
        {
            //此处不是Action，是load/作弊
            GameActionContext context = new(this, this, null);
            var currentNodeNumAction = new CurrentNodeNumChangeAction(context, targetnode.Id.Id);

            ActionManager.Instance.Perform(currentNodeNumAction);

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
        /*
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
        */
    }

}
