using BetCity.Core.Settings;
using BetCity.Core.Tools;
using System;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    /// <summary>
    /// 主角移动控制器
    /// </summary>
    public class CityPlayerController : MonoBehaviour
    {
        //脚本
        private CityMapController MapController => CityMapController.Instance;
        private InputManager inputManager=>InputManager.Instance;
        /// <summary>
        /// 相机控制脚本
        /// </summary>
        [field: SerializeField]
        public CityCameraController CameraController {  get; private set; }
        //主角属性
        private const float PLAYERMOVESPEED = 4f;
        /// <summary>
        /// 检测相机是否能移动
        /// </summary>
        public bool CameraStatic { get; private set; }
        /// <summary>
        /// 限制相机移动距离限制
        /// </summary>
        public float CameraPositionMax { get; private set; }
        /// <summary>
        /// 玩家移动距离限制
        /// </summary>
        public float PlayerPositionMax { get; private set; }
        private bool moveLeft => inputManager.IsMoveLeft; // 默认值设为A
        private bool moveRight => inputManager.IsMoveRight; // 默认值设为D
        /// <summary>
        /// 玩家控制器初始化
        /// </summary>
        public void Initial()
        {
            float screenLength = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
            CameraPositionMax = MapController.StreetLength - screenLength;
            PlayerPositionMax = MapController.StreetLength;
        }
        // Update is called once per frame
        void Update()
        {

            int horizontalInput = 0;

            if (moveLeft)
            {
                horizontalInput = -1;
            }
            else if (moveRight)
            {
                horizontalInput = 1;
            }
            if (horizontalInput == 0)
            {
                return;
            }
            InputDeal(horizontalInput);
        }
        /// <summary>
        /// 玩家输入处理
        /// </summary>
        /// <param name="horizontalInput"></param>
        private void InputDeal(int horizontalInput)
        {
            if (Math.Abs(transform.position.x) > CameraPositionMax)
            {
                CameraStatic = true;
            }
            else
            {
                CameraStatic = false;
            }

            Vector3 moveDirection = new Vector3(horizontalInput, 0, 0) * PLAYERMOVESPEED * Time.deltaTime;
            Vector3 movePosition = moveDirection + transform.position;
            movePosition.x = Math.Clamp(movePosition.x, -PlayerPositionMax, PlayerPositionMax);
            transform.rotation = Quaternion.Euler(new Vector3(0, 90 - 90 * horizontalInput, 0));
            transform.position = movePosition;
            CameraController.Renew(moveDirection);
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            CityNPCController npcController = other.gameObject.GetComponent<CityNPCController>();
            // 关键：通过Tag判断触发的物体类型（建议给触发体设置标签）
            if (npcController!=null)
            {
                Debug.Log("1");
            }
        }
    }
}
