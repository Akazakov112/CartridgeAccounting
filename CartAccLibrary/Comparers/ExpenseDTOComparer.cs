using System.Collections.Generic;
using CartAccLibrary.Dto;

namespace CartAccLibrary.Comparers
{
    /// <summary>
    /// Компаратор списаний.
    /// </summary>
    class ExpenseDTOComparer : IEqualityComparer<ExpenseDTO>
    {
        public bool Equals(ExpenseDTO first, ExpenseDTO second)
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

        public int GetHashCode(ExpenseDTO obj)
        {
            return obj.Id;
        }
    }
}
