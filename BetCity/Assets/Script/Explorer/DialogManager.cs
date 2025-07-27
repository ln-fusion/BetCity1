using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class DialogManager : MonoBehaviour
{
    public TextAsset dialogDataFile;
    public SpriteRenderer spriteLeft;
    public SpriteRenderer spriteRight;
    public TMP_Text nameText;
    public TMP_Text dialogText;
    public List<Sprite> sprites = new List<Sprite>();
    private Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>();
    public int dialogIndex;
    public string[] dialogRows;
    public Button next;
    public GameObject optionButton;
    public Transform buttonGroup;
    private SanityManager sanityManager;
    public TMP_FontAsset chineseFont;

    private void Awake()
    {
        // 初始化角色图片映射
        if (sprites.Count >= 2)
        {
            imageDic["A"] = sprites[0];
            imageDic["B"] = sprites[1];
            imageDic["player"] = sprites.Count > 2 ? sprites[2] : null;
        }

        sanityManager = FindObjectOfType<SanityManager>();
        if (sanityManager == null)
        {
            Debug.LogError("场景中未找到SanityManager实例，请确保已添加该组件！");
        }

        // 设置中文字体
        if (chineseFont != null)
        {
            nameText.font = chineseFont;
            dialogText.font = chineseFont;
            if (optionButton != null)
            {
                var optionText = optionButton.GetComponentInChildren<TMP_Text>();
                if (optionText != null)
                {
                    optionText.font = chineseFont;
                }
            }
        }
        else
        {
            Debug.LogWarning("请在Inspector中指定支持中文的字体资产（TMP_FontAsset）");
        }
    }

    void Start()
    {
        if (dialogDataFile != null)
        {
            ReadText(dialogDataFile);
            dialogIndex = 0;
            ShowDiaLogRow();
        }
        else
        {
            Debug.LogError("请在Inspector中指定对话数据文件！");
        }
    }

    public void UpdateText(string _name, string _text)
    {
        string filteredName = FilterInvalidCharacters(_name);
        string filteredText = FilterInvalidCharacters(_text);

        nameText.text = filteredName;
        dialogText.text = filteredText;
    }

    public void UpdateImage(string _name, string _position)
    {
        spriteLeft.sprite = null;
        spriteRight.sprite = null;

        if (imageDic.TryGetValue(_name, out Sprite sprite))
        {
            if (_position == "左")
            {
                spriteLeft.sprite = sprite;
            }
            else if (_position == "右")
            {
                spriteRight.sprite = sprite;
            }
        }
        else
        {
            Debug.LogWarning($"未找到角色 {_name} 的图片资源");
        }
    }

    public void ReadText(TextAsset _textAsset)
    {
        dialogRows = _textAsset.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (dialogRows.Length > 0 && dialogRows[0].Contains("标志,ID,人物,位置,内容,跳转,效果,目标"))
        {
            List<string> tempRows = new List<string>(dialogRows);
            tempRows.RemoveAt(0);
            dialogRows = tempRows.ToArray();
        }
        Debug.Log($"成功读取 {dialogRows.Length} 行对话数据");
    }

    // 检查并显示当前对话ID对应的所有选项
    private bool CheckAndShowOptions()
    {
        List<string[]> optionRows = new List<string[]>();

        foreach (string row in dialogRows)
        {
            string[] cells = SplitCsvRow(row);
            // 关键修复：只匹配标志为&且ID等于当前dialogIndex的选项行
            if (cells.Length >= 6 && cells[0] == "&" && int.TryParse(cells[1], out int optionId))
            {
                // 你的选项行ID是3和4，但流程中当前ID是3，所以只需要匹配3即可
                if (optionId == dialogIndex)
                {
                    optionRows.Add(cells);
                }
            }
        }

        // 生成选项按钮
        if (optionRows.Count > 0)
        {
            GenerateOptions(optionRows);
            return true;
        }
        return false;
    }

    // 同时修复ShowDiaLogRow中普通对话的跳转逻辑（确保下一步正确更新ID）
    public void ShowDiaLogRow()
    {
        ClearOptionButtons();

        // 先检查是否有选项（优先处理选项）
        bool hasOptions = CheckAndShowOptions();
        if (hasOptions)
        {
            next.gameObject.SetActive(false);
            return;
        }

        // 处理普通对话行
        foreach (string row in dialogRows)
        {
            string[] cells = SplitCsvRow(row);
            if (cells.Length < 6) continue;

            if (cells[0] == "#" && int.TryParse(cells[1], out int id) && id == dialogIndex)
            {
                UpdateText(cells[2], cells[4]);
                UpdateImage(cells[2], cells[3]);
                ProcessSanityEffect(cells[6], cells[7]);

                // 修复：点击下一步时才更新dialogIndex为跳转ID
                next.onClick.RemoveAllListeners();
                if (int.TryParse(cells[5], out int nextId))
                {
                    next.onClick.AddListener(() =>
                    {
                        dialogIndex = nextId; // 点击后才更新ID
                        ShowDiaLogRow();
                    });
                }
                next.gameObject.SetActive(true);
                return;
            }
            else if (cells[0].Equals("END", StringComparison.OrdinalIgnoreCase) && int.TryParse(cells[1], out int endId) && endId == dialogIndex)
            {
                Debug.Log("剧情结束");
                next.gameObject.SetActive(false);
                return;
            }
        }

        Debug.LogError($"未找到ID为 {dialogIndex} 的对话行，请检查CSV");
    }

    // 生成选项按钮
    public void GenerateOptions(List<string[]> optionRows)
    {
        foreach (var cells in optionRows)
        {
            GameObject button = Instantiate(optionButton, buttonGroup);
            var buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.font = chineseFont;
                buttonText.text = FilterInvalidCharacters(cells[4]);
            }

            // 解析选项对应的跳转ID
            if (int.TryParse(cells[5], out int targetId))
            {
                Button btnComponent = button.GetComponent<Button>();
                btnComponent.onClick.AddListener(() =>
                {
                    // 处理选项可能带来的理智效果
                    ProcessSanityEffect(cells[6], cells[7]);
                    OnOptionClick(targetId);
                });
            }
            else
            {
                Debug.LogWarning($"选项 {cells[4]} 的跳转ID配置无效");
            }
        }
    }

    public void OnOptionClick(int _id)
    {
        dialogIndex = _id;
        ClearOptionButtons();
        ShowDiaLogRow();
    }

    private void ClearOptionButtons()
    {
        for (int i = 0; i < buttonGroup.childCount; i++)
        {
            Destroy(buttonGroup.GetChild(i).gameObject);
        }
    }

    private void ProcessSanityEffect(string effect, string target)
    {
        if (sanityManager == null || string.IsNullOrEmpty(effect) || target != "player")
            return;

        if (effect.Contains("加@"))
        {
            int amount = ParseEffectValue(effect, "加@");
            if (amount > 0) sanityManager.IncreaseSanity(amount);
        }
        else if (effect.Contains("@"))
        {
            int amount = ParseEffectValue(effect, "@");
            if (amount > 0) sanityManager.DecreaseSanity(amount);
        }
    }

    private int ParseEffectValue(string effect, string splitStr)
    {
        string[] parts = effect.Split(new[] { splitStr }, System.StringSplitOptions.None);
        if (parts.Length >= 2)
        {
            int.TryParse(parts[1], out int amount);
            return amount;
        }
        return 0;
    }

    private string FilterInvalidCharacters(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return input.Replace("餷", "").Replace("�", "");
    }

    // 处理CSV行分割，支持包含逗号的带引号内容
    private string[] SplitCsvRow(string row)
    {
        List<string> cells = new List<string>();
        bool inQuotes = false;
        string currentCell = "";

        foreach (char c in row)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                cells.Add(currentCell);
                currentCell = "";
            }
            else
            {
                currentCell += c;
            }
        }

        cells.Add(currentCell);
        return cells.ToArray();
    }
}