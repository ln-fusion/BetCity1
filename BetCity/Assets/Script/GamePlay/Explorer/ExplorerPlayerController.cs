using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetCity.GamePlay.Explorer
{
    public class ExplorerPlayerController :MonoSingleton<ExplorerPlayerController>,ISubmitArchive<PlayerDTO>,IHasCoin
    {
        /// <summary>
        /// 玩家物体，用于获取位置信息
        /// </summary>
        public GameObject Player;
        private RectTransform playerTransform;
        private static bool _initial = false;
        private Animator animator;
        [Header("玩家状态")]
        /// <summary>
        /// 地图状态，也可以用枚举结构
        /// </summary>
        public static int PLAYER_STATUS = 0;//0空闲 1行走 2 丢骰子
        /// <summary>
        /// 接口，返回当前金币值
        /// </summary>
        public int Coin=>playerData.Coin;

        private ExplorerScreenController screenController;
        private ExplorerDiceController diceController;
        private ExplorerMapController mapController;
        private StorageManager storageManager;
        private ActionManager actionManager;
        private data.PlayerData playerData;


        private float MoveSpeed =300f;//玩家移动速度
        // 玩家显示点位
        Vector2 showPosition = new Vector2(-20, 20);

        protected override void Awake()
        {
            base.Awake();
        }
        void Start()
        {
            screenController=ExplorerScreenController.Instance;
            diceController=ExplorerDiceController.Instance;
            storageManager=StorageManager.Instance;
            actionManager=ActionManager.Instance;
            mapController=ExplorerMapController.Instance;
            playerData = data.PlayerData.Instance;
            if (!_initial)
            {
                //强制设置初值，不用事件系统
                _initial = true;
                playerData.MaxSanity = 20;
                playerData.CurrentSanity = 10;
                playerData.MaxActionPoints = 6;
                playerData.CurrentActionPoints = 0;
                playerData.CurrentNodeNum = 0;
                playerData.Coin = 10;
                playerData.MapID = 0;
            }
            else
            {
                //强制设置初值，不用事件系统
                playerData.MaxSanity = storageManager.ArchiveData.PlayerDTO.MaxSanity;
                playerData.CurrentSanity = storageManager.ArchiveData.PlayerDTO.CurrentSanity;
                playerData.MaxActionPoints = storageManager.ArchiveData.PlayerDTO.MaxActionPoints;
                playerData.CurrentActionPoints = storageManager.ArchiveData.PlayerDTO.CurrentActionPoints;
                playerData.CurrentNodeNum = storageManager.ArchiveData.PlayerDTO.CurrentNodeNum;
                playerData.Coin = storageManager.ArchiveData.PlayerDTO.Coin;
                playerData.MapID = storageManager.ArchiveData.PlayerDTO.MapID;
                screenController.printPlayerNature();
                RefreshPlayerPosition();
            }
            mapController.MapCreate();
            playerTransform = Player.GetComponent<RectTransform>();
            animator = Player.GetComponent<Animator>();
            ToNodeInstant(ExplorerMapController.MapNode[playerData.CurrentNodeNum]);
        }
        /// <summary>
        /// 从storagemanager中读取数据
        /// </summary>
        public void Lode()
        {
            playerData.MaxSanity = storageManager.ArchiveData.PlayerDTO.MaxSanity;
            playerData.CurrentSanity = storageManager.ArchiveData.PlayerDTO.CurrentSanity;
            playerData.MaxActionPoints = storageManager.ArchiveData.PlayerDTO.MaxActionPoints;
            playerData.CurrentActionPoints = storageManager.ArchiveData.PlayerDTO.CurrentActionPoints;
            playerData.CurrentNodeNum = storageManager.ArchiveData.PlayerDTO.CurrentNodeNum;
            playerData.Coin = storageManager.ArchiveData.PlayerDTO.Coin;
            playerData.MapID=storageManager.ArchiveData.PlayerDTO.MapID;
            screenController.printPlayerNature();
            RefreshPlayerPosition() ;
        }
        /// <summary>
        /// 立刻更新玩家位置，用于存档读取
        /// </summary>
        public void RefreshPlayerPosition()
        {
            ToNodeInstant(ExplorerMapController.MapNode[ playerData.CurrentNodeNum]);
        }
        /// <summary>
        /// 玩家移动Action的实际逻辑
        /// </summary>
        public IEnumerator Move(Node targetnode)
        {
            //判断当前是否能进行操作
            if (PLAYER_STATUS != 0)
            {
                ExplorerScreenController.CreateMessage("当前无法操作");
                yield break;
            }
            //判断玩家AP点
            if (playerData.CurrentActionPoints <= 0)
            {
                ExplorerScreenController.CreateMessage("AP点不足");
                yield break;
            }
            Node currentnode = ExplorerMapController.MapNode[playerData.CurrentNodeNum];
            //判断目标节点是否可到达
            if (!mapController.CheckNode(currentnode.id,targetnode.id))
            {
                ExplorerScreenController.CreateMessage("无法到达");
                yield break;
            }
            screenController.ScreenFocus(targetnode);

            UseActionPointChange(-1);
            playerData.CurrentNodeNum=targetnode.id;
            PLAYER_STATUS = 1;
            animator.SetBool("move", true);


            Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) + showPosition;
            Vector2 moveframe = (movetarget- playerTransform.anchoredPosition).normalized;
            float distance = Vector2.Distance(playerTransform.anchoredPosition, new Vector2(targetnode.Xposition, targetnode.Yposition) + showPosition);
            while (distance > 10)
            {
                playerTransform.anchoredPosition += moveframe * MoveSpeed * Time.deltaTime;
                distance = Vector2.Distance(playerTransform.anchoredPosition, new Vector2(targetnode.Xposition, targetnode.Yposition) + showPosition);
                yield return null;
            }
            playerTransform.anchoredPosition = new Vector2(targetnode.Xposition, targetnode.Yposition) + showPosition;
            animator.SetBool("move", false);
            screenController.printPlayerNature();
            yield return null;
            PLAYER_STATUS = 0;
        }
        /// <summary>
        /// 立刻更新玩家位置到指定Node
        /// </summary>
        public void ToNodeInstant(Node targetnode)
        {
            playerData.CurrentNodeNum = targetnode.id;
            playerTransform.anchoredPosition = new Vector2(targetnode.Xposition, targetnode.Yposition)+showPosition;
            screenController.ScreenFocusInstant(targetnode);
        }
        #region 接口
        /// <summary>
        /// 更改当前理智值接口
        /// </summary>
        public bool ChangeCurrentSanity(int Sanity, CurrentSanityChangeAction currentSanityChangeAction)
        {
            if (playerData.CurrentSanity >= playerData.MaxSanity)
            {
                ExplorerScreenController.CreateMessage("理智值已满");
                playerData.CurrentSanity = playerData.MaxSanity;
                screenController.printPlayerNature();
            }
            else if (playerData.CurrentSanity >playerData.MaxSanity - Sanity)
            {
                playerData.CurrentSanity = playerData.MaxSanity;
                screenController.printPlayerNature();
            }
            else
            {
                playerData.CurrentSanity += Sanity;
                screenController.printPlayerNature();
            }
            return true;
        }
        /// <summary>
        /// 更改当前金币值接口
        /// </summary>
        public bool ChangeCoin(int coin, CoinChangeAction coinChangeAction)
        {
            playerData.Coin += coin;
            screenController.printPlayerNature();
            return true;
        }
        /// <summary>
        /// 更改当前AP点接口
        /// </summary>
        public bool ChangeCurrentActionPoint(int actionPoint, CurrentActionPointChangeAction currentActionPointChangeAction)
        {
            playerData.CurrentActionPoints += actionPoint;
            
            if (playerData.CurrentActionPoints >= playerData.MaxActionPoints)
            {
                playerData.CurrentActionPoints = playerData.MaxActionPoints;
            }
            else if (playerData.CurrentActionPoints < 0)
            {
                playerData.CurrentActionPoints = 0;
            }
            screenController.printPlayerNature();
            diceController.APRefresh();
            return true;
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
            var currentSanityAction=new CurrentSanityChangeAction(context,i);
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
        /// 创建更改当前节点位置动作
        /// </summary>
        public void UseNodeChange( Node targetnode)
        {
            GameActionContext context = new(this, this, null);
            var currentNodeChange = new ExplorerNodeChangeAction(context,targetnode);
            ActionManager.Instance.Perform(currentNodeChange);
        }
        #endregion
        #region 存储
        /// <summary>
        ///提交保存申请
        /// </summary>
        private void SaveArchive()
        {
            List<PlayerDTO> saveData = new List<PlayerDTO>();
            PlayerDTO playerDTO = new PlayerDTO(
                playerData.MaxSanity,
                playerData.CurrentSanity,
                playerData.MaxActionPoints,
                playerData.CurrentActionPoints,
                playerData.CurrentNodeNum,
                playerData.Coin,
                playerData.MapID
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
        public void SubmitArchive(List<PlayerDTO> dTOs)
        {
            storageManager.ModifyArchive(dTOs, this);
        }
        #endregion
    }

}
