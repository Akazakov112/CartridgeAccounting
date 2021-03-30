using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс ОСП.
    /// </summary>
    public class OspDTO : BaseVm, IValidatableObject
    {
        private string name;
        private bool active;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(nameof(Name)); }
        }

        /// <summary>
        /// Статус подключения к учету.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set { active = value; RaisePropertyChanged(nameof(Active)); }
        }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public OspDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id ОСП</param>
        /// <param name="name">Название</param>
        /// <param name="address">Адрес</param>
        /// <param name="active">Статус подключения к учету</param>
        public OspDTO(int id, string name, bool active = true)
        {
            Id = id;
            Name = name;
            Active = active;
        }


        /// <summary>
        /// Проводит валидацию объекта.
        /// </summary>
        /// <param name="validationContext">Контекст валидации</param>
        /// <returns>Список ошибок</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Name))
                errors.Add(new ValidationResult("Название не может быть пустым."));

            return errors;
        }
    }
}
