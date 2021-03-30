using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс электронной почты.
    /// </summary>
    public class EmailDTO : BaseVm, IValidatableObject
    {
        private string address;
        private bool active;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Адрес.
        /// </summary>
        public string Address
        {
            get { return address; }
            set { address = value; RaisePropertyChanged(nameof(Address)); }
        }

        /// <summary>
        /// Статус активности.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; RaisePropertyChanged(nameof(Active)); }
        }

        /// <summary>
        /// Id ОСП расположения.
        /// </summary>
        public int OspId { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public EmailDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id записи</param>
        /// <param name="number">Порядковый номер</param>
        /// <param name="address">Адрес</param>
        public EmailDTO(int id, int number, string address, int ospId, bool active = true)
        {
            Id = id;
            Number = number;
            Address = address;
            Active = active;
            OspId = ospId;
        }


        /// <summary>
        /// Проводит валидацию объекта.
        /// </summary>
        /// <param name="validationContext">Контекст валидации</param>
        /// <returns>Список ошибок</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Регулярное выражение для проверки адреса электронной почты.
            string emailPattern = @"^([a-zA-Z0-9_\.-]+)@([a-zA-Z0-9_\.-]+)\.([a-zA-Z\.]{2,6})$";

            List<ValidationResult> errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Address))
                errors.Add(new ValidationResult("Адрес не может быть пустым."));

            if (!Regex.IsMatch(Address, emailPattern))
                errors.Add(new ValidationResult("Поле \"Адрес\" заполнено некорректно.\nПроверьте введенные данные."));

            return errors;
        }
    }
}
