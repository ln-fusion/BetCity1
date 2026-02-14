using System.Collections.Generic;
using UnityEngine;

namespace BetCity.Core.DialogueSystem
{
    [System.Serializable]
    public class PortraitEntry
    {
        public string CharacterId;
        public Expression Expression;
        public Sprite Portrait;
    }
    public class PortraitDatabase : MonoBehaviour
    {
        private static PortraitDatabase instance;
        public static PortraitDatabase Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<PortraitDatabase>();
                return instance;
            }
        }
        [SerializeField]
        private List<PortraitEntry> entries = new List<PortraitEntry>();

        private Dictionary<(string, Expression), Sprite> map = null;

        private void EnsureMap()
        {
            if (map != null) return;
            map = new Dictionary<(string, Expression), Sprite>();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.CharacterId) || e.Portrait == null) continue;
                map[(e.CharacterId, e.Expression)] = e.Portrait;
            }
        }

        public Sprite GetPortrait(string characterId, Expression expression)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            EnsureMap();
            if (map.TryGetValue((characterId, expression), out var s)) return s;
            if (map.TryGetValue((characterId, Expression.Neutral), out var s2)) return s2;
            return null;
        }
    }
}
