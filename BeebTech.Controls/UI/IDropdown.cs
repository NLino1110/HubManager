using System.Collections;

namespace BeebTech.Controls.UI;
public interface IDropdown : IView
{
    TextAlignment HorizontalTextAlignment { get; set; }

    Color PlaceholderColor { get; set; }

    string Placeholder { get; set; }

    object SelectedItem { get; set; }

    IList ItemsSource { get; set; }

    BindingBase ItemDisplayBinding { get; set; }
}
