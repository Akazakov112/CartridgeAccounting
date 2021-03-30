using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор поступлений.
    /// </summary>
    class ReceiptDTOComparer : IEqualityComparer<ReceiptDTO>
    {
        public bool Equals(ReceiptDTO first, ReceiptDTO second)
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

        public int GetHashCode(ReceiptDTO obj)
        {
            return obj.Id;
        }
    }
}
