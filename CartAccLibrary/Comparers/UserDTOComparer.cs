using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор для пользователей.
    /// </summary>
    public class UserDTOComparer : IEqualityComparer<UserDTO>
    {
        public bool Equals(UserDTO first, UserDTO second)
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

        public int GetHashCode(UserDTO user)
        {
            return user.Id;
        }
    }
}
