using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор поставщиков.
    /// </summary>
    public class ProviderDTOComparer : IEqualityComparer<ProviderDTO>
    {
        public bool Equals(ProviderDTO first, ProviderDTO second)
        {
            if (second == null && first == null)
                return true;
            else if (first == null || second == null)
                return false;
            else if (first.Id == second.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(ProviderDTO obj)
        {
            return obj.Id;
        }
    }
}
