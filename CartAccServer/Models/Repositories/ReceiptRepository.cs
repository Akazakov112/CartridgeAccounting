using System;
using System.Collections.Generic;
using System.Linq;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace CartAccServer.Models.Repositories
{
    public class ReceiptRepository : IRepository<Receipt>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public ReceiptRepository(CartAccDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(Receipt item)
        {
            dbContext.Receipts.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(Receipt item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(Receipt item)
        {
            dbContext.Receipts.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public Receipt Get(int id)
        {
            return dbContext.Receipts
                .Include(u => u.User)
                .Include(p => p.Provider)
                .Include(c => c.Cartridges).ThenInclude(c => c.Cartridge)
                .Include(o => o.Osp)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<Receipt> Find(Func<Receipt, bool> predicate)
        {
            return dbContext.Receipts
                .Include(u => u.User)
                .Include(p => p.Provider)
                .Include(c => c.Cartridges).ThenInclude(c => c.Cartridge)
                .Include(o => o.Osp)
                .Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Receipt> GetAll()
        {
            return dbContext.Receipts
                .Include(u => u.User)
                .Include(p => p.Provider)
                .Include(c => c.Cartridges).ThenInclude(c => c.Cartridge)
                .Include(o => o.Osp);
        }
    }
}
