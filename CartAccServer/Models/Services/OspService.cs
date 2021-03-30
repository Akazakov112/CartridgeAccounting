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
    /// Сервис работы с ОСП.
    /// </summary>
    public class OspService : IEntityService<Osp, OspDTO>
    {
        private IUnitOfWork Database { get; }

        public OspService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<OspDTO> GetAll()
        {
            IEnumerable<Osp> osps = Database.Osps.GetAll();
            // Если ОСП не получены.
            if (osps is null)
            {
                throw new ValidationException("ОСП не получены", "");
            }
            // Создать список ОСП Dto.
            var ospsDto = osps.Select(p => new OspDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Active = p.Active
            });
            // Вернуть DTO ОСП.
            return new ObservableCollection<OspDTO>(ospsDto);
        }

        public OspDTO Get(int id)
        {
            // Получить ОСП из бд.
            Osp osp = Database.Osps.Get(id);
            // Если ОСП не получено.
            if (osp is null)
            {
                throw new ValidationException("ОСП не получено", "");
            }
            // Создать ОСП Dto.
            var ospsDto = new OspDTO()
            {
                Id = osp.Id,
                Name = osp.Name,
                Active = osp.Active
            };
            // Вернуть DTO ОСП.
            return ospsDto;
        }

        public ICollection<OspDTO> Find(Func<Osp, bool> predicate)
        {
            IEnumerable<Osp> osps = Database.Osps.Find(predicate);
            // Если ОСП не получены.
            if (osps is null)
            {
                throw new ValidationException("ОСП не получены", "");
            }
            // Создать список ОСП Dto.
            var ospsDto = osps.Select(p => new OspDTO()
            {
                Id = p.Id,
                Name = p.Name,
                Active = p.Active
            });
            // Вернуть DTO ОСП.
            return new ObservableCollection<OspDTO>(ospsDto);
        }

        public void Add(OspDTO item)
        {
            // Создать ОСП по данным DTO.
            var newOsp = new Osp()
            {
                Name = item.Name,
                Active = item.Active
            };
            // Добавить созданное ОСП в бд.
            Database.Osps.Create(newOsp);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(OspDTO item)
        {
            // Найти ОСП в бд по Id.
            Osp osp = Database.Osps.Get(item.Id);
            // Изменить значение наименования из Dto.
            osp.Name = item.Name;
            // Обновить значение для бд.
            Database.Osps.Update(osp);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
