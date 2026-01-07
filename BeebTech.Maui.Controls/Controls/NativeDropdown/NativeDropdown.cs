using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeebTech.Maui.Controls.Controls.NativeDropdown
{
    public class NativeDropdown : View
    {
        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(
                nameof(ItemsSource),
                typeof(IList<string>),
                typeof(NativeDropdown),
                null);

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(
                nameof(SelectedItem),
                typeof(string),
                typeof(NativeDropdown),
                null,
                BindingMode.TwoWay);

        public IList<string>? ItemsSource
        {
            get => (IList<string>?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string? SelectedItem
        {
            get => (string?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
    }
}
