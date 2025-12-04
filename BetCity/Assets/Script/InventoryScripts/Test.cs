using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Button button;
    public SouvenirManager souvenirManager;
    private void Start()
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
    }
    public void OnButtonClick()
    {
        souvenirManager.OwnSouvenirById(0, out Souvenir souvenir, out string str);
        Debug.Log(str);
        Debug.Log(souvenir);
        souvenirManager.LoseSouvenirById(0, out souvenir, out str);
        Debug.Log(str);
        Debug.Log(souvenir);
        souvenirManager.OwnSouvenirById(1, out souvenir, out str);
        Debug.Log(str);
        Debug.Log(souvenir);

    }
}
