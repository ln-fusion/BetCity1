using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test_DataManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Travel(string scenename)
    {
        Debug.Log($"ÇÐ»»µ½explorerscene");

        SceneManager.LoadScene(scenename);
    }
}
