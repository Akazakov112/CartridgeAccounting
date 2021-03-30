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
    /// Репозиторий списаний.
    /// </summary>
    public class ExpenseRepository : IRepository<Expense>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public ExpenseRepository(CartAccDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(Expense item)
        {
            dbContext.Expenses.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(Expense item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(Expense item)
        {
            dbContext.Expenses.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public Expense Get(int id)
        {
            return dbContext.Expenses
                .Include(u => u.User)
                .Include(c => c.Cartridge)
                .Include(o => o.Osp)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<Expense> Find(Func<Expense, bool> predicate)
        {
            return dbContext.Expenses
                .Include(u => u.User)
                .Include(c => c.Cartridge)
                .Include(o => o.Osp)
                .Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Expense> GetAll()
        {
            return dbContext.Expenses
                .Include(u => u.User)
                .Include(c => c.Cartridge)
                .Include(o => o.Osp);
        }
    }
}
