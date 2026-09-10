namespace DMCobranzas.Settings.helpers;

/// <summary>
/// Cambiar <see cref="UseColumnDetailRowLayout"/> a false y restaurar
/// Controls/CustomRows/CreditNoteRequestDetailRow.Legacy.xaml sobre CreditNoteRequestDetailRow.xaml
/// para revertir el diseño en columnas de líneas de NC.
/// </summary>
public static class CreditNoteUiSettings
{
    public const bool UseColumnDetailRowLayout = true;
}
