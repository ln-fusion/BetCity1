using System.IO;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public  class Explorer_GameDataManager : MonoBehaviour
{
    private string csvFilePath; // CSVÎÄ¼þÂ·¾¶

    private void Awake()
    {
        string assetsPath = Application.dataPath;
        csvFilePath = Path.Combine(assetsPath, "Data", "playernature.csv");
        LoadPlayerData();
    }
    
    public void LoadPlayerData()
    {
        if (File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length > 1)
            {
                string[] values = lines[1].Split(',');
                if (values.Length >= 2)
                {
                    int.TryParse(values[0], out Playernature.maxSanity);
                    int.TryParse(values[1], out Playernature.currentSanity);
                    int.TryParse(values[2], out Playernature.maxActionPoints);
                    int.TryParse(values[3], out Playernature.currentActionPoints);
                    int.TryParse(values[4], out Playernature.currentNodeNum);
                }
            }
        }
    }
    public void SaveToCSV()
    {
        string[] lines = {
            "MaxSanity,CurrentSanity,MaxActionPoints,CurrentActionPoints,CurrentNodeNum",

            $"{Playernature.maxSanity},{Playernature.currentSanity},{Playernature.maxActionPoints},{Playernature.currentActionPoints},{Playernature.currentNodeNum}"
        };
        File.WriteAllLines(csvFilePath, lines);
    }

}
