using System;

namespace DuplicateHider.Models
{
    public struct DuplicateEntry
    {
        public Guid gameId;
        public bool isEdition;
        public string editionTag;

        public override int GetHashCode()
        {
            return gameId.GetHashCode();
        }
    }
}
