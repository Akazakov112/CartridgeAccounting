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
    /// Репозиторий картриджей.
    /// </summary>
    public class CartridgeRepository : IRepository<Cartridge>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public CartridgeRepository(CartAccDbContext context)
        {
            dbContext = context;
        }

        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(Cartridge item)
        {
            dbContext.Cartridges.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(Cartridge item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(Cartridge item)
        {
            dbContext.Cartridges.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public Cartridge Get(int id)
        {
            return dbContext.Cartridges
                .Include(c => c.Compatibility).ThenInclude(p => p.Printer)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<Cartridge> Find(Func<Cartridge, bool> predicate)
        {
            return dbContext.Cartridges
                .Include(c => c.Compatibility).ThenInclude(p => p.Printer)
                .Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Cartridge> GetAll()
        {
            return dbContext.Cartridges
                .Include(c => c.Compatibility).ThenInclude(p => p.Printer);
        }
    }
}
