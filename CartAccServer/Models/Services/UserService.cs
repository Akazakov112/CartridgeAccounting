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
    /// Сервис работы с пользователями.
    /// </summary>
    public class UserService : IEntityService<User, UserDTO>
    {
        private IUnitOfWork Database { get; }

        public UserService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<UserDTO> GetAll()
        {
            IEnumerable<User> users = Database.Users.GetAll();
            // Если пользователи не получены.
            if (users is null)
            {
                throw new ValidationException("Пользователи не получены", "");
            }
            // Создать список пользователей Dto.
            var usersDto = users.Select(p => new UserDTO()
            {
                Id = p.Id,
                Active = p.Active,
                Fullname = p.Fullname,
                Login = p.Login,
                Access = new AccessDTO(p.Access.Id, p.Access.Name),
                Osp = new OspDTO(p.Osp.Id, p.Osp.Name, p.Osp.Active)
            });
            // Вернуть DTO пользователей.
            return new ObservableCollection<UserDTO>(usersDto);
        }

        public UserDTO Get(int id)
        {
            User user = Database.Users.Get(id);
            // Если пользователь не получен.
            if (user is null)
            {
                throw new ValidationException("Пользователь не получен", "");
            }
            // Создать пользователя Dto.
            var usersDto = new UserDTO()
            {
                Id = user.Id,
                Active = user.Active,
                Fullname = user.Fullname,
                Login = user.Login,
                Access = new AccessDTO(user.Access.Id, user.Access.Name),
                Osp = new OspDTO(user.Osp.Id, user.Osp.Name, user.Osp.Active)
            };
            // Вернуть DTO пользователя.
            return usersDto;
        }

        public ICollection<UserDTO> Find(Func<User, bool> predicate)
        {
            IEnumerable<User> users = Database.Users.Find(predicate);
            // Если пользователи не получены.
            if (users is null)
            {
                throw new ValidationException("Пользователи не получены", "");
            }
            // Создать список пользователей Dto.
            var usersDto = users.Select(p => new UserDTO()
            {
                Id = p.Id,
                Active = p.Active,
                Fullname = p.Fullname,
                Login = p.Login,
                Access = new AccessDTO(p.Access.Id, p.Access.Name),
                Osp = new OspDTO(p.Osp.Id, p.Osp.Name, p.Osp.Active)
            });
            // Вернуть DTO пользователей.
            return new ObservableCollection<UserDTO>(usersDto);
        }

        public void Add(UserDTO item)
        {
            // Найти в бд связанные сущности для почты.
            Osp osp = Database.Osps.Get(item.Osp.Id);
            Access access = Database.Accesses.Get(item.Access.Id);
            // Создать пользователя по данным DTO.
            var newUser = new User()
            {
                Login = item.Login,
                Fullname = item.Fullname,
                Access = access,
                Active = item.Active,
                Osp = osp
            };
            // Добавить созданного пользователя в бд.
            Database.Users.Create(newUser);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(UserDTO item)
        {
            // Найти пользователя в бд по Id.
            User user = Database.Users.Get(item.Id);
            // Изменить значения из Dto.
            user.Login = item.Login;
            user.Fullname = item.Fullname;
            user.Active = item.Active;
            user.Access = Database.Accesses.Get(item.Access.Id);
            user.Osp = Database.Osps.Get(item.Osp.Id);
            // Обновить значение для бд.
            Database.Users.Update(user);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
