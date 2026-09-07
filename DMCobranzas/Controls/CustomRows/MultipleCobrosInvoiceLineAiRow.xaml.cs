using DMCobranzas;
using DMCobranzas.Converters;
using DMSA.Models.Odoo.DebitCollection;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace DMCobranzas.Controls.CustomRows;

public partial class MultipleCobrosInvoiceLineAiRow : ContentView
{
    private static readonly DecimalToStringConverter AmountConverter = new();

    private MultipleCobrosInvoiceLineAi? _subscribedItem;
    private bool _isEntryFocused;
    private bool _suppressEntrySync;

    public static readonly BindableProperty ValueChangedCommandProperty =
    BindableProperty.Create(
        nameof(ValueChangedCommand),
        typeof(ICommand),
        typeof(MultipleCobrosInvoiceLineAiRow)
    );

    public ICommand ValueChangedCommand
    {
        get => (ICommand)GetValue(ValueChangedCommandProperty);
        set => SetValue(ValueChangedCommandProperty, value);
    }

    public static readonly BindableProperty dataItemProperty =
        BindableProperty.Create(
            nameof(dataItem),
            typeof(MultipleCobrosInvoiceLineAi),
            typeof(MultipleCobrosInvoiceLineAiRow),
            null,
            propertyChanged: OnDataItemChanged);

    public MultipleCobrosInvoiceLineAi dataItem
    {
        get => (MultipleCobrosInvoiceLineAi)GetValue(dataItemProperty);
        set => SetValue(dataItemProperty, value);
    }

    public static readonly BindableProperty ClearValueCommandProperty =
        BindableProperty.Create(nameof(ClearValueCommand), typeof(ICommand), typeof(CustomHeaderView), null);

    public ICommand ClearValueCommand
    {
        get => (ICommand)GetValue(ClearValueCommandProperty);
        set => SetValue(ClearValueCommandProperty, value);
    }

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(CustomHeaderView), null);

    public ICommand ActionCommand
    {
        get => (ICommand)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public static readonly BindableProperty ApplyAmountCommandProperty =
        BindableProperty.Create(nameof(ApplyAmountCommand), typeof(ICommand), typeof(MultipleCobrosInvoiceLineAiRow), null);

    public ICommand ApplyAmountCommand
    {
        get => (ICommand)GetValue(ApplyAmountCommandProperty);
        set => SetValue(ApplyAmountCommandProperty, value);
    }

    public MultipleCobrosInvoiceLineAiRow()
    {
        InitializeComponent();

        ClearValueCommand = new Command(ClearValue);
        ActionCommand = new Command(ActionButton);
    }

    private static void OnDataItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((MultipleCobrosInvoiceLineAiRow)bindable).AttachToDataItem(
            oldValue as MultipleCobrosInvoiceLineAi,
            newValue as MultipleCobrosInvoiceLineAi);
    }

    private void AttachToDataItem(MultipleCobrosInvoiceLineAi? oldItem, MultipleCobrosInvoiceLineAi? newItem)
    {
        if (oldItem != null)
            oldItem.PropertyChanged -= OnDataItemPropertyChanged;

        _subscribedItem = newItem;

        if (newItem != null)
            newItem.PropertyChanged += OnDataItemPropertyChanged;

        SyncEntryFromModel();
    }

    private void OnDataItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MultipleCobrosInvoiceLineAi.amount_asigned))
            SyncEntryFromModel();
    }

    private CultureInfo AmountCulture =>
        App.Session?.ApplicationCultureInfo ?? CultureInfo.CurrentCulture;

    private void SyncEntryFromModel()
    {
        if (_isEntryFocused || _suppressEntrySync || _subscribedItem == null)
            return;

        entryPagoImporte.Text = FormatAmount(_subscribedItem.amount_asigned);
    }

    private string FormatAmount(decimal value) =>
        (string)AmountConverter.Convert(value, typeof(string), null, AmountCulture)!;

    private decimal ParseEntryText(string? text) =>
        (decimal)AmountConverter.ConvertBack(text, typeof(decimal), null, AmountCulture)!;

    private void CommitEntryToModel(bool formatDisplay)
    {
        var item = dataItem;
        if (item == null)
            return;

        var parsed = ParseEntryText(entryPagoImporte.Text);

        _suppressEntrySync = true;
        try
        {
            item.amount_asigned = parsed;
        }
        finally
        {
            _suppressEntrySync = false;
        }

        if (formatDisplay)
            entryPagoImporte.Text = FormatAmount(parsed);
    }

    private void EntryPagoImporte_Focused(object? sender, FocusEventArgs e)
    {
        _isEntryFocused = true;
    }

    private void EntryPagoImporte_Unfocused(object? sender, FocusEventArgs e)
    {
        _isEntryFocused = false;
        CommitEntryToModel(formatDisplay: true);
    }

    private void ClearValue(object obj)
    {
        var item = dataItem ?? obj as MultipleCobrosInvoiceLineAi;
        if (item == null || !item.CanApplyPayment)
            return;

        _suppressEntrySync = true;
        try
        {
            item.amount_asigned = 0;
        }
        finally
        {
            _suppressEntrySync = false;
        }

        entryPagoImporte.Text = FormatAmount(0m);

        if (ValueChangedCommand?.CanExecute(item) == true)
            ValueChangedCommand.Execute(item);
    }

    private void ActionButton(object obj)
    {
        try
        {
            var item = dataItem ?? obj as MultipleCobrosInvoiceLineAi;
            if (item == null)
                return;

            CommitEntryToModel(formatDisplay: true);

            if (ApplyAmountCommand?.CanExecute(item) == true)
            {
                ApplyAmountCommand.Execute(item);
                SyncEntryFromModel();
                return;
            }

            if (ValueChangedCommand?.CanExecute(item) == true)
                ValueChangedCommand.Execute(item);

            SyncEntryFromModel();
        }
        catch (TaskCanceledException)
        {
        }
    }
}
