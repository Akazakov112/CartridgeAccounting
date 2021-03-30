using System.Windows;

namespace CartAccClient.Model
{
    /// <summary>
    /// Прокси для привязки объектов вне визуального дерева xaml.
    /// </summary>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// Статическое свойство зависимости для режима редактирования.
        /// </summary>
        public static readonly DependencyProperty EditDataProperty = DependencyProperty.Register("Editable", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        /// <summary>
        /// Объект хранения значения свойства зависимости для режима редактирвоания.
        /// </summary>
        public object Editable
        {
            get
            {
                return (object)GetValue(EditDataProperty);
            }
            set
            {
                SetValue(EditDataProperty, value);
            }
        }

        /// <summary>
        /// Создать объект.
        /// </summary>
        /// <returns></returns>
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}
