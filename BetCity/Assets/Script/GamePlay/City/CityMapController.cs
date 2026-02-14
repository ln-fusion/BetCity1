using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace BetCity.GamePlay.City
{
    /// <summary>
    /// 城市地图控制器
    /// </summary>
    public class CityMapController : MonoSingleton<CityMapController>
    {
        /// <summary>
        /// 摄像机控制器
        /// </summary>
        [field: SerializeField]
        public CityCameraController CameraController { get; private set; }
        /// <summary>
        /// 玩家控制器
        /// </summary>
        [field: SerializeField]
        public CityPlayerController PlayerController { get; private set; }
        /// <summary>
        /// UI固定背景图片
        /// </summary>
        [field: SerializeField]
        public GameObject Back { get; private set; }
        private float ScreenWidth => Screen.width;
        private float ScreenHeight => Screen.height;
        /// <summary>
        /// 玩家
        /// </summary>
        [field: SerializeField]
        public GameObject Player { get; private set; }
        /// <summary>
        /// 城市背景
        /// </summary>
        [field: SerializeField]
        public GameObject Street { get; private set; }
        public float StreetLength { get; private set; }
        /// <summary>
        /// 场景地图边缘数值保留量
        /// </summary>
        const float LENGTHOFFSET = 1;
        //地图长度
        private const float mapWidth = 40;
        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
        }
        void Start()
        {

            //背景填充屏幕
            Back.GetComponent<RectTransform>().sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
            //player
            Player.transform.localPosition = new Vector3(0, 1.2f, 0);
            //主城图片位置
            Street.transform.localPosition = new Vector3(0, 0, 0);
            //获取主城限制数值
            StreetLength = Street.GetComponent<SpriteRenderer>().sprite.rect.width / (Street.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit * 2) - LENGTHOFFSET;
            CameraController.Initial();
            PlayerController.Initial();
        }
    }
}
