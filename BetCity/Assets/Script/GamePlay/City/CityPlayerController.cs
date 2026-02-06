using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    public class CityPlayerController : MonoBehaviour
    {
        public GameObject Player;
        //脚本
        private CityMapController MapController => CityMapController.Instance;
        [field: SerializeField]
        public CityCameraController CameraController {  get; private set; }
        //主角属性
        private const float PLAYERMOVESPEED = 4f;
        public bool CameraStatic { get; private set; }
        public float CameraPositionMax { get; private set; }
        public float PlayerPositionMax { get; private set; }

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


        }
        public void Input(int horizontalInput)
        {
            if (Math.Abs(Player.transform.position.x) > CameraPositionMax)
            {
                CameraStatic = true;
            }
            else
            {
                CameraStatic = false;
            }

            Vector3 moveDirection = new Vector3(horizontalInput, 0, 0) * PLAYERMOVESPEED * Time.deltaTime;
            Vector3 movePosition = moveDirection + Player.transform.position;
            movePosition.x = Math.Clamp(movePosition.x, -PlayerPositionMax, PlayerPositionMax);
            Player.transform.rotation = Quaternion.Euler(new Vector3(0, 90 - 90 * horizontalInput, 0));
            Player.transform.position = movePosition;
            CameraController.Renew(moveDirection);
        }
    }
}
