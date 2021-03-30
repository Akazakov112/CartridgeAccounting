using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс поставщика.
    /// </summary>
    public class ProviderDTO : BaseVm, IValidatableObject
    {
        private string name, email;
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
        /// Название.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(nameof(Name)); }
        }

        /// <summary>
        /// Адрес электронной почты.
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; RaisePropertyChanged(nameof(Email)); }
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
        public ProviderDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id поставщика</param>
        /// <param name="number">Порядковый номер</param>
        /// <param name="name">Название</param>
        /// <param name="email">Электронная почта</param>
        /// <param name="active">Статус активности</param>
        public ProviderDTO(int id, int number, string name, string email, int ospId, bool active = true)
        {
            Id = id;
            Number = number;
            Name = name;
            Email = email;
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

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(new ValidationResult("Наименование не может быть пустым."));

            if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, emailPattern))
                errors.Add(new ValidationResult("Поле \"Адрес электронной почты\" заполнено некорректно.\nПроверьте введенные данные."));

            return errors;
        }
    }
}
