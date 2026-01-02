using BetCity.Core.Tools;
using BetCity.Storage;
using BetCity.Data.ConfigModels;
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
        private float screenHeight;
        private float screenWidth;
        private float mapHeight;
        private float mapWidth;

        private RectTransform CurrentMap;

        private MapData CurrentMapData;
        /// <summary>
        /// 获取玩家位置信息，用于处理移动逻辑
        /// </summary>
        public RectTransform Player;
        /// <summary>
        /// 获取画布信息，用于适配屏幕
        /// </summary>
        public Canvas canvas;
        private static bool _initial = false;
        [Header("屏幕拖动")]
        private Vector2 lastMousePos;
        /// <summary>
        /// 屏幕滑动灵敏度
        /// </summary>
        public float DragSensitivity;
        private bool mousePress;
        private Vector2 MapMoveLimitX;
        private Vector2 MapMoveLimitY;
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
        private data.PlayerData playerData;
        private ExplorerPlayerController playerController;
        /// <summary>
        /// 打印玩家的信息，临时系统
        /// </summary>
        public Text[] Texts;
        /// <summary>
        /// 打印玩家的信息，临时系统
        /// </summary>
        public float FocusSpeed;
        public float temp;



        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            if (!_initial)
            {
                //RectTransform canvasrect = canvas.GetComponent<RectTransform>();
                //_screenHeight = canvasrect.rect.height;
                //_screenWidth = canvasrect.rect.width;
                //if (_screenHeight > _screenWidth * 0.7f)
                //{
                //    MapScale = _screenWidth / 1000;
                //    Map.localScale = new Vector2(MapScale, MapScale);
                //    Player.localScale = Map.localScale;
                //}
                //else
                //{
                //    float i = _screenHeight / 700;
                //    Map.localScale = new Vector2(i, i);
                //    Player.localScale = Map.localScale;
                //}

                //MessagePos = _screenHeight * 3 / 10;
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
            playerData = data.PlayerData.Instance;
            playerController = ExplorerPlayerController.Instance;
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
            Texts[0].text = "" + playerData.MaxSanity;
            Texts[1].text = "" + playerData.CurrentSanity;
            Texts[2].text = "" + playerData.MaxActionPoints;
            Texts[3].text = "" + playerData.CurrentActionPoints;
            Texts[4].text = "" + playerData.CurrentNodeNum;
            Texts[5].text = "" + playerData.Coin;
        }
        // Update is called once per frame
        void Update()
        {
            if (ExplorerPlayerController.PLAYER_STATUS!=0)
            {
                mousePress =false;
                return;
            }
            HandleMouseDrag();
        }
        void HandleMouseDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                mousePress = true;
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButton(0)&&mousePress==true) 
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 mouseDelta = currentMousePos - lastMousePos;
                Vector2 mapMoveDir = mouseDelta * DragSensitivity;
                Vector2 targetPosition=ClampMapPosition(mapMoveDir+CurrentMap.anchoredPosition);
                CurrentMap.anchoredPosition =targetPosition;
                lastMousePos = currentMousePos;
            }
        }
        public void Mapintial(GameObject map)
        {
            CurrentMap = map.GetComponent<RectTransform>();
            CurrentMapData=map.GetComponent<MapData>();
            CurrentMap.localScale = Vector3.one*CurrentMapData.MapScale;
            mapHeight = CurrentMapData.BackHeight * CurrentMapData.MapScale;
            mapWidth = CurrentMapData.BackWidth * CurrentMapData.MapScale;
            RectTransform canvasrect = canvas.GetComponent<RectTransform>();
            screenHeight = canvasrect.rect.height;
            screenWidth = canvasrect.rect.width;
            CalculateMapMoveBounds();
        }
        private void CalculateMapMoveBounds()
        {
            // 计算相机X轴移动边界：左右不能超出地图边缘
            float maxX =(mapWidth-screenWidth)/2;
            MapMoveLimitX = new Vector2(-maxX, maxX);

            // 计算相机Y轴移动边界：上下不能超出地图边缘
            float maxY = (mapHeight - screenHeight) / 2;
            MapMoveLimitY = new Vector2(-maxY, maxY);
        }
        private Vector2 ClampMapPosition(Vector3 targetPos)
        {
            // 只限制X、Y轴（2D地图，Z轴保持不变）
            float clampedX = Mathf.Clamp(targetPos.x, MapMoveLimitX.x, MapMoveLimitX.y);
            float clampedY = Mathf.Clamp(targetPos.y, MapMoveLimitY.x, MapMoveLimitY.y);

            return new Vector2(clampedX, clampedY);
        }
        public void ScreenFocusInstant(Node targetNode)
        {
            Vector2 movePosition = ClampMapPosition(-1 *CurrentMapData.MapScale* new Vector2(targetNode.Xposition, targetNode.Yposition));

            CurrentMap.anchoredPosition = movePosition;

        }
        public void ScreenFocus(Node targetNode)
        {
            StartCoroutine(screenFocus(targetNode));
        }
        private IEnumerator screenFocus(Node targetNode)
        {
            Vector2 movePosition=ClampMapPosition(-1 * CurrentMapData.MapScale * new Vector2(targetNode.Xposition, targetNode.Yposition));
            Vector2 moveDirection =movePosition- CurrentMap.anchoredPosition;
            Vector2 remainMoveDirection =moveDirection;
            Vector2 moveFrame = moveDirection.normalized*FocusSpeed;
            while (remainMoveDirection.sqrMagnitude>temp)
            {
                CurrentMap.anchoredPosition += moveFrame;
                remainMoveDirection = movePosition - CurrentMap.anchoredPosition;
                Debug.Log(remainMoveDirection.sqrMagnitude);
                yield return null;
            }
            CurrentMap.anchoredPosition = movePosition;
            Debug.Log(movePosition - CurrentMap.anchoredPosition);
        }
    }
}
