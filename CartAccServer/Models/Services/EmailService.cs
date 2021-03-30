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
    /// Сервис работы с электронной почтой ОСП.
    /// </summary>
    public class EmailService : IEntityService<Email, EmailDTO>
    {
        private IUnitOfWork Database { get; }

        public EmailService(IUnitOfWork unitOfWork)
        {
            Database = unitOfWork;
        }


        public ICollection<EmailDTO> GetAll()
        {
            IEnumerable<Email> emails = Database.Emails.GetAll();
            // Если почта не найдена.
            if (emails is null)
            {
                throw new ValidationException("Адреса электронной почты не получены", "");
            }
            // Создать список почты Dto.
            var emailsDto = emails.Select(p => new EmailDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Address = p.Address,
                Active = p.Active,
                OspId = p.Osp.Id
            });
            // Вернуть DTO почты.
            return new ObservableCollection<EmailDTO>(emailsDto);
        }

        public EmailDTO Get(int id)
        {
            // Получить почту из бд.
            Email email = Database.Emails.Get(id);
            // Если почта не найдена.
            if (email is null)
            {
                throw new ValidationException("Адрес электронной почты не получен", "");
            }
            // Создать почту Dto.
            var emailDto = new EmailDTO()
            {
                Id = email.Id,
                Number = email.Number,
                Address = email.Address,
                Active = email.Active,
                OspId = email.Osp.Id
            };
            // Вернуть DTO почты.
            return emailDto;

        }

        public ICollection<EmailDTO> Find(Func<Email, bool> predicate)
        {
            IEnumerable<Email> emails = Database.Emails.Find(predicate);
            // Если почта не найдена.
            if (emails is null)
            {
                throw new ValidationException("Адреса электронной почты не получены", "");
            }
            // Создать список почты Dto.
            var emailsDto = emails.Select(p => new EmailDTO()
            {
                Id = p.Id,
                Number = p.Number,
                Address = p.Address,
                Active = p.Active,
                OspId = p.Osp.Id
            });
            // Вернуть DTO почты.
            return new ObservableCollection<EmailDTO>(emailsDto);
        }

        public void Add(EmailDTO item)
        {
            // Найти в бд связанные сущности для почты.
            Osp osp = Database.Osps.Get(item.OspId);
            // Найти последнюю запись в ОСП.
            Email lastEmail = Database.Emails.Find(x => x.Osp.Id == item.OspId).LastOrDefault();
            // Создать почту по данным DTO.
            Email newEmail = new Email()
            {
                Address = item.Address,
                Active = item.Active,
                Number = lastEmail is null ? 1 : lastEmail.Number + 1,
                Osp = osp
            };
            // Добавить созданную почту в бд.
            Database.Emails.Create(newEmail);
            // Сохранить изменения.
            Database.Save();
        }

        public void Update(EmailDTO item)
        {
            // Найти почту в бд по Id.
            Email email = Database.Emails.Get(item.Id);
            // Изменить значение адреса и статуса из Dto.
            email.Address = item.Address;
            email.Active = item.Active;
            // Обновить значение для бд.
            Database.Emails.Update(email);
            // Сохранить изменения.
            Database.Save();
        }
    }
}
