using DMSA.Models.Odoo.DebitCollection;
using System.Windows.Input;

namespace DMCobranzas.Controls.CustomRows;

public partial class MultipleCobrosInvoiceLineAiRow : ContentView
{
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
            null);

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

    public MultipleCobrosInvoiceLineAiRow()
	{
		InitializeComponent();

        ClearValueCommand = new Command(ClearValue);
        ActionCommand = new Command(ActionButton);
    }

    private void ClearValue(object obj)
    {
        entryPagoImporte.Text = "0.00";
    }

    private CancellationTokenSource _cts;

    private void ActionButton(object obj)
    {
        try
        {
            
            if (ValueChangedCommand?.CanExecute(obj) == true)
            {
                ValueChangedCommand.Execute(obj);
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void Old_EntryPagoImporte_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            var currentItem = dataItem; // snapshot REAL

            if (currentItem == null) return;
            if (!currentItem.EventsOn) return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                await Task.Delay(400, token);

                if (token.IsCancellationRequested)
                    return;

                //  VALIDACIÓN CRÍTICA (AQUÍ ESTÁ LA SOLUCIÓN)
                if (currentItem != dataItem) return;        // item cambió (recycling)
                if (!currentItem.EventsOn) return;          // se desactivó mientras tanto
                if (BindingContext != currentItem) return;  // seguridad extra

                if (ValueChangedCommand?.CanExecute(currentItem) == true)
                {
                    ValueChangedCommand.Execute(currentItem);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }
        catch
        {
        }
    }

    //////private async void EntryPagoImporte_TextChanged(object sender, TextChangedEventArgs e)
    //////{
    //////    try
    //////    {
    //////        if (dataItem == null) return;
    //////        if (!dataItem.EventsOn) return;

    //////        _cts?.Cancel();
    //////        _cts = new CancellationTokenSource();
    //////        var token = _cts.Token;

    //////        try
    //////        {                
    //////            await Task.Delay(400, token);

    //////            if (token.IsCancellationRequested)
    //////                return;

    //////            if (ValueChangedCommand?.CanExecute(dataItem) == true)
    //////            {
    //////                ValueChangedCommand.Execute(dataItem);
    //////            }
    //////        }
    //////        catch (TaskCanceledException)
    //////        {
    //////            // esperado, no hacer nada
    //////        }
    //////    }
    //////    catch
    //////    {
    //////        // opcional log
    //////    }
    //////}
}