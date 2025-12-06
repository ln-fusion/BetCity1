//using UnityEngine;
//using UnityEngine.UI; // 引用 UI 命名空间以访问 Image 组件
//using UnityEngine.Events;
//using System.IO;

//[System.Serializable]
//public class SanityEvent : UnityEvent<int> { }

//public class SanityManager : MonoBehaviour
//{
//    [SerializeField] private Image sanityBarImage;   // 用于显示理智条的 Image 组件

//    [Header("逻辑状态锁")]
//    public bool IsLocked = false; // 外部可控制，防止误调用


//    public SanityEvent onSanityIncreased; // 理智增加事件
//    public SanityEvent onSanityDecreased; // 理智减少事件
//    public UnityEvent onSanityChanged;    // 理智变化事件
//    public UnityEvent onSanityZero;       // 理智归零事件

//    private void Awake()
//    {

//        // 初始化事件
//        if (onSanityIncreased == null)
//            onSanityIncreased = new SanityEvent();

//        if (onSanityDecreased == null)
//            onSanityDecreased = new SanityEvent();

//        if (onSanityChanged == null)
//            onSanityChanged = new UnityEvent();

//        if (onSanityZero == null)
//            onSanityZero = new UnityEvent();

//    }

//    private void Start()
//    {
//        // 初始化理智条显示
//        //UpdateSanityBar();  // 初始设置理智条的显示
//    }



//    // 增加理智值
//    public void IncreaseSanity(int amount)
//    {
//        if (amount <= 0) return;

//        int oldValue = currentSanity;
//        currentSanity = Mathf.Min(maxSanity, currentSanity + amount);

//        if (currentSanity != oldValue)
//        {
//            onSanityIncreased?.Invoke(amount);
//            onSanityChanged?.Invoke();
//            //UpdateSanityBar(); // 更新理智条显示
//        }
//    }

//    // 减少理智值
//    public void DecreaseSanity(int amount)
//    {
//     /*   if (IsLocked)
//        {
//            Debug.Log("SanityManager: 当前锁定状态，理智无法减少！");
//            return;
//        } */

//        if (amount <= 0) return;

//        int oldValue = currentSanity;
//        currentSanity = Mathf.Max(0, currentSanity - amount);

//        if (currentSanity != oldValue)
//        {
//            onSanityDecreased?.Invoke(amount);
//            onSanityChanged?.Invoke();
//            SaveToCSV();
//            if (currentSanity <= 0)
//            {
//                onSanityZero?.Invoke();
//            }
//            UpdateSanityBar();
//        }
//    }

//    // 设置理智值
//    public void SetSanity(int newSanity)
//    {
//        int oldValue = currentSanity;
//        currentSanity = Mathf.Clamp(newSanity, 0, maxSanity);

//        if (currentSanity != oldValue)
//        {
//            onSanityChanged?.Invoke();
//            SaveToCSV(); // 保存到CSV
//            if (currentSanity <= 0)
//            {
//                onSanityZero?.Invoke();
//            }
//            //UpdateSanityBar(); // 更新理智条显示
//        }
//    }

//    // 更新理智条显示
//    private void UpdateSanityBar()
//    {
//        if (sanityBarImage != null)
//        {
//            float fillAmount = (float)currentSanity / maxSanity;
//            sanityBarImage.fillAmount = fillAmount; // 更新理智条的显示（通过 fillAmount 控制填充）
//        }
//    }
//}
