using System.Windows.Input;

namespace DMCobranzas.AppPages;

public class FlyoutPageItem : ContentView
{
    public string Title { get; set; }
    public string IconSource { get; set; }
    public string FontFamily { get; set; }
    public ExecuteModeEnum ExecuteMode { get; set; }
    public Type TargetType { get; set; }
    public ICommand TargetCommand { get; set; }

    //public static readonly BindableProperty TargetCommandProperty =
    //    BindableProperty.Create(nameof(TargetCommand), typeof(ICommand), typeof(FlyoutPageItem), null);

    //public ICommand TargetCommand
    //{
    //    get => (ICommand)GetValue(TargetCommandProperty);
    //    set => SetValue(TargetCommandProperty, value);
    //}
}
