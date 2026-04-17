using DMOrders.Converters;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Odoo.Tareas;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class ActivityRow : ContentView
{
    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(nameof(Item), typeof(ProjectTask), typeof(ActivityRow), null);

    public ProjectTask Item
    {
        get => (ProjectTask)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private INotifyPropertyChanged? _itemNotifier;

    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(ActivityRow), null);

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }


    public ActivityRow()
	{
		InitializeComponent();        
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateItemSubscription();
        UpdateEditButtonVisibility();
        UpdateStateDisplay();
    }

    private void UpdateItemSubscription()
    {
        if (_itemNotifier != null)
            _itemNotifier.PropertyChanged -= OnItemPropertyChanged;

        _itemNotifier = null;

        labelId.BindingContext = BindingContext;
        labelSellerName.BindingContext = BindingContext;
        labelPlanningDate.BindingContext = BindingContext;
        labelWriteDate.BindingContext = BindingContext;
        labelState.BindingContext = BindingContext;

        if (BindingContext is INotifyPropertyChanged npc)
        {
            _itemNotifier = npc;
            _itemNotifier.PropertyChanged += OnItemPropertyChanged;
        }
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e?.PropertyName) ||
            e.PropertyName.Equals("is_synchronized", StringComparison.OrdinalIgnoreCase) ||
            e.PropertyName.Equals("state", StringComparison.OrdinalIgnoreCase) ||
            e.PropertyName.Equals("state_view", StringComparison.OrdinalIgnoreCase))
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateEditButtonVisibility();
                UpdateStateDisplay();
            });
        }
    }

    private void UpdateEditButtonVisibility()
    {
        if (buttonEdit == null || BindingContext == null) return;

        bool visible = true;

        try
        {
            var prop = BindingContext.GetType().GetProperty("is_synchronized", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop != null)
            {
                var val = prop.GetValue(BindingContext);
                if (val is bool b) visible = !b;
                else if (val is int i) visible = i == 0;
                else if (val is long l) visible = l == 0;
            }
        }
        catch { visible = true; }

        buttonEdit.IsVisible = visible;
    }

    private void UpdateStateDisplay()
    {
        if (labelState == null || BindingContext == null)
            return;

        string stateView = null;
        string rawState = null;
        bool? isSynchronized = null;

        var itemType = BindingContext.GetType();

        var pStateView = itemType.GetProperty("state_view", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        var pState = itemType.GetProperty("state", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        var pIsSync = itemType.GetProperty("is_synchronized", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        stateView = pStateView?.GetValue(BindingContext)?.ToString();
        rawState = pState?.GetValue(BindingContext)?.ToString();
        if (pIsSync != null)
        {
            var v = pIsSync.GetValue(BindingContext);
            if (v is bool b) isSynchronized = b;
            else if (v is int i) isSynchronized = i != 0;
            else if (v is long l) isSynchronized = l != 0;
        }

        if (string.IsNullOrWhiteSpace(stateView) && string.IsNullOrWhiteSpace(rawState))
        {
            if (isSynchronized.HasValue) stateView = isSynchronized.Value ? "SINCRONIZADO" : "ACTIVO";
        }

        if (string.IsNullOrWhiteSpace(stateView) && !string.IsNullOrWhiteSpace(rawState))
        {
            var rs = rawState.Trim().ToLowerInvariant();
            if (rs == "draft") stateView = (isSynchronized == true) ? "SINCRONIZADO" : "ACTIVO";
            else if (rs == "sent") stateView = "SINCRONIZADO";
            else if (rs == "done") stateView = "TERMINADO";
            else if (rs == "cancel") stateView = "CANCELADO";
            else stateView = rawState;
        }

        var converter = new StateToLabelConverter();
        var display = converter.Convert(stateView ?? string.Empty, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture)?.ToString() ?? string.Empty;

        labelState.Text = display;

        switch (display.ToUpperInvariant())
        {
            case "ACTIVO":
                labelState.TextColor = Colors.Green;
                break;
            case "SINCRONIZADA":
            case "SINCRONIZADO":
                labelState.TextColor = Colors.DodgerBlue;
                break;
            default:
                labelState.TextColor = Colors.DarkGray;
                break;
        }
    }
}