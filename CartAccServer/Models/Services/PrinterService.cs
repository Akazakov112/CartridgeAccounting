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
    /// Сервис работы с принтерами.
    /// </summary>
    public class PrinterService : IEntityService<Printer, PrinterDTO>
    {
        private IUnitOfWork Database { get; }

        public PrinterService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<PrinterDTO> GetAll()
        {
            IEnumerable<Printer> printers = Database.Printers.GetAll();
            // Если принтеры не получены.
            if (printers is null)
            {
                throw new ValidationException("Принтеры не получены", "");
            }
            // Создать список принтеров.
            var printersDto = printers.Select(p => new PrinterDTO()
            {
                Id = p.Id,
                Model = p.Model,
                InUse = p.InUse,
                Compatibility = new ObservableCollection<CartridgeDTO>(p.Compatibility.Select(c =>
                    new CartridgeDTO(c.Cartridge.Id, c.Cartridge.Model, new ObservableCollection<PrinterDTO>(), c.Cartridge.InUse))
                )
            });
            // Вернуть DTO принтеры.
            return new ObservableCollection<PrinterDTO>(printersDto);
        }

        public PrinterDTO Get(int id)
        {
            Printer printer = Database.Printers.Get(id);
            // Если принтер не получен.
            if (printer is null)
            {
                throw new ValidationException("Принтер не получен", "");
            }
            // Создать Dto принтера.
            var printersDto = new PrinterDTO()
            {
                Id = printer.Id,
                Model = printer.Model,
                InUse = printer.InUse,
                Compatibility = new ObservableCollection<CartridgeDTO>(printer.Compatibility.Select(c =>
                    new CartridgeDTO(c.Cartridge.Id, c.Cartridge.Model, new ObservableCollection<PrinterDTO>(), c.Cartridge.InUse))
                )
            };
            // Вернуть DTO принтер.
            return printersDto;
        }

        public ICollection<PrinterDTO> Find(Func<Printer, bool> predicate)
        {
            IEnumerable<Printer> printers = Database.Printers.Find(predicate);
            // Если принтеры не получены.
            if (printers is null)
            {
                throw new ValidationException("Принтеры не получены", "");
            }
            // Создать список ОСП принтеров.
            var printersDto = printers.Select(p => new PrinterDTO()
            {
                Id = p.Id,
                Model = p.Model,
                InUse = p.InUse,
                Compatibility = new ObservableCollection<CartridgeDTO>(p.Compatibility.Select(c =>
                    new CartridgeDTO(c.Printer.Id, c.Printer.Model, new ObservableCollection<PrinterDTO>(), c.Printer.InUse))
                )
            });
            // Вернуть DTO принтеры.
            return new ObservableCollection<PrinterDTO>(printersDto);
        }

        public void Add(PrinterDTO item)
        {
            // Создать принтер по данным DTO.
            Printer newPrinter = new Printer()
            {
                Model = item.Model,
                InUse = item.InUse
            };
            // Добавить созданный принтер в бд.
            Database.Printers.Create(newPrinter);
            // Сохранить изменения.
            Database.Save();
            // Найти добавленный принтер в БД.
            Printer addedPrinter = Database.Printers.GetAll().LastOrDefault();
            // Создать список совместимости.
            List<Compatibility> compatibilities = item.Compatibility.Select(x => new Compatibility()
            {
                Cartridge = Database.Cartridges.Get(x.Id),
                Printer = addedPrinter
            }).ToList();
            addedPrinter.Compatibility = compatibilities;
            // Обновить данные.
            Database.Printers.Update(addedPrinter);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(PrinterDTO item)
        {
            // Найти принтер в бд по Id.
            Printer dbPrinter = Database.Printers.Get(item.Id);
            List<Compatibility> compatibilities = item.Compatibility.Select(x => new Compatibility()
            {
                Cartridge = Database.Cartridges.Get(x.Id),
                Printer = Database.Printers.Get(item.Id)
            }).ToList();
            // Изменить модель принтера и список совместимости.
            dbPrinter.Model = item.Model;
            dbPrinter.Compatibility = compatibilities;
            dbPrinter.InUse = item.InUse;
            // Обновить значение принтера в бд.
            Database.Printers.Update(dbPrinter);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
