//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;
//using TMPro;

//public class SanityUIController : MonoBehaviour
//{
//    [Header("UI 组件")]
//    [SerializeField] private Button increaseButton;
//    [SerializeField] private Button decreaseButton;
//    [SerializeField] private Slider sanitySlider;
//    [SerializeField] private TextMeshProUGUI sanityText;

//    [Header("调试设置")]
//    [SerializeField] private int changeAmount = 10; // 理智变化量
//    [SerializeField] private string sanityTextFormat = "Sanity: {0} / {1}";

//    private SanityManager sanityManager;

//    private void Awake()
//    {
//        sanityManager = FindObjectOfType<SanityManager>();
//        if (sanityManager == null)
//        {
//            Debug.LogError("SanityUIController: 未找到 SanityManager 实例！");
//        }

//        ValidateReferences();
//    }

//    private void Start()
//    {
//        UpdateSanityUI();
//        SetupEventListeners();

//#if UNITY_EDITOR
//        SetupButtonEvents(); // 仅在编辑器中激活测试按钮
//#endif

//        SceneManager.sceneLoaded += OnSceneLoaded;
//    }

//    private void OnDestroy()
//    {
//        RemoveEventListeners();
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        sanityManager = FindObjectOfType<SanityManager>();
//        if (sanityManager != null)
//        {
//            RemoveEventListeners();
//            SetupEventListeners();
//            UpdateSanityUI();
//        }
//    }

//    private void ValidateReferences()
//    {
//        if (increaseButton == null)
//            Debug.LogWarning("increaseButton 未分配");
//        if (decreaseButton == null)
//            Debug.LogWarning("decreaseButton 未分配");
//        if (sanitySlider == null)
//            Debug.LogError("sanitySlider 未分配");
//        if (sanityText == null)
//            Debug.LogError("sanityText 未分配");
//    }

//    private void SetupButtonEvents()
//    {
//        if (increaseButton != null)
//            increaseButton.onClick.AddListener(() => sanityManager?.IncreaseSanity(changeAmount));

//        if (decreaseButton != null)
//            decreaseButton.onClick.AddListener(() => sanityManager?.DecreaseSanity(changeAmount));
//    }

//    private void SetupEventListeners()
//    {
//        if (sanityManager != null)
//        {
//            sanityManager.onSanityChanged.AddListener(UpdateSanityUI);
//        }
//    }

//    private void RemoveEventListeners()
//    {
//        if (sanityManager != null)
//        {
//            sanityManager.onSanityChanged.RemoveListener(UpdateSanityUI);
//        }
//    }

//    public void UpdateSanityUI()
//    {
//        if (sanityManager == null) return;

//        int current = sanityManager.CurrentSanity;
//        int max = sanityManager.MaxSanity;

//        if (sanitySlider != null)
//        {
//            sanitySlider.maxValue = max;
//            sanitySlider.value = current;
//        }

//        if (sanityText != null)
//        {
//            sanityText.text = string.Format(sanityTextFormat, current, max);
//        }
//    }
//}
