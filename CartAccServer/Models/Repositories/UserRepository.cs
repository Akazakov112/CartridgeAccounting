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
    /// Репозиторий пользователей.
    /// </summary>
    public class UserRepository : IRepository<User>
    {
        /// <summary>
        /// Контекст БД.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="context">Контекст БД</param>
        public UserRepository(CartAccDbContext context)
        {
            dbContext = context;
        }


        /// <summary>
        /// Создать объект в БД.
        /// </summary>
        /// <param name="item">Новый объект</param>
        public void Create(User item)
        {
            dbContext.Users.Add(item);
        }

        /// <summary>
        /// Обновить объект в БД.
        /// </summary>
        /// <param name="item">Объект с обновленными данными</param>
        public void Update(User item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        /// <summary>
        /// Удаляет запись из БД.
        /// </summary>
        /// <param name="item">Удаляемый объект</param>
        public void Delete(User item)
        {
            dbContext.Users.Remove(item);
        }

        /// <summary>
        /// Получить объект по Id.
        /// </summary>
        /// <param name="id">Id искомого объекта</param>
        /// <returns></returns>
        public User Get(int id)
        {
            return dbContext.Users
                .Include(u => u.Access)
                .Include(o => o.Osp)
                .FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Найти все объекты по заданному условию.
        /// </summary>
        /// <param name="predicate">Делегат условий поиска</param>
        /// <returns></returns>
        public IEnumerable<User> Find(Func<User, bool> predicate)
        {
            return dbContext.Users
                .Include(u => u.Access)
                .Include(o => o.Osp)
                .Where(predicate);
        }

        /// <summary>
        /// Получить все объекты из БД.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetAll()
        {
            return dbContext.Users
                .Include(u => u.Access)
                .Include(o => o.Osp);
        } 
    }
}
