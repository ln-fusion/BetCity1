using BetCity.Core.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityMapController : MonoSingleton<CityMapController>
{
    [field:SerializeField]
    public GameObject Back {  get; private set; }
    private float ScreenWidth=>Screen.width;
    private float ScreenHeight=>Screen.height;
    [field: SerializeField]
    public GameObject Street {  get; private set; }
    //地图长度
    private const float mapWidth=40;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {

        //背景填充屏幕
        Back.GetComponent<RectTransform>().sizeDelta = new Vector2(ScreenWidth, ScreenHeight);
        //主城图片位置
        Vector3 viewportPos = new Vector3(0.5f, 0f, 0);
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewportPos);
        Street.transform.position = new Vector3(transform.position.x, worldPos.y, transform.position.z);
        //获取主城限制数值
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Renew(Vector3 move)
    {
        Street.transform.position -= move;
    }
}
