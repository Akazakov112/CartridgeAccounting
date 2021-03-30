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
    /// Сервис работы с уровнями доступа.
    /// </summary>
    public class AccessService : IEntityService<Access, AccessDTO>
    {
        private IUnitOfWork Database { get; }

        public AccessService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<AccessDTO> GetAll()
        {
            IEnumerable<Access> accesses = Database.Accesses.GetAll();
            if (accesses is null)
            {
                throw new ValidationException("Уровни доступа не получены", "");
            }
            // Создать список уровней доступа Dto.
            var accessesDto = accesses.Select(p => new AccessDTO()
            {
                Id = p.Id,
                Name = p.Name
            });
            // Вернуть DTO уровней доступа.
            return new ObservableCollection<AccessDTO>(accessesDto);
        }

        public AccessDTO Get(int id)
        {
            Access access = Database.Accesses.Get(id);
            if (access is null)
            {
                throw new ValidationException("Уровень доступа не получены", "");
            }
            // Создать уровень доступа Dto.
            var accessesDto = new AccessDTO()
            {
                Id = access.Id,
                Name = access.Name
            };
            // Вернуть DTO уровня доступа.
            return accessesDto;
        }

        public ICollection<AccessDTO> Find(Func<Access, bool> predicate)
        {
            IEnumerable<Access> accesses = Database.Accesses.Find(predicate);
            if (accesses is null)
            {
                throw new ValidationException("Уровни доступа не получены", "");
            }
            // Создать список уровней доступа Dto.
            var accessesDto = accesses.Select(p => new AccessDTO()
            {
                Id = p.Id,
                Name = p.Name
            });
            // Вернуть DTO уровней доступа.
            return new ObservableCollection<AccessDTO>(accessesDto);
        }

        public void Add(AccessDTO item)
        {
            throw new NotImplementedException();
        }

        public void Update(AccessDTO item)
        {
            throw new NotImplementedException();
        }
    }
}
