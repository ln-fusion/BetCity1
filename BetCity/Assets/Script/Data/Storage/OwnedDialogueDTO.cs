using System;
using System.Collections.Generic;

namespace BetCity.Data.Storage
{
    [Serializable]
    public class OwnedDialogueDTO
    {
        public int Id { get; }
        public int Times { get; }
        public Dictionary<string, object> ExtraData { get; }

        public OwnedDialogueDTO() { }

        public OwnedDialogueDTO(int id, int times, Dictionary<string, object> extraData)
        {
            Id = id;
            Times = times;
            ExtraData = extraData;
        }
    }
}
