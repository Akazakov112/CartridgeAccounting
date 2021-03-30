using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор для баланса.
    /// </summary>
    public class BalanceDTOComparer : IEqualityComparer<BalanceDTO>
    {
        public bool Equals(BalanceDTO first, BalanceDTO second)
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

        public int GetHashCode(BalanceDTO obj)
        {
            return obj.Id;
        }
    }
}
