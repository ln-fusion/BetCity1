using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public GameObject card;
    public GameObject SummonBlock;

    private void Awake()
    {
        if (SummonBlock == null || SummonBlock.transform.parent != transform)
        {
            SummonBlock = ResolveSummonBlockChild();
        }
    }

    public GameObject GetSummonBlockObject()
    {
        if (SummonBlock == null || SummonBlock.transform.parent != transform)
        {
            SummonBlock = ResolveSummonBlockChild();
        }

        return SummonBlock;
    }

    private GameObject ResolveSummonBlockChild()
    {
        Transform direct = transform.Find("SummonBlock");
        if (direct != null)
        {
            return direct.gameObject;
        }

        foreach (Transform child in transform)
        {
            if (child.name.Contains("Summon"))
            {
                return child.gameObject;
            }
        }

        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
