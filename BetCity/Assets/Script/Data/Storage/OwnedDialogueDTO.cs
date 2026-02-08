using System;
using System.Collections.Generic;

namespace BetCity.Data.Storage
{
    [Serializable]
    public class OwnedDialogueDTO
    {
        public int Id { get; }
        public bool HasPlayed { get; }
        public Dictionary<string, object> ExtraData { get; }

        public OwnedDialogueDTO() { }

        public OwnedDialogueDTO(int id, bool hasPlayed, Dictionary<string, object> extraData)
        {
            Id = id;
            HasPlayed = hasPlayed;
            ExtraData = extraData;
        }
    }
}
