using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс списания.
    /// </summary>
    public class ExpenseDTO : BaseVm, IValidatableObject
    {
        private string basis;
        private CartridgeDTO cartridge;
        private int count;
        private bool delete, edit;


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
        /// Основание.
        /// </summary>
        public string Basis
        {
            get { return basis; }
            set { basis = value; RaisePropertyChanged(nameof(Basis)); }
        }

        /// <summary>
        /// Автор расхода.
        /// </summary>
        public UserDTO User { get; set; }

        /// <summary>
        /// Взятый картридж.
        /// </summary>
        public CartridgeDTO Cartridge
        {
            get { return cartridge; }
            set { cartridge = value; RaisePropertyChanged(nameof(Cartridge)); }
        }

        /// <summary>
        /// Количество взятого картриджа.
        /// </summary>
        public int Count
        {
            get { return count; }
            set { count = value; RaisePropertyChanged(nameof(Count)); }
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
        public ExpenseDTO() { }

        /// <summary>
        /// Конструктор для создания списания пользователем.
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="basis">Основание</param>
        /// <param name="userId">Id пользователя</param>
        /// <param name="cartridgeId">Id катриджа</param>
        /// <param name="count">Количество</param>
        /// <param name="delete">Метка удаления</param>
        /// <param name="edit">Метка редактирования</param>
        public ExpenseDTO(int id, int number, DateTime date, string basis, UserDTO user, CartridgeDTO cartridge, int count, int ospId, bool delete = false, bool edit = false)
        {
            Id = id;
            Number = number;
            Date = date;
            Basis = basis;
            User = user;
            Cartridge = cartridge;
            Count = count;
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

            if (string.IsNullOrWhiteSpace(Basis))
                errors.Add(new ValidationResult("Поле \"Заявка\" не может быть пустым."));

            if (Count <= 0)
                errors.Add(new ValidationResult("Значение должно быть больше 0."));

            if (Count.ToString().Length > 3)
                errors.Add(new ValidationResult("Поле \"Количество\" может содержать только до 3 цифр."));

            return errors;
        }
    }
}
