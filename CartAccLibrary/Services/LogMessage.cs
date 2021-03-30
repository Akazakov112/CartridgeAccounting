namespace CartAccLibrary.Services
{
    /// <summary>
    /// Типы сообщений.
    /// </summary>
    public enum LogMessageType : int
    {
        Notification,
        Warning,
        Error
    }

    /// <summary>
    /// Сообщения для логгирования.
    /// </summary>
    public sealed class LogMessage
    {
        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Тип сообщения.
        /// </summary>
        public LogMessageType Type { get; set; }

        /// <summary>
        /// Пустой контруктор.
        /// </summary>
        public LogMessage() 
        {
            Type = LogMessageType.Notification;
        }

        /// <summary>
        /// Конструктор с выбором типа сообщения.
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        /// <param name="type">Тип сообщения</param>
        public LogMessage(string message, LogMessageType type = LogMessageType.Notification)
        {
            Message = message;
            Type = type;
        }
    }
}
