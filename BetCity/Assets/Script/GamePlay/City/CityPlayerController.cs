using BetCity.Core.Tools;
using System;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    public class CityPlayerController : MonoBehaviour
    {
        //脚本
        private CityMapController MapController => CityMapController.Instance;
        [field: SerializeField]
        public CityCameraController CameraController {  get; private set; }
        //主角属性
        private const float PLAYERMOVESPEED = 4f;
        public bool CameraStatic { get; private set; }
        public float CameraPositionMax { get; private set; }
        public float PlayerPositionMax { get; private set; }
        private KeyCode moveLeftKey => KeyCode.A; // 默认值设为A
        private KeyCode moveRightKey => KeyCode.D; // 默认值设为D

        // Start is called before the first frame update
        void Start()
        {

        }
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

            if (Input.GetKey(moveLeftKey))
            {
                horizontalInput = -1;
            }
            else if (Input.GetKey(moveRightKey))
            {
                horizontalInput = 1;
            }
            if (horizontalInput == 0)
            {
                return;
            }
            InputDeal(horizontalInput);
        }
        public void InputDeal(int horizontalInput)
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
