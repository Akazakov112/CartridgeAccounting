using System;
using System.Collections.Generic;

namespace CartAccServer.Models.Interfaces.Services
{
    /// <summary>
    /// Интерфейс сервисов сущностей.
    /// </summary>
    /// <typeparam name="T">Класс сущности БД</typeparam>
    /// <typeparam name="U">Класс сущности DTO</typeparam>
    public interface IEntityService<T, U> where T : class
                                          where U : class
    {
        /// <summary>
        /// Получить все объекты в БД.
        /// </summary>
        /// <returns>Список объектов DTO</returns>
        ICollection<U> GetAll();

        /// <summary>
        /// Найти объект в БД по Id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Объект DTO</returns>
        U Get(int id);

        /// <summary>
        /// Найти по предикату.
        /// </summary>
        /// <param name="predicate">Предикат фильтрации</param>
        /// <returns>Список объектов DTO</returns>
        ICollection<U> Find(Func<T, bool> predicate);

        /// <summary>
        /// Создать и добавить объект для БД из DTO.
        /// </summary>
        /// <param name="item">Объект DTO</param>
        void Add(U item);

        /// <summary>
        /// Обновить данные объекта в БД из DTO.
        /// </summary>
        /// <param name="item">Объект DTO</param>
        void Update(U item);
    }
}
