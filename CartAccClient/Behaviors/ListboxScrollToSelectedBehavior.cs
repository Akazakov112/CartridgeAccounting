using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace CartAccClient.Behaviors
{
    /// <summary>
    /// Поведение листбокса для прокручивания списка на выбранный элемент.
    /// </summary>
    class ListboxScrollToSelectedBehavior : Behavior<ListBox>
    {
        // Получает доступ к элементу, в котором размещено поведение.
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
        }

        // Убирает функциональность от элемента после произошедшего поведения.
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.SelectionChanged -= new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
        }

        // Обработчик поведения.
        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb)
            {
                if (lb.SelectedItem != null)
                {
                    lb.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lb.ScrollIntoView(lb.SelectedItem);
                    }));
                }
            }
        }
    }
}
