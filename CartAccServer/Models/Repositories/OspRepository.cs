using System;
using System.Collections.Generic;
using System.Linq;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace CartAccServer.Models.Repositories
{
    /// <summary>
    /// Репозиторий ОСП.
    /// </summary>
    public class OspRepository : IRepository<Osp>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public OspRepository(CartAccDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(Osp item)
        {
            dbContext.Osps.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(Osp item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(Osp item)
        {
            dbContext.Osps.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public Osp Get(int id)
        {
            return dbContext.Osps.FirstOrDefault(i => i.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<Osp> Find(Func<Osp, bool> predicate)
        {
            return dbContext.Osps.Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Osp> GetAll()
        {
            return dbContext.Osps;
        }
    }
}
