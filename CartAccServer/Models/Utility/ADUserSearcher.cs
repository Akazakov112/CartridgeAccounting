using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Threading.Tasks;
using CartAccLibrary.Dto;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Utility;
using Microsoft.Extensions.Configuration;

namespace CartAccServer.Models.Utility
{
    /// <summary>
    /// Осуществляет поиск пользователя в Active Directory.
    /// </summary>
    public class ADUserSearcher : IUserSearcher
    {
        /// <summary>
        /// Поисковик.
        /// </summary>
        private readonly DirectorySearcher search;

        /// <summary>
        /// Результаты поиска.
        /// </summary>
        private SearchResultCollection results;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="configuration"></param>
        public ADUserSearcher(IConfiguration configuration)
        {
            search = new DirectorySearcher(configuration["Domain"]);
        }


        /// <summary>
        /// Асинхронный поиск пользователей в Active Directory по Ф.И.О.
        /// </summary>
        /// <param name="name">Ф.И.О. пользователя</param>
        /// <returns>Список найденных пользователей Dto</returns>
        public async Task<List<UserDTO>> FindAsync(string name)
        {
            return await Task.Run(() => Find(name));
        }

        /// <summary>
        /// Поиск пользователей в Active Directory по Ф.И.О.
        /// </summary>
        /// <param name="name">Ф.И.О. пользователя</param>
        /// <returns>Список найденных пользователей Dto</returns>
        private List<UserDTO> Find(string name)
        {
            // Применить фильтр поиска по запросу.
            search.Filter = $"(&(objectClass=user)(DisplayName=*{name}*))";
            // Коллекция результатов.
            List<UserDTO> users = new List<UserDTO>();
            try
            {
                results = search.FindAll();
                // Перебираем результаты и создаем пользователей.
                foreach (SearchResult results in results)
                {
                    DirectoryEntry result = results.GetDirectoryEntry();
                    var user = new UserDTO(0, result.Properties["SamAccountName"].Value.ToString(), result.Properties["DisplayName"].Value.ToString(), new OspDTO(), new AccessDTO());
                    users.Add(user);
                }
                // Вернуть коллекцию результатов.
                return users;
            }
            catch (InvalidOperationException invEx)
            {
                throw new ValidationException(invEx.InnerException.Message, "");
            }
            catch (NotSupportedException notEx)
            {
                throw new ValidationException(notEx.InnerException.Message, "");
            }
        }
    }
}
