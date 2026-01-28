using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityPlayerController : MonoSingleton<CityPlayerController>
{
    public GameObject Player;
    //½Å±¾
    private CityMapController MapController => CityMapController.Instance;
    //Ö÷½ÇÊôÐÔ
    private const float SPEED = 4f;
    protected override void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = 0;

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
        Vector3 moveDirection = new Vector3(horizontalInput, 0, 0) * SPEED * Time.deltaTime;
        //Vector3 movePosition = Player.transform.position+moveDirection;
        //Player.transform.position = movePosition;
        Player.transform.rotation = Quaternion.Euler(new Vector3(0,90-90*horizontalInput,0));
        MapController.Renew(moveDirection);
    }
}
