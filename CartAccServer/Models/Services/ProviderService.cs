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
    /// Сервис работы с поставщиками.
    /// </summary>
    public class ProviderService : IEntityService<Provider, ProviderDTO>
    {
        private IUnitOfWork Database { get; }

        public ProviderService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<ProviderDTO> GetAll()
        {
            IEnumerable<Provider> providers = Database.Providers.GetAll();
            // Если поставщики не найдены.
            if (providers is null)
            {
                throw new ValidationException("Поставщики не получены", "");
            }
            // Создать список поставщиков Dto.
            var providersDto = providers.Select(p => new ProviderDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Email = p.Email,
                Name = p.Name,
                Active = p.Active,
                OspId = p.Osp.Id
            });
            // Вернуть DTO поставщиков.
            return new ObservableCollection<ProviderDTO>(providersDto);
        }

        public ProviderDTO Get(int id)
        {
            // Найти поставщика в бд по id.
            Provider provider = Database.Providers.Get(id);
            // Если поставщик не найден.
            if (provider is null)
            {
                throw new ValidationException("Поставщик не получен", "");
            }
            // Создать поставщика Dto.
            var providerDto = new ProviderDTO()
            {
                Id = provider.Id,
                Number = provider.Number,
                Email = provider.Email,
                Name = provider.Name,
                Active = provider.Active,
                OspId = provider.Osp.Id
            };
            // Вернуть DTO поставщика.
            return providerDto;
        }

        public ICollection<ProviderDTO> Find(Func<Provider, bool> predicate)
        {
            IEnumerable<Provider> providers = Database.Providers.Find(predicate);
            // Если поставщики не найдены.
            if (providers is null)
            {
                throw new ValidationException("Поставщики не получены", "");
            }
            // Создать список поставщиков Dto.
            var providersDto = providers.Select(p => new ProviderDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Email = p.Email,
                Name = p.Name,
                Active = p.Active,
                OspId = p.Osp.Id
            });
            // Вернуть DTO поставщиков.
            return new ObservableCollection<ProviderDTO>(providersDto);
        }

        public void Add(ProviderDTO item)
        {
            // Найти в бд связанные сущности для поставщика.
            Osp osp = Database.Osps.Get(item.OspId);
            // Найти последнего поставщика в ОСП.
            Provider lastProvider = Database.Providers.Find(x => x.Osp.Id == item.OspId).LastOrDefault();
            // Создать поставщика по данным DTO.
            Provider newProvider = new Provider()
            {
                Name = item.Name,
                Email = item.Email,
                Active = item.Active,
                Number = lastProvider is null ? 1 : lastProvider.Number + 1,
                Osp = osp
            };
            // Добавить созданного поставщика в бд.
            Database.Providers.Create(newProvider);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(ProviderDTO item)
        {
            // Найти поставщика в бд по Id.
            Provider provider = Database.Providers.Get(item.Id);
            // Изменить значение количества из Dto.
            provider.Name = item.Name;
            // Изменить значение статуса использования из Dto.
            provider.Email = item.Email;
            // Обновить значение для бд.
            Database.Providers.Update(provider);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
