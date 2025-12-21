using BetCity.Core.ActionSystem;
using BetCity.Core.Tools;
using BetCity.Explorer;
using BetCity.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetCity.Explorer
{
    public class ExplorerPlayerController :MonoSingleton<ExplorerPlayerController>,ISubmitArchive<PlayerDTO>,IHasCoin,IHasCurrentSanity,IHasCurrentActionPoint
    {
        /// <summary>
        /// 玩家物体，用于获取位置信息
        /// </summary>
        public GameObject Player;
        private RectTransform _playerTransform;
        private static bool _initial = false;
        private Animator _animator;
        [Header("玩家状态")]
        /// <summary>
        /// 地图状态，也可以用枚举结构
        /// </summary>
        public static int PLAYER_STATUS = 0;//0空闲 1行走 2 丢骰子
        /// <summary>
        /// 接口，返回当前理智值
        /// </summary>
        public int CurrentSanity=>PlayerData.CurrentSanity;
        /// <summary>
        /// 接口，返回当前金币值
        /// </summary>
        public int Coin=>PlayerData.Coin;
        /// <summary>
        /// 接口，返回当前AP点
        /// </summary>
        public int CurrentActionPoint=>PlayerData.CurrentActionPoints;

        private ExplorerScreenController screenController;
        private ExplorerDiceController diceController;
        private ExplorerMapController mapController;
        private StorageManager storageManager;
        private ActionManager actionManager;
        private data.PlayerData PlayerData;


        private float MoveSpeed =300f;//玩家移动速度



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
            PlayerData = data.PlayerData.Instance;
            _playerTransform = Player.GetComponent<RectTransform>();
            _animator = Player.GetComponent<Animator>();
            if (!_initial)
            {
                //强制设置初值，不用事件系统
                _initial = true;
                PlayerData.MaxSanity = 20;
                PlayerData.CurrentSanity = 10;
                PlayerData.MaxActionPoints = 6;
                PlayerData.CurrentActionPoints = 0;
                PlayerData.CurrentNodeNum = 0;
                PlayerData.Coin = 10;
            }
            else
            {
                //强制设置初值，不用事件系统
                PlayerData.MaxSanity = storageManager.ArchiveData.PlayerDTO.MaxSanity;
                PlayerData.CurrentSanity = storageManager.ArchiveData.PlayerDTO.CurrentSanity;
                PlayerData.MaxActionPoints = storageManager.ArchiveData.PlayerDTO.MaxActionPoints;
                PlayerData.CurrentActionPoints = storageManager.ArchiveData.PlayerDTO.CurrentActionPoints;
                PlayerData.CurrentNodeNum = storageManager.ArchiveData.PlayerDTO.CurrentNodeNum;
                PlayerData.Coin = storageManager.ArchiveData.PlayerDTO.Coin;
                screenController.printPlayerNature();
                RefreshPlayerPosition();
            }
        }
        /// <summary>
        /// 从storagemanager中读取数据
        /// </summary>
        public void Lode()
        {
            PlayerData.MaxSanity = storageManager.ArchiveData.PlayerDTO.MaxSanity;
            PlayerData.CurrentSanity = storageManager.ArchiveData.PlayerDTO.CurrentSanity;
            PlayerData.MaxActionPoints = storageManager.ArchiveData.PlayerDTO.MaxActionPoints;
            PlayerData.CurrentActionPoints = storageManager.ArchiveData.PlayerDTO.CurrentActionPoints;
            PlayerData.CurrentNodeNum = storageManager.ArchiveData.PlayerDTO.CurrentNodeNum;
            PlayerData.Coin = storageManager.ArchiveData.PlayerDTO.Coin;
            screenController.printPlayerNature();
            RefreshPlayerPosition() ;
        }
        /// <summary>
        /// 立刻更新玩家位置，用于存档读取
        /// </summary>
        public void RefreshPlayerPosition()
        {
            ToNodeInstant(ExplorerMapController.MapNodes[ PlayerData.CurrentNodeNum]);
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
            if (PlayerData.CurrentActionPoints <= 0)
            {
                ExplorerScreenController.CreateMessage("AP点不足");
                yield break;
            }
            Node currentnode = ExplorerMapController.MapNodes[PlayerData.CurrentNodeNum];
            //判断目标节点是否可到达
            if (!mapController.CheckNode(currentnode.id,targetnode.id))
            {
                ExplorerScreenController.CreateMessage("无法到达");
                yield break;
            }
            UseActionPointChange(-1);
            PlayerData.CurrentNodeNum=targetnode.id;
            PLAYER_STATUS = 1;
            _animator.SetBool("move", true);

            Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) - new Vector2(currentnode.Xposition, currentnode.Yposition);
            Vector2 target = new Vector2(targetnode.Xposition, targetnode.Yposition) + new Vector2(-50, 50);
            Vector2 moveframe = movetarget.normalized;
            float distance = movetarget.magnitude;
            while (distance > 10)
            {
                _playerTransform.anchoredPosition += moveframe * MoveSpeed * Time.deltaTime;
                distance = Vector2.Distance(_playerTransform.anchoredPosition, target);
                yield return null;
            }
            _playerTransform.anchoredPosition = target;
            _animator.SetBool("move", false);
            screenController.printPlayerNature();
            yield return null;
            PLAYER_STATUS = 0;
        }
        /// <summary>
        /// 立刻更新玩家位置到指定Node
        /// </summary>
        public void ToNodeInstant(Node targetnode)
        {
            PlayerData.CurrentNodeNum = targetnode.id;
            _playerTransform.anchoredPosition = new Vector2(targetnode.Xposition - 50, targetnode.Yposition + 50);
        }
        #region 接口
        /// <summary>
        /// 更改当前理智值接口
        /// </summary>
        public bool ChangeCurrentSanity(int Sanity, CurrentSanityChangeAction currentSanityChangeAction)
        {
            if (PlayerData.CurrentSanity >= PlayerData.MaxSanity)
            {
                ExplorerScreenController.CreateMessage("理智值已满");
                PlayerData.CurrentSanity = PlayerData.MaxSanity;
                screenController.printPlayerNature();
            }
            else if (PlayerData.CurrentSanity >PlayerData.MaxSanity - Sanity)
            {
                PlayerData.CurrentSanity = PlayerData.MaxSanity;
                screenController.printPlayerNature();
            }
            else
            {
                PlayerData.CurrentSanity += Sanity;
                screenController.printPlayerNature();
            }
            return true;
        }
        /// <summary>
        /// 更改当前金币值接口
        /// </summary>
        public bool ChangeCoin(int coin, CoinChangeAction coinChangeAction)
        {
            PlayerData.Coin += coin;
            screenController.printPlayerNature();
            return true;
        }
        /// <summary>
        /// 更改当前AP点接口
        /// </summary>
        public bool ChangeCurrentActionPoint(int actionPoint, CurrentActionPointChangeAction currentActionPointChangeAction)
        {
            PlayerData.CurrentActionPoints += actionPoint;
            
            if (PlayerData.CurrentActionPoints >= PlayerData.MaxActionPoints)
            {
                PlayerData.CurrentActionPoints = PlayerData.MaxActionPoints;
            }
            else if (PlayerData.CurrentActionPoints < 0)
            {
                PlayerData.CurrentActionPoints = 0;
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
                PlayerData.MaxSanity,
                PlayerData.CurrentSanity,
                PlayerData.MaxActionPoints,
                PlayerData.CurrentActionPoints,
                PlayerData.CurrentNodeNum,
                PlayerData.Coin
                );
            //PlayerDTO playerDTO = new PlayerDTO();
            //playerDTO.MaxSanity = PlayerData.MaxSanity;
            //playerDTO.CurrentSanity= PlayerData.CurrentSanity;
            //playerDTO.MaxActionPoints = PlayerData.MaxActionPoints;
            //playerDTO.CurrentActionPoints = PlayerData.CurrentActionPoints;
            //playerDTO.CurrentNodeNum= PlayerData.CurrentNodeNum;
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
