using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using CartAccServer.Models.Interfaces.Services;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Сервис работы с записями баланса ОСП.
    /// </summary>
    public class BalanceService : IEntityService<Balance, BalanceDTO>
    {
        private IUnitOfWork Database { get; }

        public BalanceService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<BalanceDTO> GetAll()
        {
            IEnumerable<Balance> balance = Database.Balances.GetAll();
            // Если баланс не найдено.
            if (balance is null)
            {
                throw new ValidationException("Баланс не получен", "");
            }
            // Создать список баланса Dto.
            var balanceDto = balance.Select(p => new BalanceDTO()
            {
                Id = p.Id,
                Count = p.Count,
                Cartridge = new CartridgeDTO(p.Cartridge.Id, p.Cartridge.Model, new ObservableCollection<PrinterDTO>(), p.Cartridge.InUse),
                InUse = p.InUse,
                OspId = p.Osp.Id
            });
            // Вернуть DTO баланса.
            return new ObservableCollection<BalanceDTO>(balanceDto);
        }

        public BalanceDTO Get(int id)
        {
            // Найти баланс с бд.
            Balance balance = Database.Balances.Get(id);
            // Если баланс не найден.
            if (balance is null)
            {
                throw new ValidationException("Баланс не получен", "");
            }
            // Создать баланс Dto.
            var balanceDto = new BalanceDTO()
            {
                Id = balance.Id,
                Count = balance.Count,
                Cartridge = new CartridgeDTO(balance.Cartridge.Id, balance.Cartridge.Model, new ObservableCollection<PrinterDTO>(), balance.Cartridge.InUse),
                InUse = balance.InUse,
                OspId = balance.Osp.Id
            };
            // Вернуть DTO баланса.
            return balanceDto;
        }

        public ICollection<BalanceDTO> Find(Func<Balance, bool> predicate)
        {
            IEnumerable<Balance> balance = Database.Balances.Find(predicate);
            // Если баланс не найдено.
            if (balance is null)
            {
                throw new ValidationException("Баланс не получен", "");
            }
            // Создать список баланса Dto.
            var balanceDto = balance.Select(p => new BalanceDTO()
            {
                Id = p.Id,
                Count = p.Count,
                Cartridge = new CartridgeDTO(p.Cartridge.Id, p.Cartridge.Model, new ObservableCollection<PrinterDTO>(), p.Cartridge.InUse),
                InUse = p.InUse,
                OspId = p.Osp.Id
            });
            // Вернуть DTO баланса.
            return new ObservableCollection<BalanceDTO>(balanceDto);
        }

        public void Add(BalanceDTO item)
        {
            throw new NotImplementedException();
        }

        public void Update(BalanceDTO item)
        {
            // Найти баланс в бд по Id.
            Balance balance = Database.Balances.Get(item.Id);
            // Изменить значение количества из Dto.
            balance.Count = item.Count;
            // Изменить значение статуса использования из Dto.
            balance.InUse = item.InUse;
            // Обновить значение для бд.
            Database.Balances.Update(balance);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
