using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using CartAccServer.Models.Infrastructure;
using CartAccServer.Models.Interfaces.Repository;
using CartAccServer.Models.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CartAccServer.Models.Services
{
    /// <summary>
    /// Сервис работы с обновлениями клиента.
    /// </summary>
    public class FileClientUpdateService : IClientUpdateService
    {
        /// <summary>
        /// Объект работы с БД.
        /// </summary>
        private IUnitOfWork Database { get; }

        /// <summary>
        /// Данные среды окружения приложения.
        /// </summary>
        private IWebHostEnvironment AppEnvironment { get; }

        /// <summary>
        /// Конфигурация приложения.
        /// </summary>
        private IConfiguration AppConfiguration { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="appConfiguration"></param>
        public FileClientUpdateService(IUnitOfWork unitOfWork, IWebHostEnvironment appEnvironment, IConfiguration configuration)
        {
            Database = unitOfWork;
            AppEnvironment = appEnvironment;
            AppConfiguration = configuration;
        }


        /// <summary>
        /// Добавляет обновление в БД и скачивает файл на сервер.
        /// </summary>
        /// <param name="uploadUpdate">Объект файла с веб формы</param>
        /// <param name="description">Описание обновления</param>
        /// <param name="version">Версия обновления</param>
        public void AddUpdate(IFormFile uploadUpdate, string description, int version)
        {
            // Если переданный файл не null.
            if (uploadUpdate != null)
            {
                // Имя файла с рандомной частью.
                string path = $"/{AppConfiguration["UpdatesCatalog"]}/{Path.GetRandomFileName() + uploadUpdate.FileName}";
                // Сохранить файл в папку.
                using (var fileStream = new FileStream(AppEnvironment.WebRootPath + path, FileMode.Create))
                {
                    uploadUpdate.CopyTo(fileStream);
                }
                // Создать объект обновления.
                var updateFile = new ClientUpdate
                {
                    Date = DateTime.Now.Date,
                    Filename = path,
                    Version = version,
                    Info = description
                };
                // Добавить в бд и сохранить.
                Database.Updates.Create(updateFile);
                Database.Save();
            }
        }

        /// <summary>
        /// Проверяет начилие обновления для клиента.
        /// </summary>
        /// <param name="version">Текущая версия клиента</param>
        /// <returns>DTO объект обновления</returns>
        public ClientUpdateDTO CheckUpdate(int version)
        {
            // Найти данные последнего обновления в бд.
            ClientUpdate update = Database.Updates.Find(v => v.Version > version).OrderByDescending(x => x.Id).FirstOrDefault();
            // Если обновление не найдено.
            if (update is null)
            {
                throw new ValidationException("Установлена последняя версия программы.", "");
            }
            // Создать ссылку скачивания.
            var downloadUrl = new Uri($"http://{Environment.MachineName}:{AppConfiguration["ServerPort"]}{update.Filename}");
            // Вернуть обновление для передачи клиенту.
            return new ClientUpdateDTO(update.Date, update.Version, update.Info, update.Filename, downloadUrl);
        }

        /// <summary>
        /// Получить список всех обновлений.
        /// </summary>
        /// <returns>Список обновлений</returns>
        public IEnumerable<ClientUpdate> GetAllUpdates()
        {
            return Database.Updates.GetAll();
        }

        /// <summary>
        /// Получить последнее обновление.
        /// </summary>
        /// <returns>Dto последнего загруженного обновления</returns>
        public ClientUpdateDTO GetLastUpdate()
        {
            // Найти данные последнего обновления в бд.
            ClientUpdate update = Database.Updates.GetAll().LastOrDefault();
            // Если обновление не найдено.
            if (update is null)
            {
                throw new ValidationException("Обновления отсутствуют.", "");
            }
            // Создать ссылку скачивания.
            var downloadUrl = new Uri($"http://{Environment.MachineName}:{AppConfiguration["ServerPort"]}{update.Filename}");
            // Вернуть обновление для передачи клиенту.
            return new ClientUpdateDTO(update.Date, update.Version, update.Info, update.Filename, downloadUrl);
        }
    }
}
