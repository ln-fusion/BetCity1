using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhaseDisplayer : MonoBehaviour
{
    public TextMeshProUGUI phaseText;
    // Start is called before the first frame update
    void Start()
    {
        CombatManager.Instance.phaseChangeEvent.AddListener(UpdateText);
    }

    // Update is called once per frame

    void UpdateText()
    {
        phaseText.text = CombatManager.Instance.GamePhase.ToString();
    }
 
}
