using System;
using System.Collections.Generic;
using System.Linq;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace CartAccServer.Models.Repositories
{
    public class ReceiptCartsRepository : IRepository<ReceiptCartridge>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public ReceiptCartsRepository(CartAccDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(ReceiptCartridge item)
        {
            dbContext.ReceiptCartridges.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(ReceiptCartridge item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(ReceiptCartridge item)
        {
            dbContext.ReceiptCartridges.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public ReceiptCartridge Get(int id)
        {
            return dbContext.ReceiptCartridges
                .Include(c => c.Cartridge)
                .FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<ReceiptCartridge> Find(Func<ReceiptCartridge, bool> predicate)
        {
            return dbContext.ReceiptCartridges
                .Include(c => c.Cartridge)
                .Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReceiptCartridge> GetAll()
        {
            return dbContext.ReceiptCartridges
                .Include(c => c.Cartridge);
        }
    }
}
