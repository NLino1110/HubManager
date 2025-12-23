using DMOrders.Converters;
using DMSA.Models.Odoo.DMOrders.tareas;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRowsV2;

public partial class ActivityRow : ContentView
{
    INotifyPropertyChanged? _itemNotifier;

    public ActivityRow()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty ItemProperty =
            BindableProperty.Create(nameof(Item),
                typeof(ProjectTask),
                typeof(ActivityRow),
                null,
                propertyChanged: OnItemChanged);

    public ProjectTask Item
    {
        get => (ProjectTask)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand),
            typeof(ICommand),
            typeof(ActivityRow));

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var row = (ActivityRow)bindable;
        row.SubscribeItem(oldValue as INotifyPropertyChanged, newValue as INotifyPropertyChanged);
        row.UpdateStateDisplay();
        row.UpdateEditButtonVisibility();
    }

    void SubscribeItem(INotifyPropertyChanged? oldItem, INotifyPropertyChanged? newItem)
    {
        if (oldItem != null)
            oldItem.PropertyChanged -= OnItemPropertyChanged;

        if (newItem != null)
            newItem.PropertyChanged += OnItemPropertyChanged;

        _itemNotifier = newItem;
    }

    void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateEditButtonVisibility();
            UpdateStateDisplay();
        });
    }

    void UpdateEditButtonVisibility()
    {
        bool visible = true;

        if (Item != null)
        {
            var prop = Item.GetType()
                .GetProperty("is_synchronized", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            var val = prop?.GetValue(Item);
            if (val is bool b) visible = !b;
            else if (val is int i) visible = i == 0;
        }

        EditButton.IsVisible = visible;
    }

    void UpdateStateDisplay()
    {
        if (Item == null)
        {
            StateLabel.Text = "";
            StateLabel.TextColor = Colors.DarkGray;
            return;
        }

        var converter = new StateToLabelConverter();
        var text = converter.Convert(Item.state_view, typeof(string), null, null)?.ToString() ?? "";

        StateLabel.Text = text;

        StateLabel.TextColor = text.ToUpperInvariant() switch
        {
            "ACTIVO" => Colors.Green,
            "SINCRONIZADO" => Colors.DodgerBlue,
            _ => Colors.DarkGray
        };
    }
}