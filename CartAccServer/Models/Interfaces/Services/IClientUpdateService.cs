using System.Collections.Generic;
using CartAccLibrary.Dto;
using CartAccLibrary.Entities;
using Microsoft.AspNetCore.Http;

namespace CartAccServer.Models.Interfaces.Services
{
    /// <summary>
    /// ИНтерфейс сервиса работы с обновлениями клиента.
    /// </summary>
    public interface IClientUpdateService
    {
        /// <summary>
        /// Получить список всех обновлений.
        /// </summary>
        /// <returns>Список обновлений</returns>
        IEnumerable<ClientUpdate> GetAllUpdates();

        /// <summary>
        /// Проверяет начилие обновления для клиента.
        /// </summary>
        /// <param name="version">Текущая версия клиента</param>
        /// <returns>DTO объект обновления</returns>
        ClientUpdateDTO CheckUpdate(int version);

        /// <summary>
        /// Получить последнее обновление.
        /// </summary>
        /// <returns>Dto последнего загруженного обновления</returns>
        ClientUpdateDTO GetLastUpdate();

        /// <summary>
        /// Добавляет обновление в БД и скачивает файл на сервер.
        /// </summary>
        /// <param name="uploadUpdate">Объект файла с веб формы</param>
        /// <param name="description">Описание обновления</param>
        /// <param name="version">Версия обновления</param>
        void AddUpdate(IFormFile uploadUpdate, string description, int version);
    }
}
