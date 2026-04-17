using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

    public MultipleCobrosInvoiceLineAiRow()
	{
		InitializeComponent();

        ClearValueCommand = new Command(ClearValue);
    }

    private void ClearValue(object obj)
    {
        entryPagoImporte.Text = "0.00";
    }

    private CancellationTokenSource _cts;

    private async void EntryPagoImporte_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            if (dataItem == null) return;
            if (!dataItem.EventsOn) return;
                        
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {                
                await Task.Delay(400, token);

                if (token.IsCancellationRequested)
                    return;
                                
                if (ValueChangedCommand?.CanExecute(dataItem) == true)
                {
                    ValueChangedCommand.Execute(dataItem);
                }
            }
            catch (TaskCanceledException)
            {
                // esperado, no hacer nada
            }
        }
        catch
        {
            // opcional log
        }
    }

    private void EntryPagoImporte_Unfocused(object sender, FocusEventArgs e)
    {        
        _cts?.Cancel();

        if (ValueChangedCommand?.CanExecute(dataItem) == true)
        {
            ValueChangedCommand.Execute(dataItem);
        }
    }

    //private async void EntryPagoImporte_TextChanged(object sender, TextChangedEventArgs e)
    //{
    //    try
    //    {
    //        if (dataItem == null) return;

    //        if (!dataItem.EventsOn)
    //            return;

    //        if (ValueChangedCommand?.CanExecute(dataItem) == true)
    //        {
    //            ValueChangedCommand.Execute(dataItem);
    //        }
    //    }
    //    catch (TaskCanceledException)
    //    {

    //    }
    //}
}