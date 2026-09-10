using DMSA.Models.Odoo.Accounting;
using System.Diagnostics;
using System.Windows.Input;

namespace DMCobranzas.Controls.CustomRows;

public partial class CreditNoteRequestDetailRow : ContentView
{
    public static readonly BindableProperty DataItemProperty =
        BindableProperty.Create(
            nameof(DataItem),
            typeof(credit_note_request_detail),
            typeof(CreditNoteRequestDetailRow),
            null);

    public credit_note_request_detail DataItem
    {
        get => (credit_note_request_detail) GetValue(DataItemProperty);
        set => SetValue(DataItemProperty, value);
    }


    public static readonly BindableProperty DeleteCommandProperty =
        BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(CustomHeaderView), null);

    public ICommand DeleteCommand
    {
        get => (ICommand)GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomHeaderView), null);

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ICommand ClearValueCommand { get; }

    public CreditNoteRequestDetailRow()
	{
		InitializeComponent();

        ClearValueCommand = new Command(() =>
        {
            if (DataItem == null)
                return;

            DataItem.quantity = 0;
            entryCantidad.Text = "0";

            Debug.WriteLine("Cantidad limpiada");
        });

        entryCantidad.TextChanged += EntryCantidad_TextChanged;
        entryCantidad.Focused += EntryCantidad_Focused;
    }

    private async void EntryCantidad_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataItem == null)
            return;

        if (!decimal.TryParse(e.NewTextValue, out decimal value))
            value = 0;

        if (value > DataItem.quantity_available)
        {
            value = DataItem.quantity_available;
            entryCantidad.Text = value.ToString("0.##");
            await ShowAlertAsync(
                "Atención",
                "La cantidad ingresada no puede ser mayor a la disponible.");
            DataItem.quantity = value;
            return;
        }

        DataItem.quantity = value;
    }

    private static Page? GetHostPage()
        => Application.Current?.Windows?.FirstOrDefault()?.Page;

    private static async Task ShowAlertAsync(string title, string message)
    {
        var page = GetHostPage();
        if (page != null)
            await page.DisplayAlertAsync(title, message, "Aceptar");
    }

    private void EntryCantidad_Focused(object sender, FocusEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            entryCantidad.CursorPosition = 0;
            entryCantidad.SelectionLength = entryCantidad.Text?.Length ?? 0;
        });
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (DataItem == null)
            return;

        DataItem.quantity = 0;        

        Debug.WriteLine("Cantidad limpiada");
    }
}
