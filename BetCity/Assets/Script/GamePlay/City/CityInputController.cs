using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.City
{
    public class CityInputController : MonoBehaviour
    {
        [field: SerializeField]
        public CityPlayerController PlayerController { get; private set; }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            int horizontalInput = 0;

            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                horizontalInput = 1;
            }
            if (horizontalInput == 0)
            {
                return;
            }
            PlayerController.Input(horizontalInput);
        }
    }
}
