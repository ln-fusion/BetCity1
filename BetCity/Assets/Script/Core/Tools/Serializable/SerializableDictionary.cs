using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetCity.Core.Tools
{
    /// <summary>
    /// 可被序列化的字典
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : IEnumerable
    {
        [Serializable]
        public struct Ele
        {
            public TKey key;
            public TValue value;
        }

        [SerializeField]
        private List<Ele> list = new List<Ele>();
        private Lazy<Dictionary<TKey, TValue>> dic;

        SerializableDictionary()
        {
            dic = new Lazy<Dictionary<TKey, TValue>>(Init);
        }

        /// <summary>
        /// 将序列化字典转化为字典
        /// </summary>
        public Dictionary<TKey, TValue> Init()
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(list.Count);
            for (int i = 0; i < list.Count; ++i)
                dictionary.Add(list[i].key, list[i].value);
            return dictionary;
        }

        public TValue this[TKey key] { get => dic.Value[key]; }

        public int Count => dic.Value.Count;

        public bool ContainsKey(TKey key)
        {
            return dic.Value.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dic.Value.TryGetValue(key, out value);
        }

        public IEnumerator GetEnumerator()
        {
            return dic.Value.GetEnumerator();
        }
    }
}