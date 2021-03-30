using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CartAccLibrary.EventArgs;
using CartAccLibrary.Services;

namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс пользователя.
    /// </summary>
    public class UserDTO : BaseVm, IValidatableObject
    {
        private string login, fullname;
        private OspDTO osp;
        private AccessDTO access;
        private bool active;

        /// <summary>
        /// Делегат, представляющий событие PropChanged.
        /// </summary>
        /// <param name="e"></param>
        public delegate void UserHandler(UserChangeEventArgs e);

        /// <summary>
        /// Событие изменения свойств.
        /// </summary>
        public event UserHandler PropChanged;


        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public string Login
        {
            get { return login; }
            set { login = value; RaisePropertyChanged(nameof(Login)); }
        }

        /// <summary>
        /// Полное имя.
        /// </summary>
        public string Fullname
        {
            get { return fullname; }
            set
            {
                if (fullname != null && fullname != value)
                {
                    PropChanged?.Invoke(new UserChangeEventArgs(UserChangeEventArgs.Props.Fullname, value));
                }
                fullname = value;
                RaisePropertyChanged(nameof(Fullname));
            }
        }

        /// <summary>
        /// ОСП, к которому относится пользователь.
        /// </summary>
        public OspDTO Osp
        {
            get { return osp; }
            set
            {
                if (osp != null && osp.Id != value.Id)
                {
                    PropChanged?.Invoke(new UserChangeEventArgs(UserChangeEventArgs.Props.Osp, value.Name));
                }
                osp = value;
                RaisePropertyChanged(nameof(Osp));
            }
        }

        /// <summary>
        /// Уровень доступа.
        /// </summary>
        public AccessDTO Access
        {
            get { return access; }
            set
            {
                if (access != null && access.Id != value.Id)
                {
                    PropChanged?.Invoke(new UserChangeEventArgs(UserChangeEventArgs.Props.Access, value.Name));
                }
                access = value;
                RaisePropertyChanged(nameof(Access));
            }
        }

        /// <summary>
        /// Статус активности.
        /// </summary>
        public bool Active
        {
            get { return active; }
            set
            {
                if (active != value)
                {
                    PropChanged?.Invoke(new UserChangeEventArgs(UserChangeEventArgs.Props.Active, value ? "Доступ предоставлен" : "Доступ закрыт"));
                }
                active = value;
                RaisePropertyChanged(nameof(Active));
            }
        }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public UserDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id пользователя</param>
        /// <param name="login">Логин</param>
        /// <param name="fullname">Полное имя</param>
        /// <param name="email">Адрес электронной почты</param>
        /// <param name="osp">ОСП пользователя</param>
        /// <param name="access">Уровень доступа</param>
        /// <param name="active">Статус активности</param>
        public UserDTO(int id, string login, string fullname, OspDTO osp, AccessDTO access, bool active = true)
        {
            Id = id;
            Login = login;
            Fullname = fullname;
            Osp = osp;
            Access = access;
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

            if (string.IsNullOrWhiteSpace(Login))
                errors.Add(new ValidationResult("Проведите поиск пользователя."));

            if (string.IsNullOrWhiteSpace(Fullname))
                errors.Add(new ValidationResult("Ф.И.О. не может быть пустым."));

            if (Osp is null || Osp.Id == 0)
                errors.Add(new ValidationResult("Выберите ОСП."));

            if (Access is null || Access.Id == 0)
                errors.Add(new ValidationResult("Выберите уровень доступа."));

            return errors;
        }
    }
}
