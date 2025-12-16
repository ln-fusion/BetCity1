using BetCity.Core.Tools;
using BetCity.Storage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetCity.Explorer
{
    /// <summary>
    /// 适配不同大小的屏幕
    /// </summary>
    public class ExplorerScreenController : MonoSingleton<ExplorerScreenController>
    {
        private float _screenHeight;
        private float _screenWidth;
        public RectTransform Map;
        public RectTransform Player;
        public Canvas canvas;
        private static bool _initial = false;
        public static float MapScale;
        //Message
        public static GameObject MessagePrefab;
        public static GameObject MessageTransform;
        public static float MessagePos;
        //playernature
        public data.PlayerData PlayerData;
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
            printPlayerNature();
        }
        public static void CreateMessage(string Content)
        {
            GameObject NewMessage = Instantiate(MessagePrefab, Vector3.zero, Quaternion.identity);
            NewMessage.GetComponent<RectTransform>().SetParent(MessageTransform.GetComponent<RectTransform>(), false);
            NewMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, MessagePos);
            NewMessage.GetComponent<Message>().MessageContent.text = Content;

        }
        public void printPlayerNature()
        {
            Texts[0].text = "" + PlayerData.MaxSanity;
            Texts[1].text = "" + PlayerData.CurrentSanity;
            Texts[2].text = "" + PlayerData.MaxActionPoints;
            Texts[3].text = "" + PlayerData.CurrentActionPoints;
            Texts[4].text = "" + PlayerData.CurrentNodeNum;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }
}
