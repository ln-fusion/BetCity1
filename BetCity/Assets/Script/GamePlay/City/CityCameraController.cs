using BetCity.Core.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    public class CityCameraController : MonoBehaviour
    {
        CityMapController MapController => CityMapController.Instance;
        [field:SerializeField]
        public CityPlayerController PlayerController {  get; private set; }
        [field: SerializeField]
        public GameObject MainCamera { get; private set; }
        public float CameraMax { get; private set; }

        private void Start()
        {

        }
        public void Initial()
        {

        }
        public void Renew(Vector3 moveDirection)
        {
            if (PlayerController.CameraStatic) return;
            MainCamera.transform.position += moveDirection;
        }
    }
}
