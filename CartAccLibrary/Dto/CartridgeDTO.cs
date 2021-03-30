using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс картриджа.
    /// </summary>
    public class CartridgeDTO : BaseVm, IValidatableObject
    {
        private string model;
        private ObservableCollection<PrinterDTO> compatibility;
        private bool inUse;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Модель.
        /// </summary>
        public string Model
        {
            get { return model; }
            set { model = value; RaisePropertyChanged(nameof(Model)); }
        }

        /// <summary>
        /// Совместимые принтеры.
        /// </summary>
        public ObservableCollection<PrinterDTO> Compatibility
        {
            get { return compatibility; }
            set { compatibility = value; RaisePropertyChanged(nameof(Compatibility)); }
        }

        /// <summary>
        /// Статус использования.
        /// </summary>
        public bool InUse
        {
            get { return inUse; }
            set { inUse = value; RaisePropertyChanged(nameof(InUse)); }
        }

        /// <summary>
        /// Конструктор без параметров.
        /// </summary>
        public CartridgeDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id картриджа</param>
        /// <param name="model">Модель</param>
        /// <param name="compatibility">Совместимые принтеры</param>
        /// <param name="inUse">Статус использования</param>
        public CartridgeDTO(int id, string model, ObservableCollection<PrinterDTO> compatibility, bool inUse = true)
        {
            Id = id;
            Model = model;
            Compatibility = compatibility;
            InUse = inUse;
        }


        /// <summary>
        /// Проводит валидацию объекта.
        /// </summary>
        /// <param name="validationContext">Контекст валидации</param>
        /// <returns>Список ошибок</returns>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(Model))
                errors.Add(new ValidationResult("Поле \"Модель\" не может быть пустым."));

            return errors;
        }
    }
}
