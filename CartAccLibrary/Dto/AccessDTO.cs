namespace CartAccLibrary.Dto
{
    /// <summary>
    /// DTO класс уровня доступа.
    /// </summary>
    public class AccessDTO
    {
        /// <summary>
        /// Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Пустой конструктор.
        /// </summary>
        public AccessDTO() { }

        /// <summary>
        /// Полный конструктор.
        /// </summary>
        /// <param name="id">Id доступа</param>
        /// <param name="name">Наименование</param>
        public AccessDTO(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
