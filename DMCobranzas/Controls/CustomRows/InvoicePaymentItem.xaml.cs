using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DMCobranzas.Controls.CustomRows;

public partial class InvoicePaymentItem : ContentView
{
    public static readonly BindableProperty dataItemProperty =
        BindableProperty.Create(
            nameof(dataItem),
            typeof(MultipleCobrosInvoiceLineAi),
            typeof(InvoicePaymentItem),
            null);

    public MultipleCobrosInvoiceLineAi dataItem
    {
        get => (MultipleCobrosInvoiceLineAi)GetValue(dataItemProperty);
        set => SetValue(dataItemProperty, value);
    }

    public ICommand ClearValueCommand { get; }

    public InvoicePaymentItem()
	{
		InitializeComponent();

        ClearValueCommand = new Command(ClearValue);
    }

    private void ClearValue(object obj)
    {
        entryPagoImporte.Text = "0.00";
    }

    //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    base.OnPropertyChanged(propertyName);

    //    if (propertyName == nameof(dataItem) && dataItem != null)
    //    {
    //        labelArticulo.Text = dataItem.invoice_line_id_name;
    //        labelTotal.Text = $"   ($ {dataItem.invoice_amount_total})";
    //        labelFechaRegistro.Text = $"FECHA: {dataItem.invoice_date:yyyy-MM-dd}";
    //        labelVendedor.Text = $"VEND: {dataItem.seller}";
    //        labelAmountResidual.Text = $"SALDO: $ {dataItem.invoice_amount_residual}";

    //        var dias = Math.Max(0, (DateTime.Now - dataItem.invoice_date).Days);
    //        labelDias.Text = $"(DIAS: {dias})";

    //        entryPagoImporte.Text =
    //            Settings.helpers.ParseTool.ConvertirAMoneda(
    //                dataItem.reconcile_amount.ToString());
    //    }
    //}

    private void EntryPagoImporte_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (dataItem == null) return;

        decimal rec;
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            var txt = entryPagoImporte.Text?.Replace(".", ",");
            if (!decimal.TryParse(txt, out rec))
            {
                entryPagoImporte.Text = "";
                return;
            }
        }
        else
        {
            decimal.TryParse(entryPagoImporte.Text, out rec);
        }
        
        dataItem.amount_asigned = rec;
        Debug.WriteLine(rec);
    }

    private void EntryPagoImporte_Focused(object sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            entryPagoImporte.SelectionLength = entryPagoImporte.Text?.Length ?? 0;
        });
    }
}