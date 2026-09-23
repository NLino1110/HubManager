using CommunityToolkit.Maui.Alerts;
using DMCobranzas.Settings.helpers;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace DMCobranzas.AppPages.Reporteria;

public sealed class ReportTypeOption
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

public sealed class ReportStateOption
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ReporteriaPage : ContentPage
{
    private const string TipoRecibosCabecera = "recibos_cabecera";
    private const string TipoNcreCabecera = "nota_credito_cabecera";
    private const string EstadoTodos = "todos";
    private const string EstadoBorrador = "borrador";
    private const string EstadoValidado = "validado";

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged(nameof(IsLoading));
        }
    }

    readonly ReportTypeOption[] _tipos =
    [
        new ReportTypeOption { Key = TipoRecibosCabecera, DisplayName = "Recibos generados (cabecera)" },
        new ReportTypeOption { Key = TipoNcreCabecera, DisplayName = "Notas de credito generados (cabecera)" }
    ];

    readonly ReportStateOption[] _estados =
    [
        new ReportStateOption { Key = EstadoTodos, DisplayName = "Todos" },
        new ReportStateOption { Key = EstadoBorrador, DisplayName = "Borrador" },
        new ReportStateOption { Key = EstadoValidado, DisplayName = "Validado" }
    ];

    public ReporteriaPage()
    {
        InitializeComponent();

        pickerTipo.ItemsSource = _tipos;
        pickerTipo.SelectedIndex = 0;

        pickerEstado.ItemsSource = _estados;
        pickerEstado.SelectedIndex = 0;

        dateIni.Date = DateTime.Today.AddMonths(-1);
        dateEnd.Date = DateTime.Today;

        BindingContext = this;
    }

    private async void btnImportar_Clicked(object sender, EventArgs e)
    {
        if (pickerTipo.SelectedItem is not ReportTypeOption tipo)
        {
            await DisplayAlertAsync("Reportería", "Seleccione un tipo de reporte.", "Aceptar");
            return;
        }

        if (pickerEstado.SelectedItem is not ReportStateOption estado)
        {
            await DisplayAlertAsync("Reportería", "Seleccione un estado.", "Aceptar");
            return;
        }

        if (dateIni.Date > dateEnd.Date)
        {
            await DisplayAlertAsync("Reportería", "La fecha desde no puede ser mayor que la fecha hasta.", "Aceptar");
            return;
        }

        await ImportReportAsync(tipo, estado);
    }

    private async Task ImportReportAsync(ReportTypeOption tipo, ReportStateOption estado)
    {
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            switch (tipo.Key)
            {
                case TipoRecibosCabecera:
                    await ImportRecibosCabeceraAsync(estado);
                    break;
                default:
                    await DisplayAlertAsync("Reportería", "Tipo de reporte no implementado.", "Aceptar");
                    break;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Reporteria import error: " + ex.Message);
            await DisplayAlertAsync("Reportería", "No se pudo generar el reporte: " + ex.Message, "Aceptar");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportRecibosCabeceraAsync(ReportStateOption estado)
    {
        DateTime dateEndField = dateEnd.Date!.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
        var database = new MultipleCobrosInvoiceDb(App.Session.odooConnection.DbNameSqlite);

        var items = await database.GetItemsAsync(x =>
            x.create_date >= dateIni.Date &&
            x.create_date <= dateEndField &&
            x.create_uid == App.Session.CurrentUserFront.uid);

        items = items
            .Where(x => MatchesEstado(x, estado.Key))
            .OrderByDescending(x => x.create_date)
            .ToList();

        if (items.Count == 0)
        {
            await Toast.Make("No hay registros para los filtros seleccionados.").Show();
            return;
        }

        var csv = BuildRecibosCabeceraCsv(items);
        var fileName = $"recibos_cabecera_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var savedPath = await ReportFileHelper.SaveCsvToDownloadsAsync(fileName, csv);

        await DisplayAlertAsync(
            "Reportería",
            $"{items.Count} registro(s) exportado(s).\n\nArchivo guardado en:\n{savedPath}",
            "Aceptar");
    }

    private static bool MatchesEstado(MultipleCobrosInvoice item, string estadoKey)
    {
        return estadoKey switch
        {
            EstadoBorrador => string.Equals(item.state, "draft", StringComparison.OrdinalIgnoreCase),
            EstadoValidado => string.Equals(item.state, "posted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.payment_status, CobrosEstados.PROCESADO, StringComparison.OrdinalIgnoreCase)
                || string.Equals(item.payment_status, CobrosEstados.APLICADO, StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private static string BuildRecibosCabeceraCsv(IEnumerable<MultipleCobrosInvoice> items)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Recibo;Cliente;Fecha;Monto;Estado;Estado pago;Empresa");

        foreach (var item in items)
        {
            sb.AppendLine(string.Join(';',
                EscapeCsv(item.receipt_name),
                EscapeCsv(item.partner_name),
                item.create_date.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                item.amount.ToString(CultureInfo.InvariantCulture),
                EscapeCsv(item.state),
                EscapeCsv(item.payment_status_display),
                item.company_id.ToString(CultureInfo.InvariantCulture)));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        value ??= string.Empty;
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }
}
