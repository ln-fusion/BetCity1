using BetCity.Core.ActionSystem;
using BetCity.GamePlay.Explorer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BetCity.GamePlay.Chest
{
    public class ChestTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void CreateChest()
        {
            var chestManager = new ChestEventManager();

            // 1. 创建不同类型的RoundConfig
            var roundConfigA = new RoundConfig<A>
            {
                ResourceList = new List<A> { new A(1), new A(2) }
            };
            var roundConfigB = new RoundConfig<B>
            {
                ResourceList = new List<B> { new B("a"), new B("b"), new B("c") }
            };

            // 2. 放入非泛型接口列表（核心：IRoundConfig统一类型）
            var mixedConfigs = new List<IRoundConfig>
            {
                roundConfigA,
                roundConfigB
            };

            // 3. 调用StartChest，支持混合类型传入
            GameActionContext context = new(this, mixedConfigs,null);
            var currentNodeChange = new EnterChestAction(context);
            ActionManager.Instance.Perform(currentNodeChange);
        }
    }
    public class A:Display
    {
        public int x;
        public A(int e)
        {
            x = e;
        }
        public void Display()
        {
            Debug.Log($"这是A，值是{x}");
        }
    }
    public class B : Display
    {
        public string x;
        public B(string e)
        {
            x = e;
        }
        public void Display()
        {
            Debug.Log($"这是B，值是{x}");

        }
    }
    public interface Display
    {
        public void Display();
    }
}
