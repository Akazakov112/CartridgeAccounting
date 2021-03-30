using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор картриджей.
    /// </summary>
    public class CartridgeDTOComparer : IEqualityComparer<CartridgeDTO>
    {
        public bool Equals(CartridgeDTO first, CartridgeDTO second)
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

        public int GetHashCode(CartridgeDTO cartridge)
        {
            return cartridge.Id;
        }
    }
}
