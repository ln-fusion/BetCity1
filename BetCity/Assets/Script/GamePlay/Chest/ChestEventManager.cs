using BetCity.Core.ActionSystem;
using BetCity.Core.EventSystem;
using BetCity.Core.ProgressSystem;
using BetCity.Data.ConfigModels;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BetCity.GamePlay.Chest
{
    /// <summary>
    /// 宝箱事件管理器
    /// </summary>
    public class ChestEventManager : BaseEventManager<ChestEvent, ChestEventManager>
    {
        // 所有轮次的配置数据
        private List<RoundData<object>> allRoundDatas;

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




        public void StartChest(List<IRoundConfig> roundConfigs)
        {
            allRoundDatas = new List<RoundData<object>>();
            foreach (var config in roundConfigs)
            {
                // 从非泛型接口获取资源列表，限制每轮最多3个资源
                var resList = config.ResourceList.Count > 3 ? config.ResourceList.GetRange(0, 3) : config.ResourceList;
                allRoundDatas.Add(new RoundData<object>
                {
                    ResourceList = new List<object>(resList)
                });
            }
            currentRound = 0;
            Debug.Log($"总共{allRoundDatas.Count}轮");
            NextChoose();
        }
        public void NextChoose()
        {
            if(currentRound < allRoundDatas.Count)
            {
                //清除
                clickNum = -1;
                Debug.Log($"第{currentRound+1}轮开始");
                GameActionContext context = new(this, this, null);
                var chooseChest = new ChooseChestAction(context);
                ActionManager.Instance.Perform(chooseChest);
            }
        }
        public async UniTask WaitChoose(CancellationToken cancellationToken)
        {
            RoundData<object> roundData = allRoundDatas[currentRound];
            List<object> objectList = roundData.ResourceList;
            for(int i=0;i< objectList.Count; i++)
            {
                Buttons[i].SetActive(true);
                switch(objectList[i])
                {
                    case A:
                        A a = (A)objectList[i];
                        Texts[i].GetComponent<TextMeshProUGUI>().text = "A";
                        Buttons[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            a.Display();
                            clickNum = i;
                        });
                        break;
                    case B:
                        B b = (B)objectList[i];
                        Texts[i].GetComponent<TextMeshProUGUI>().text = "B";
                        Buttons[i].GetComponent<Button>().onClick.AddListener(() =>
                        {
                            b.Display();
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

        /// <summary>
        /// 单轮选择的运行时数据，内部运行的时候调用
        /// </summary>
        private class RoundData<T>
        {
            public List<T> ResourceList;
        }
    }
    /// <summary>
    /// 非泛型接口：统一所有RoundConfig的访问方式
    /// </summary>
    public interface IRoundConfig
    {
        // 非泛型的资源列表（所有类型都转为object）
        List<object> ResourceList { get; }
    }
    /// <summary>
    /// 单轮选择的配置，外部调用的时候传入
    /// </summary>
    [Serializable]
    public class RoundConfig<T>:IRoundConfig
    {
        // 本轮的资源列表（A/B/C/D类）
        public List<T> ResourceList;
        List<object> IRoundConfig.ResourceList
        {
            get
            {
                // 把 List<T> 转成 List<object>，满足接口要求
                return ResourceList.Cast<object>().ToList();
            }
        }
    }

}
