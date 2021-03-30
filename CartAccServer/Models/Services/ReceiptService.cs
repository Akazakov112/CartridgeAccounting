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
    /// Сервис работы с поступлениями.
    /// </summary>
    public class ReceiptService : IEntityService<Receipt, ReceiptDTO>
    {
        private IUnitOfWork Database { get; }

        public ReceiptService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<ReceiptDTO> GetAll()
        {
            IEnumerable<Receipt> receipts = Database.Receipts.GetAll();
            // Если поступления не найдены.
            if (receipts is null)
            {
                throw new ValidationException("Поступления не получены", "");
            }
            // Создать список поступлений Dto.
            var receiptsDto = receipts.Select(p => new ReceiptDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Comment = p.Comment,
                Provider = p.Provider is null 
                    ? null 
                    : new ProviderDTO(p.Provider.Id, p.Provider.Number, p.Provider.Name, p.Provider.Email, p.Provider.Osp.Id, p.Provider.Active),
                Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(p.Cartridges.Select(x =>
                    new ReceiptCartridgeDTO(x.Id, new CartridgeDTO(x.Cartridge.Id, x.Cartridge.Model, new ObservableCollection<PrinterDTO>(), x.Cartridge.InUse), x.Count))),
                Date = p.Date,
                Delete = p.Delete,
                Edit = p.Edit,
                User = new UserDTO(p.User.Id, p.User.Login, p.User.Fullname, new OspDTO(), new AccessDTO(), p.User.Active),
                OspId = p.Osp.Id
            });
            // Вернуть DTO поступлений.
            return new ObservableCollection<ReceiptDTO>(receiptsDto);
        }

        public ReceiptDTO Get(int id)
        {
            Receipt receipt = Database.Receipts.Get(id);
            // Если поступление не найдено.
            if (receipt is null)
            {
                throw new ValidationException("Поступление не получено", "");
            }
            // Создать поступление Dto.
            var receiptDto = new ReceiptDTO()
            {
                Id = receipt.Id,
                Number = receipt.Number,
                Comment = receipt.Comment,
                Provider = receipt.Provider is null 
                    ? null 
                    : new ProviderDTO(receipt.Provider.Id, receipt.Provider.Number, receipt.Provider.Name, receipt.Provider.Email, receipt.Provider.Osp.Id, receipt.Provider.Active),
                Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(receipt.Cartridges.Select(x =>
                    new ReceiptCartridgeDTO(x.Id, new CartridgeDTO(x.Cartridge.Id, x.Cartridge.Model, new ObservableCollection<PrinterDTO>(), x.Cartridge.InUse), x.Count))),
                Date = receipt.Date,
                Delete = receipt.Delete,
                Edit = receipt.Edit,
                User = new UserDTO(receipt.User.Id, receipt.User.Login, receipt.User.Fullname, new OspDTO(), new AccessDTO(), receipt.User.Active),
                OspId = receipt.Osp.Id
            };
            // Вернуть DTO поступления.
            return receiptDto;
        }

        public ICollection<ReceiptDTO> Find(Func<Receipt, bool> predicate)
        {
            IEnumerable<Receipt> receipts = Database.Receipts.Find(predicate);
            // Если поступления не найдены.
            if (receipts is null)
            {
                throw new ValidationException("Поступления не получены", "");
            }
            // Создать список поступлений Dto.
            var receiptsDto = receipts.Select(p => new ReceiptDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Comment = p.Comment,
                Provider = p.Provider is null 
                    ? new ProviderDTO() 
                    : new ProviderDTO(p.Provider.Id, p.Provider.Number, p.Provider.Name, p.Provider.Email, p.Provider.Osp.Id, p.Provider.Active),
                Cartridges = new ObservableCollection<ReceiptCartridgeDTO>(p.Cartridges.Select(x =>
                    new ReceiptCartridgeDTO(x.Id, new CartridgeDTO(x.Cartridge.Id, x.Cartridge.Model, new ObservableCollection<PrinterDTO>(), x.Cartridge.InUse), x.Count))),
                Date = p.Date,
                Delete = p.Delete,
                Edit = p.Edit,
                User = new UserDTO(p.User.Id, p.User.Login, p.User.Fullname, new OspDTO(), new AccessDTO(), p.User.Active),
                OspId = p.Osp.Id
            });
            // Вернуть DTO поступлений.
            return new ObservableCollection<ReceiptDTO>(receiptsDto);
        }

        public void Add(ReceiptDTO item)
        {
            // Найти в бд связанные сущности для поступления.
            User user = Database.Users.Get(item.User.Id);
            Osp osp = Database.Osps.Get(item.OspId);
            Provider provider = item.Provider is null ? null : Database.Providers.Get(item.Provider.Id);
            // Создать список катриджей для поступления из Dto.
            List<ReceiptCartridge> repCarts = item.Cartridges.Select(x => new ReceiptCartridge()
            {
                Cartridge = Database.Cartridges.Get(x.Cartridge.Id),
                Count = x.Count
            }).ToList();
            // Перебрать картриджи поступления.
            foreach (var cart in repCarts)
            {
                // Найти баланс в ОСП для картриджа.
                Balance balance = Database.Balances.Find(x => x.Osp.Id == item.OspId && x.Cartridge.Id == cart.Cartridge.Id).FirstOrDefault();
                // Если баланс не найден (новый картридж)
                if (balance is null)
                {
                    // Создать новый объект баланса.
                    Balance newBalance = new Balance()
                    {
                        Osp = osp,
                        Count = cart.Count,
                        Cartridge = cart.Cartridge,
                        InUse = true
                    };
                    Database.Balances.Create(newBalance);
                }
                // Иначе, баланс найден.
                else
                {
                    // Прибавить к остатку количество картриджей из поступления.
                    balance.Count += cart.Count;
                    // Обновить баланс в БД.
                    Database.Balances.Update(balance);
                }
            }
            // Найти последнее поступление в ОСП.
            Receipt lastReceipt = Database.Receipts.Find(x => x.Osp.Id == item.OspId).LastOrDefault();
            // Создать списание по данным DTO.
            Receipt newReceipt = new Receipt()
            {
                Date = item.Date,
                Number = lastReceipt is null ? 1 : lastReceipt.Number + 1,
                Comment = item.Comment,
                Provider = provider,
                Cartridges = repCarts,
                Delete = item.Delete,
                Edit = item.Edit,
                User = user,
                Osp = osp
            };
            // Добавить созданное поступление в бд.
            Database.Receipts.Create(newReceipt);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(ReceiptDTO item)
        {
            // Найти поступление в бд по Id.
            Receipt dbReceipt = Database.Receipts.Get(item.Id);
            // Если была установлена пометка удаления.
            if (item.Delete == true && dbReceipt.Delete == false)
            {
                // Перебрать картриджи поступления в бд.
                foreach (ReceiptCartridge cart in dbReceipt.Cartridges)
                {
                    // Найти баланс для картриджа поступления.
                    Balance balance = Database.Balances.Find(x => x.Osp.Id == dbReceipt.Osp.Id && x.Cartridge.Id == cart.Cartridge.Id).FirstOrDefault();
                    // Вычесть из остатка количество из поступления.
                    balance.Count -= cart.Count;
                    // Обновить значение баланса в бд.
                    Database.Balances.Update(balance);
                }
                // Изменить метку удаления.
                dbReceipt.Delete = item.Delete;
            }
            // Если пометка удаления была снята.
            else if (item.Delete == false && dbReceipt.Delete == true)
            {
                // Перебрать картриджи измененного поступления.
                foreach (ReceiptCartridgeDTO cart in item.Cartridges)
                {
                    // Найти баланс для картриджа поступления.
                    Balance balance = Database.Balances.Find(x => x.Osp.Id == dbReceipt.Osp.Id && x.Cartridge.Id == cart.Cartridge.Id).FirstOrDefault();
                    // Прибавить в остаток количество из поступления.
                    balance.Count += cart.Count;
                    // Обновить значение баланса в бд.
                    Database.Balances.Update(balance);
                }
                // Изменить метку удаления.
                dbReceipt.Delete = item.Delete;
            }
            // Если метка удаления не менялась.
            else
            {
                // Обработка удаленных при редактировании картриджей.
                foreach (ReceiptCartridge dbRecCart in dbReceipt.Cartridges)
                {
                    // Найти картридж в измененном поступлении.
                    ReceiptCartridgeDTO editReceiptCartridge = item.Cartridges.FirstOrDefault(x => x.Id == dbRecCart.Id);
                    // Если такого картриджа в поступлении нет (удален при редактировании).
                    if (editReceiptCartridge is null)
                    {
                        // Найти баланс для картриджа поступления.
                        Balance balance = Database.Balances.Find(x => x.Osp.Id == dbReceipt.Osp.Id && x.Cartridge.Id == dbRecCart.Cartridge.Id).FirstOrDefault();
                        // Вычесть из остатка количество из поступления в БД.
                        balance.Count -= dbRecCart.Count;
                        // Обновить значение баланса в бд.
                        Database.Balances.Update(balance);
                        // Удалить картридж из списка поступления.
                        Database.ReceiptCartridges.Delete(dbRecCart);
                    }
                }
                // Обработка добавленных и измененных при редактировании картриджей.
                foreach (ReceiptCartridgeDTO editRecCart in item.Cartridges)
                {
                    // Если картридж новый.
                    if (editRecCart.Id == 0)
                    {
                        // Найти баланс для этого картриджа.
                        Balance balance = Database.Balances.Find(x => x.Osp.Id == dbReceipt.Osp.Id && x.Cartridge.Id == editRecCart.Cartridge.Id).FirstOrDefault();
                        // Если такого картриджа не было на балансе.
                        if (balance is null)
                        {
                            // Найти картридж в бд.
                            Cartridge cartridge = Database.Cartridges.Get(editRecCart.Cartridge.Id);
                            // Найти ОСП в БД.
                            Osp osp = Database.Osps.Get(dbReceipt.Osp.Id);
                            // Создать новую запись баланса.
                            Balance newBalance = new Balance()
                            {
                                Cartridge = cartridge,
                                Osp = osp,
                                Count = editRecCart.Count,
                                InUse = true
                            };
                            // Добавить новый баланс.
                            Database.Balances.Create(newBalance);
                        }
                        // Если картридж уже был на складе ОСП.
                        else
                        {
                            // Прибавить к остатку количество из поступления.
                            balance.Count += editRecCart.Count;
                            // Обновить значение баланса в бд.
                            Database.Balances.Update(balance);
                        }
                        // Найти картридж в БД.
                        Cartridge dbCart = Database.Cartridges.Get(editRecCart.Cartridge.Id);
                        // Создать картридж для поступления.
                        ReceiptCartridge newReceiptCart = new ReceiptCartridge()
                        {
                            Cartridge = dbCart,
                            Count = editRecCart.Count
                        };
                        // Добавить картридж в список поступления.
                        dbReceipt.Cartridges.Add(newReceiptCart);
                    }
                    // Если картридж уже был в поступлении.
                    else
                    {
                        // Найти этот картридж в данных поступления БД.
                        ReceiptCartridge dbReceiptCartridge = dbReceipt.Cartridges.FirstOrDefault(x => x.Id == editRecCart.Id);
                        // Найти баланс для этого картриджа поступления.
                        Balance balance = Database.Balances.Find(x => x.Osp.Id == dbReceipt.Osp.Id && x.Cartridge.Id == dbReceiptCartridge.Cartridge.Id).FirstOrDefault();
                        // Если количество увеличилось.
                        if (editRecCart.Count > dbReceiptCartridge.Count)
                        {
                            // Прибавить в остаток разницу в количестве.
                            balance.Count += editRecCart.Count - dbReceiptCartridge.Count;
                        }
                        // Если количество уменьшилось.
                        else if (editRecCart.Count < dbReceiptCartridge.Count)
                        {
                            // Вычесть из остатка разницу в количестве.
                            balance.Count -= dbReceiptCartridge.Count - editRecCart.Count;
                        }
                        // Сохранить новое количество.
                        dbReceiptCartridge.Count = editRecCart.Count;
                        // Обновить значение баланса в бд.
                        Database.Balances.Update(balance);
                    }
                }
            }
            // Присвоить новые значения изменяемым свойствам.
            dbReceipt.Comment = item.Comment;
            dbReceipt.Provider = item.Provider is null ? null : Database.Providers.Get(item.Provider.Id);
            // Обновить значение поступления в бд.
            Database.Receipts.Update(dbReceipt);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
