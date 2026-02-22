using BetCity.Core.ActionSystem;
using BetCity.Core.CheckSystem;
using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Core.Tools;
using BetCity.Data.ConfigModels;
using BetCity.GamePlay.Souvenir;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
namespace BetCity.GamePlay.Chest
{
    /// <summary>
    /// 宝箱事件管理器
    /// </summary>
    public class ChestEventManager : BaseEventManager<ChestEvent, ChestEventManager>
    {
        //商店系统中不会刷新已拥有纪念品，此条不用写在condition里
        private IReadOnlyDictionary<int, Souvenir.Souvenir> ownedSouvenirs => SouvenirManager.Instance.OwnedSouvenirs;
        private ConditionChecker conditionChecker => ConditionChecker.Instance;
        /// <summary>
        /// 商店事件
        /// </summary>
        public IReadOnlyDictionary<int, ChestEvent> chestEvents => EventLoader.Instance.ChestEvents;
        // 所有轮次的配置数据
        private List<ChestOptionSet> chestOptionSet;

        //当前轮数
        private int currentRound=0;

        //临时按钮
        public GameObject[] Buttons;
        private GameObject[] Texts;
        //用于区别点击的数字
        private int clickNum = 0;
        protected override void Awake()
        {
            base.Awake();

            if (Buttons.Length != 3)
            {
                Debug.LogError("[ChestEventManager]的按钮配置未完成");
            }
            else
            {
                Texts = new GameObject[3];
                for (int i = 0; i < 3; i++)
                {
                    Texts[i]=Buttons[i].transform.GetChild(0).gameObject;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                Buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
                Buttons[i].SetActive(false);
            }
        }
        public void NextChoose()
        {
            if(currentRound < chestOptionSet.Count)
            {
                //清除
                clickNum = -1;
                Debug.Log($"第{currentRound+1}轮开始");
                CurrentEventState = "Round"+(currentRound+1);
                GameActionContext context = new(this, this, null);
                var chooseChest = new ChooseChestAction(context);
                ActionManager.Instance.Perform(chooseChest);
            }
        }
        public async UniTask WaitChoose(CancellationToken cancellationToken)
        {
            //绑定按钮事件
            ChestOptionSet chestOption = chestOptionSet[currentRound];
            for (int i = 0; i < chestOption.ChoiceOptions.Count; i++)
            {
                Buttons[i].SetActive(true);
                switch (chestOption.ChoiceOptions[i].ItemType)
                {
                    case ItemType.Souvenir:

                        Texts[i].GetComponent<TextMeshProUGUI>().text = "A";
                        Buttons[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            //内容
                            clickNum = i;
                        });
                        break;
                    case ItemType.Card:
                        Texts[i].GetComponent<TextMeshProUGUI>().text = "B";
                        Buttons[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            //内容
                            clickNum = i;
                        });

                        break;
                }


            }

            while (true)
            {
                if (clickNum>0)
                {
                    Debug.Log("检测到点击，结束循环");
                    for (int i = 0; i < 3; i++)
                    {
                        Buttons[i].GetComponent<Button>().onClick.RemoveAllListeners();
                        Buttons[i].SetActive(false);
                    }
                    currentRound++;
                    return;
                }
                Debug.Log("等待中");

                //等待一帧
                await UniTask.Yield();
            }
        }
        private List<ChestOptionSet> CheckCondition()
        {
            List<ChestOptionSet> legalChestOptionSet = new List<ChestOptionSet>();
            foreach (ChestOptionSet chestOptionSet in CurrentEvent.ChoiceOptionSets)
            {
                ChestOptionSet optionSet = new ChestOptionSet();
                optionSet.ChoiceOptions = new List<ChestOption>();
                int loadOptionNum = 0;//记录加载的选项数，保证不超过三个
                foreach(ChestOption chestOption in chestOptionSet.ChoiceOptions)
                {
                    if (loadOptionNum >= 3)
                    {
                        break;
                    }
                    switch (chestOption.ItemType)
                    {
                        case ItemType.Souvenir:
                            if (ownedSouvenirs.ContainsKey(chestOption.ChestId))
                            {
                                continue;
                            }
                            else if (conditionChecker.Check(chestOption.Conditions.Init()))
                            {
                                optionSet.ChoiceOptions.Add(chestOption);
                                loadOptionNum++;
                                continue;
                            }
                            else continue;
                        //商店暂时不存卡牌
                        case ItemType.Card:
                            continue;
                    }
                }
                if(optionSet.ChoiceOptions.Count > 0)
                {
                    legalChestOptionSet.Add(optionSet);
                }
            }
            return legalChestOptionSet;
        }
        private void LoadChoice()
        {
            chestOptionSet = CheckCondition();
            currentRound = 0;
            Debug.Log($"总共{chestOptionSet.Count}轮");
            CurrentEventState = "Start";
            NextChoose();
        }
        #region 接口
        /// <summary>
        /// OnEnterChestNode触发该函数，将CurrentEventState设置为Start
        /// </summary>
        /// <param name="id">id</param>
        public override UniTask EnterEvent(CancellationToken cancellationToken, int id)
        {
            if (chestEvents.TryGetValue(id, out ChestEvent chestEvent))
            {
                base.EnterEvent(cancellationToken, id);
                CurrentEvent = chestEvent;
            }
            else
            {
                Debug.LogError($"[ChestEventManager]对应Id为{id}的宝箱事件不存在！");
                CurrentEventState = "None";
                return UniTask.CompletedTask;
            }
            LoadChoice();
            NextChoose();
            return UniTask.CompletedTask;
        }
        /// <summary>
        /// OnEnterChestNode触发该函数，判断宝箱是否有可选项
        /// </summary>
        public bool CheckChestOption(CancellationToken cancellationToken)
        {
            if(chestOptionSet.Count > 0)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
