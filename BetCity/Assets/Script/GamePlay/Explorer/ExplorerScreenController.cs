using BetCity.Core.Tools;
using BetCity.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.GamePlay.Explorer
{
    /// <summary>
    /// 适配不同大小的屏幕
    /// </summary>
    public class ExplorerScreenController : MonoSingleton<ExplorerScreenController>
    {
        private float _screenHeight;
        private float _screenWidth;
        /// <summary>
        /// 获取地图位置信息，用于适配屏幕
        /// </summary>
        public RectTransform Map;
        /// <summary>
        /// 获取玩家位置信息，用于处理移动逻辑
        /// </summary>
        public RectTransform Player;
        /// <summary>
        /// 获取画布信息，用于适配屏幕
        /// </summary>
        public Canvas canvas;
        private static bool _initial = false;
        /// <summary>
        /// 记录屏幕缩放比例
        /// </summary>
        public static float MapScale;
        //Message
        /// <summary>
        /// 获取预制体，用于处理玩家提示信息
        /// </summary>
        public static GameObject MessagePrefab;
        /// <summary>
        /// 用于处理玩家提示信息
        /// </summary>
        public static GameObject MessageTransform;
        /// <summary>
        /// 用于处理玩家提示信息
        /// </summary>
        public static float MessagePos;
        private data.PlayerData PlayerData;
        /// <summary>
        /// 打印玩家的信息，临时系统
        /// </summary>
        public Text[] Texts;


        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            if (!_initial)
            {
                RectTransform canvasrect = canvas.GetComponent<RectTransform>();
                /*
                screen_height = Screen.height;
                screen_width = Screen.width;
                Debug.Log(screen_height + "+" + screen_width);
                screen_height = canvasrect.sizeDelta.y;
                screen_width = canvasrect.sizeDelta.x;
                Debug.Log(screen_height + "+" + screen_width);
                */
                _screenHeight = canvasrect.rect.height;
                _screenWidth = canvasrect.rect.width;
                //Debug.Log(screen_height + "+" + screen_width);
                if (_screenHeight > _screenWidth * 0.7f)
                {
                    MapScale = _screenWidth / 1000;
                    Map.localScale = new Vector2(MapScale, MapScale);
                    Player.localScale = Map.localScale;
                }
                else
                {
                    float i = _screenHeight / 700;
                    Map.localScale = new Vector2(i, i);
                    Player.localScale = Map.localScale;
                }

                MessagePos = _screenHeight * 3 / 10;
            }
            //Message
            MessagePrefab = Resources.Load<GameObject>("Prefab/Message");
            GameObject emptyObj = new GameObject("messagetransform");
            emptyObj.AddComponent<RectTransform>();
            emptyObj.transform.SetParent(canvas.transform, false);
            MessageTransform = emptyObj;
        }
        private void Start()
        {
            PlayerData = data.PlayerData.Instance;
            printPlayerNature();
        }
        /// <summary>
        /// 打印玩家提示信息
        /// </summary>
        public static void CreateMessage(string Content)
        {
            GameObject NewMessage = Instantiate(MessagePrefab, Vector3.zero, Quaternion.identity);
            NewMessage.GetComponent<RectTransform>().SetParent(MessageTransform.GetComponent<RectTransform>(), false);
            NewMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, MessagePos);
            NewMessage.GetComponent<Message>().MessageContent.text = Content;

        }
        /// <summary>
        /// 打印玩家数据
        /// </summary>
        public void printPlayerNature()
        {
            Texts[0].text = "" + PlayerData.MaxSanity;
            Texts[1].text = "" + PlayerData.CurrentSanity;
            Texts[2].text = "" + PlayerData.MaxActionPoints;
            Texts[3].text = "" + PlayerData.CurrentActionPoints;
            Texts[4].text = "" + PlayerData.CurrentNodeNum;
            Texts[5].text = "" + PlayerData.Coin;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
