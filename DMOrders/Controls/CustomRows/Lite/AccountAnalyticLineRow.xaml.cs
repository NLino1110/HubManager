using DMSA.Models.Odoo.Tareas;
using System.ComponentModel;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows.Lite;

public partial class AccountAnalyticLineRow : ContentView
{
    public static readonly BindableProperty EditCommandProperty =
        BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(AccountAnalyticLineRow));

    public ICommand EditCommand
    {
        get => (ICommand)GetValue(EditCommandProperty);
        set => SetValue(EditCommandProperty, value);
    }

    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(AccountAnalyticLineRow));

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    private INotifyPropertyChanged? _itemNotifier;

    public AccountAnalyticLineRow()
	{
		InitializeComponent();
	}

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateItemSubscription();
        UpdateActionButtonsVisibility();
    }

    private void UpdateItemSubscription()
    {
        if (_itemNotifier != null)
            _itemNotifier.PropertyChanged -= OnItemPropertyChanged;

        _itemNotifier = BindingContext as INotifyPropertyChanged;
        if (_itemNotifier != null)
            _itemNotifier.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e?.PropertyName)
            || e.PropertyName.Equals(nameof(AccountAnalyticLine.is_synchronized), StringComparison.OrdinalIgnoreCase))
        {
            MainThread.BeginInvokeOnMainThread(UpdateActionButtonsVisibility);
        }
    }

    private void UpdateActionButtonsVisibility()
    {
        bool isSynced = BindingContext is AccountAnalyticLine line
            && ProjectTaskSyncValidation.IsLineEffectivelySynced(line, headerIdSync: 0);

        if (btnEditLine != null)
            btnEditLine.IsVisible = !isSynced;

        if (btnDeleteLine != null)
            btnDeleteLine.IsVisible = !isSynced;

        if (lblSyncedIndicator != null)
            lblSyncedIndicator.IsVisible = isSynced;
    }
}
