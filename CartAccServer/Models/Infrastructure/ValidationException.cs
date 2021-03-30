using System;

namespace CartAccServer.Models.Infrastructure
{
    /// <summary>
    /// Исключение валидации. (Тестовый, возможно добавлю доп данные) 
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Название некорректного свойства модели.
        /// </summary>
        public string Property { get; protected set; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="prop"></param>
        public ValidationException(string message, string prop) : base(message)
        {
            Property = prop;
        }
    }
}
