using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    public Text MessageContent;
    void Start()
    {
        StartCoroutine(Message_Destroy());
        StartCoroutine(Message_Move());
    }
    public IEnumerator Message_Destroy()
    {
        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }
    public IEnumerator Message_Move()
    {
        while (true)
        {
            GetComponent<RectTransform>() .anchoredPosition += Vector2.up * 40*Time.deltaTime;
            yield return null;
        }
    }
}
