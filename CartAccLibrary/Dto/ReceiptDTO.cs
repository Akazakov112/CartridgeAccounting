using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс поступления.
    /// </summary>
    public class ReceiptDTO : BaseVm, IValidatableObject
    {
        private string comment;
        private bool delete, edit;
        private ProviderDTO provider;
        private ObservableCollection<ReceiptCartridgeDTO> cartridges;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Дата.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Автор поступления.
        /// </summary>
        public UserDTO User { get; set; }

        /// <summary>
        /// Поставщик.
        /// </summary>
        public ProviderDTO Provider
        {
            get { return provider; }
            set { provider = value; RaisePropertyChanged(nameof(Provider)); }
        }

        /// <summary>
        /// Список поступивших картриджей.
        /// </summary>
        public ObservableCollection<ReceiptCartridgeDTO> Cartridges
        {
            get { return cartridges; }
            set { cartridges = value; RaisePropertyChanged(nameof(Cartridges)); }
        }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string Comment
        {
            get { return comment; }
            set { comment = value; RaisePropertyChanged(nameof(Comment)); }
        }

        /// <summary>
        /// Пометка об удалении.
        /// </summary>
        public bool Delete
        {
            get { return delete; }
            set { delete = value; RaisePropertyChanged(nameof(Delete)); }
        }

        /// <summary>
        /// Пометка о редактировании.
        /// </summary>
        public bool Edit
        {
            get { return edit; }
            set { edit = value; RaisePropertyChanged(nameof(Edit)); }
        }

        /// <summary>
        /// Id ОСП расположения.
        /// </summary>
        public int OspId { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public ReceiptDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id поступления</param>
        /// <param name="number">Номер</param>
        /// <param name="date">Дата</param>
        /// <param name="user">Автор</param>
        /// <param name="provider">Поставщик</param>
        /// <param name="cartridges">Список картриджей</param>
        /// <param name="comment">Комментарий</param>
        /// <param name="delete">Метка удаления</param>
        /// <param name="edit">Метка редактирвоания</param>
        public ReceiptDTO(int id, int number, DateTime date, UserDTO user, ProviderDTO provider,
            ObservableCollection<ReceiptCartridgeDTO> cartridges, string comment, int ospId, bool delete = false, bool edit = false)
        {
            Id = id;
            Number = number;
            Date = date;
            User = user;
            Provider = provider;
            Cartridges = cartridges;
            Comment = comment;
            Delete = delete;
            Edit = edit;
            OspId = ospId;
        }

        /// <summary>
        /// Проводит валидацию объекта.
        /// </summary>
        /// <param name="validationContext">Контекст валидации</param>
        /// <returns>Список ошибок</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (Cartridges.Count == 0)
                errors.Add(new ValidationResult("Не добавлены картриджи."));

            return errors;
        }
    }
}
