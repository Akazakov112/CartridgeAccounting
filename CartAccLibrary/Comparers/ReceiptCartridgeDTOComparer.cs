using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор картриджей поступления.
    /// </summary>
    public class ReceiptCartridgeDTOComparer : IEqualityComparer<ReceiptCartridgeDTO>
    {
        public bool Equals(ReceiptCartridgeDTO first, ReceiptCartridgeDTO second)
        {
            if (second == null && first == null)
                return true;
            else if (first == null || second == null)
                return false;
            else if (first.Cartridge.Id == second.Cartridge.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(ReceiptCartridgeDTO obj)
        {
            return obj.Cartridge.Id;
        }
    }
}
