using CartAccLibrary.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CartAccNotifier.Models
{
    /// <summary>
    /// Построитель данных для оповещения в ОСП.
    /// </summary>
    class DataBuilder
    {
        /// <summary>
        /// Контекст базы данных.
        /// </summary>
        private readonly CartAccDbContext dbContext;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public DataBuilder(string connectionString)
        {
            DbContextOptions<CartAccDbContext> option = new DbContextOptionsBuilder<CartAccDbContext>().UseSqlServer(connectionString).Options;
            dbContext = new CartAccDbContext(option);
        }


        /// <summary>
        /// Получает список ОСП для оповещения.
        /// </summary>
        /// <param name="minBalanceCount">Минимальный остаток для оповещения</param>
        /// <returns>Список ОСП для оповещения</returns>
        public List<NotifyOsp> GetOspForNotify(int minBalanceCount)
        {
            var notifyOsps = new List<NotifyOsp>();
            List<Osp> osps = dbContext.Osps.ToList();
            foreach (var osp in osps)
            {
                var balances = dbContext.Balances.Include(c => c.Cartridge).Where(x => x.Osp.Id == osp.Id && x.Count <= minBalanceCount && x.InUse).ToList();
                var emails = dbContext.Emails.Where(x => x.Osp.Id == osp.Id && x.Active).ToList();
                if (balances.Count > 0 && emails.Count > 0)
                {
                    notifyOsps.Add(new NotifyOsp(osp.Name, balances, emails));
                }
            }
            return notifyOsps;
        }
    }
}
