using System.IO;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public  class Explorer_GameDataManager : MonoBehaviour
{
    private static string csvFilePath; // CSV文件路径
    private static bool Initial = false;

    private void Awake()
    {
        string assetsPath = Application.dataPath;
        csvFilePath = Path.Combine(assetsPath, "Data", "playernature.csv");
        if (!Initial)
        {
            Initial = true;
            string[] lines = {
            "MaxSanity,CurrentSanity,MaxActionPoints,CurrentActionPoints,CurrentNodeNum",

            $"{5},{5},{10},{0},{0}"
            };
            File.WriteAllLines(csvFilePath, lines);
        }
        LoadPlayerData();
    }
    
    public static void LoadPlayerData()
    {
        if (File.Exists(csvFilePath))
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length > 1)
            {
                string[] values = lines[1].Split(',');
                if (values.Length >= 2)
                {
                    int.TryParse(values[0], out PlayerNature.maxSanity);
                    int.TryParse(values[1], out PlayerNature.currentSanity);
                    int.TryParse(values[2], out PlayerNature.maxActionPoints);
                    int.TryParse(values[3], out PlayerNature.currentActionPoints);
                    int.TryParse(values[4], out PlayerNature.currentNodeNum);
                }
            }
        }
    }
    public static void SaveToCSV()
    {
        string[] lines = {
            "MaxSanity,CurrentSanity,MaxActionPoints,CurrentActionPoints,CurrentNodeNum",

            $"{PlayerNature.maxSanity},{PlayerNature.currentSanity},{PlayerNature.maxActionPoints},{PlayerNature.currentActionPoints},{PlayerNature.currentNodeNum}"
        };
        File.WriteAllLines(csvFilePath, lines);
    }

}
