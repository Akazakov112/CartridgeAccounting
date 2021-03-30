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
    /// Сервис работы с картриджами.
    /// </summary>
    public class CartridgeService : IEntityService<Cartridge, CartridgeDTO>
    {
        private IUnitOfWork Database { get; }

        public CartridgeService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<CartridgeDTO> GetAll()
        {
            IEnumerable<Cartridge> cartridges = Database.Cartridges.GetAll();
            // Если картриджи не получены.
            if (cartridges is null)
            {
                throw new ValidationException("Картриджи не получены", "");
            }
            // Создать список картриджей Dto.
            var cartridgesDto = cartridges.Select(p => new CartridgeDTO()
            {
                Id = p.Id,
                Model = p.Model,
                InUse = p.InUse,
                Compatibility = new ObservableCollection<PrinterDTO>(p.Compatibility.Select(c =>
                    new PrinterDTO(c.Printer.Id, c.Printer.Model, new ObservableCollection<CartridgeDTO>(), c.Printer.InUse))
                )
            });
            // Вернуть DTO картриджи.
            return new ObservableCollection<CartridgeDTO>(cartridgesDto);
        }

        public CartridgeDTO Get(int id)
        {
            // Получить картридж из бд.
            Cartridge cartridge = Database.Cartridges.Get(id);
            // Если картридж не получен.
            if (cartridge is null)
            {
                throw new ValidationException("Картридж не получен", "");
            }
            // Создать картридж Dto.
            var cartridgeDto = new CartridgeDTO()
            {
                Id = cartridge.Id,
                Model = cartridge.Model,
                InUse = cartridge.InUse,
                Compatibility = new ObservableCollection<PrinterDTO>(cartridge.Compatibility.Select(c =>
                    new PrinterDTO(c.Printer.Id, c.Printer.Model, new ObservableCollection<CartridgeDTO>(), c.Printer.InUse))
                )
            };
            // Вернуть DTO картридж.
            return cartridgeDto;
        }

        public ICollection<CartridgeDTO> Find(Func<Cartridge, bool> predicate)
        {
            IEnumerable<Cartridge> cartridges = Database.Cartridges.Find(predicate);
            // Если картриджи не получены.
            if (cartridges is null)
            {
                throw new ValidationException("Картриджи не получены", "");
            }
            // Создать список картриджей Dto.
            var cartridgesDto = cartridges.Select(p => new CartridgeDTO()
            {
                Id = p.Id,
                Model = p.Model,
                InUse = p.InUse,
                Compatibility = new ObservableCollection<PrinterDTO>(p.Compatibility.Select(c =>
                    new PrinterDTO(c.Printer.Id, c.Printer.Model, new ObservableCollection<CartridgeDTO>(), c.Printer.InUse))
                )
            });
            // Вернуть DTO картриджи.
            return new ObservableCollection<CartridgeDTO>(cartridgesDto);
        }

        public void Add(CartridgeDTO item)
        {
            // Создать картридж по данным DTO.
            Cartridge newCartridge = new Cartridge()
            {
                Model = item.Model,
                InUse = item.InUse
            };
            // Добавить созданный картридж в бд.
            Database.Cartridges.Create(newCartridge);
            // Сохранить изменения.
            Database.Save();
            // Найти добавленный картридж в БД.
            Cartridge addedCartridge = Database.Cartridges.GetAll().LastOrDefault();
            // Создать список совместимости.
            List<Compatibility> compatibilities = item.Compatibility.Select(x => new Compatibility()
            {
                Cartridge = addedCartridge,
                Printer = Database.Printers.Get(x.Id)
            }).ToList();
            addedCartridge.Compatibility = compatibilities;
            // Обновить данные.
            Database.Cartridges.Update(addedCartridge);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(CartridgeDTO item)
        {
            // Найти картридж в бд по Id.
            Cartridge dbCartridge = Database.Cartridges.Get(item.Id);
            List<Compatibility> compatibilities = item.Compatibility.Select(x => new Compatibility()
            {
                Cartridge = Database.Cartridges.Get(item.Id),
                Printer = Database.Printers.Get(x.Id)
            }).ToList();
            // Изменить модель картриджа и список совместимости.
            dbCartridge.Model = item.Model;
            dbCartridge.Compatibility = compatibilities;
            dbCartridge.InUse = item.InUse;
            // Обновить значение картриджа в бд.
            Database.Cartridges.Update(dbCartridge);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
