using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Explorer_ScreenController : MonoBehaviour
{
    private float screen_height;
    private float screen_width;
    public RectTransform Map;
    public RectTransform Player;
    public Canvas canvas;
    private static bool Initial = false;
    public static float mapscale;
    //Message
    public static GameObject messagePrefab;
    public static GameObject messageTransform;
    public static float messagepos;
    //playernature
    public Text[] texts;


    // Start is called before the first frame update
    void Awake()
    {
        if (!Initial)
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
            screen_height = canvasrect.rect.height;
            screen_width = canvasrect.rect.width;
            //Debug.Log(screen_height + "+" + screen_width);
            if (screen_height > screen_width * 0.7f)
            {
                mapscale = screen_width / 1000;
                Map.localScale = new Vector2(mapscale, mapscale);
                Player.localScale = Map.localScale;
            }
            else
            {
                float i = screen_height / 700;
                Map.localScale = new Vector2(i, i);
                Player.localScale = Map.localScale;
            }

            messagepos = screen_height * 3 / 10;
        }
        //Message
        messagePrefab = Resources.Load<GameObject>("Prefab/Message");
        GameObject emptyObj = new GameObject("messagetransform");
        emptyObj.AddComponent<RectTransform>();
        emptyObj.transform.SetParent(canvas.transform, false);
        messageTransform = emptyObj;
    }
    private void Start()
    {
        printPlayerNature();
    }
    public static void CreateMessage(string Content)
    {
        GameObject NewMessage = Instantiate(messagePrefab, Vector3.zero, Quaternion.identity);
        NewMessage.GetComponent<RectTransform>().SetParent(messageTransform.GetComponent<RectTransform>(), false);
        NewMessage.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, messagepos);
        NewMessage.GetComponent<Message>().MessageContent.text = Content;

    }
    public void printPlayerNature()
    {
        texts[0].text = "" + Playernature.maxSanity;
        texts[1].text = "" + Playernature.currentSanity;
        texts[2].text = "" + Playernature.maxActionPoints;
        texts[3].text = "" + Playernature.currentActionPoints;
        texts[4].text = "" + Playernature.currentNodeNum ;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
