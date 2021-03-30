using System.Collections.Generic;
using System.Threading.Tasks;
using CartAccLibrary.Dto;

namespace CartAccServer.Models.Interfaces.Utility
{
    /// <summary>
    /// Интерфейс поисковика пользователей.
    /// </summary>
    interface IUserSearcher
    {
        /// <summary>
        /// Ищет пользователей в Active Directory.
        /// </summary>
        /// <param name="name">Ф.И.О. пользователя</param>
        /// <returns></returns>
        Task<List<UserDTO>> FindAsync(string name);
    }
}
