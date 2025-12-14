using BetCity.Explorer;
using BetCity.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class PlayerNature
{
    public static int maxSanity {  get; private set; }
    public static int currentSanity { get; private set; }
    public static int maxActionPoints { get; private set; }
    public static int currentActionPoints { get; private set; }
    public static int currentNodeNum { get; private set; }
    public static void modifyMaxSanity(int i)
    {
        maxSanity += i;
    }
    public static void modifyCurrentSanity(int i)
    {
        currentSanity += i;
    }
    public static void modifyMaxActionPoints(int i)
    {
        maxActionPoints+= i;
    }
    public static void modifyCurrentActionPoints(int i)
    {
        currentActionPoints += i;
    }
    public static void modifyCurrentNodeNum(int i)
    {
        currentNodeNum = i;
    }
}
namespace BetCity.Explorer
{
    public class Explorer_PlayerController : MonoBehaviour,ISubmitArchive<Explorer_PlayerDTO>
    {
        public GameObject player;
        private RectTransform playertransform;
        private static bool Initial = false;
        private Animator animator;
        [Header("玩家状态")]
        public static int playerstatus = 0;
        //0空闲 1行走 2 丢骰子
        public Explorer_ScreenController screencontroller;
        public StorageManager storageManager;
        //move相关
        public float movespeed;
        private void Awake()
        {
            if (!Initial)
            {
                Initial = true;
                PlayerNature.modifyMaxSanity(20);
                PlayerNature.modifyCurrentSanity(10);
                PlayerNature.modifyMaxActionPoints(10);
                PlayerNature.modifyCurrentActionPoints(0);
                PlayerNature.modifyCurrentNodeNum(0);
            }
            playertransform = player.GetComponent<RectTransform>();
            animator = player.GetComponent<Animator>();
        }
        void Start()
        {

        }
        public void ToNode(Node currentnode, Node targetnode)
        {
            if (playerstatus == 0)
            {
                if (PlayerNature.currentActionPoints > 0)
                {
                    PlayerNature.modifyCurrentActionPoints(-1);
                    StartCoroutine(Move(currentnode, targetnode));
                }
                else
                {
                    Explorer_ScreenController.CreateMessage("AP点不足");
                    return;
                }

            }
            else
            {
                Explorer_ScreenController.CreateMessage("当前无法操作");
            }
        }
        public IEnumerator Move(Node currentnode, Node targetnode)
        {
            PlayerNature.modifyCurrentNodeNum(targetnode.id);
            playerstatus = 1;
            animator.SetBool("move", true);

            Vector2 movetarget = new Vector2(targetnode.Xposition, targetnode.Yposition) - new Vector2(currentnode.Xposition, currentnode.Yposition);
            Vector2 target = new Vector2(targetnode.Xposition, targetnode.Yposition) + new Vector2(-50, 50);
            Vector2 moveframe = movetarget.normalized;
            float distance = movetarget.magnitude;
            while (distance > 10)
            {
                playertransform.anchoredPosition += moveframe * movespeed * Time.deltaTime;
                distance = Vector2.Distance(playertransform.anchoredPosition, target);
                yield return null;
            }
            playertransform.anchoredPosition = target;
            animator.SetBool("move", false);
            screencontroller.printPlayerNature();
            yield return null;
            playerstatus = 0;
        }
        public void ToNodeInstant(Node targetnode)
        {
            PlayerNature.modifyCurrentNodeNum(targetnode.id);
            playertransform.anchoredPosition = new Vector2(targetnode.Xposition - 50, targetnode.Yposition + 50);
        }
        public void addap()
        {
            if (PlayerNature.currentActionPoints < PlayerNature.maxActionPoints)
            {
                PlayerNature.modifyCurrentActionPoints(1);
                screencontroller.printPlayerNature();
            }
            else
            {
                Explorer_ScreenController.CreateMessage("AP点已满");
            }
        }
        public void addsan()
        {
            if (PlayerNature.currentSanity < PlayerNature.maxSanity)
            {
                PlayerNature.modifyCurrentSanity(1);
                screencontroller.printPlayerNature();
            }
            else
            {
                Explorer_ScreenController.CreateMessage("理智值已满");
            }
        }

        #region 存储
        /// <summary>
        ///提交保存申请
        /// </summary>
        private void SaveArchive()
        {
            List<Explorer_PlayerDTO> saveData = new List<Explorer_PlayerDTO>();
            Explorer_PlayerDTO playerdatadto=new Explorer_PlayerDTO(
                PlayerNature.maxSanity,
                PlayerNature.currentSanity,
                PlayerNature.maxActionPoints,
                PlayerNature.currentActionPoints,
                PlayerNature.currentNodeNum
                );
            saveData.Add(playerdatadto);
            SubmitArchive(saveData);
        }
        /// <summary>
        /// 【公开接口】手动触发保存（外部可调用，比如游戏退出/存档点）
        /// </summary>
        public void ManualSave()
        {
            SaveArchive();
        }
        public void SubmitArchive(List<Explorer_PlayerDTO> dTOs)
        {
            storageManager.ModifyArchive(dTOs, this);
        }
        #endregion
    }

}
