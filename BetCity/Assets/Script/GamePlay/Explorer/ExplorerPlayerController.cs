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
    public class ExplorerPlayerController :MonoSingleton<ExplorerPlayerController>,ISubmitArchive<PlayerDTO>
    {
        public GameObject Player;
        private RectTransform _playerTransform;
        private static bool _initial = false;
        private Animator _animator;
        [Header("玩家状态")]
        public static int PLAYER_STATUS = 0;
        public data.PlayerData PlayerData;

        //0空闲 1行走 2 丢骰子
        public ExplorerScreenController ScreenController;
        public ExplorerDiceManager DiceManager;
        public StorageManager StorageManager;
        //move相关
        public float MoveSpeed;
        protected override void Awake()
        {
            base.Awake();
            if (!_initial)
            {
                _initial = true;
                PlayerData.MaxSanity=20;
                PlayerData.CurrentSanity = 10;
                PlayerData.MaxActionPoints = 6;
                PlayerData.CurrentActionPoints = 0;
                PlayerData.CurrentNodeNum=0;
            }
            else
            {
                PlayerData.MaxSanity = StorageManager.ArchiveData.PlayerDTO.MaxSanity;
                PlayerData.CurrentSanity = StorageManager.ArchiveData.PlayerDTO.CurrentSanity;
                PlayerData.MaxActionPoints = StorageManager.ArchiveData.PlayerDTO.MaxActionPoints;
                PlayerData.CurrentActionPoints = StorageManager.ArchiveData.PlayerDTO.CurrentActionPoints;
                PlayerData.CurrentNodeNum = StorageManager.ArchiveData.PlayerDTO.CurrentNodeNum;
                ScreenController.printPlayerNature();
            }
            _playerTransform = Player.GetComponent<RectTransform>();
            _animator = Player.GetComponent<Animator>();
        }
        void Start()
        {

        }
        public void ToNode(Node currentnode, Node targetnode)
        {
            if (PLAYER_STATUS == 0)
            {
                if (PlayerData.CurrentActionPoints > 0)
                {
                    PlayerData.CurrentActionPoints-=1;
                    DiceManager.APMinus();
                    StartCoroutine(Move(currentnode, targetnode));
                }
                else
                {
                    ExplorerScreenController.CreateMessage("AP点不足");
                    return;
                }

            }
            else
            {
                ExplorerScreenController.CreateMessage("当前无法操作");
            }
        }
        public IEnumerator Move(Node currentnode, Node targetnode)
        {
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
            ScreenController.printPlayerNature();
            yield return null;
            PLAYER_STATUS = 0;
        }
        public void ToNodeInstant(Node targetnode)
        {
            PlayerData.CurrentNodeNum = targetnode.id;
            _playerTransform.anchoredPosition = new Vector2(targetnode.Xposition - 50, targetnode.Yposition + 50);
        }
        public void addap()
        {
            if (PlayerData.CurrentActionPoints < PlayerData.MaxActionPoints)
            {
                PlayerData.CurrentActionPoints+=1;
                ScreenController.printPlayerNature();
            }
            else
            {
                ExplorerScreenController.CreateMessage("AP点已满");
            }
        }
        public void addsan()
        {
            if (PlayerData.CurrentSanity < PlayerData.MaxSanity)
            {
                PlayerData.CurrentSanity+=1;
                ScreenController.printPlayerNature();
            }
            else
            {
                ExplorerScreenController.CreateMessage("理智值已满");
            }
        }

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
                PlayerData.CurrentNodeNum
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
        public void SubmitArchive(List<PlayerDTO> dTOs)
        {
            StorageManager.ModifyArchive(dTOs, this);
        }
        #endregion
    }

}
