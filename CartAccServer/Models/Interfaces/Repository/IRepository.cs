using System;
using System.Collections.Generic;

namespace CartAccServer.Models.Interfaces.Repository
{
    /// <summary>
    /// Интерфейс для репозиториев сущностей.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        void Create(T item);

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        void Update(T item);

        /// <summary>
        /// Удалить запись из БД.
        /// </summary>
        /// <param name="item"></param>
        void Delete(T item);

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        T Get(int id);

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();
    }
}
