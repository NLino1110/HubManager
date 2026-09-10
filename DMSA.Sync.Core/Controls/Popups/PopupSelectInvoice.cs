using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectInvoice : PopupSelectBase<account_move>
    {
        const int LastInvoicesLimit = 20;
        const int MinSearchLength = 2;

        const string LegendDefault =
            "Escriba al menos 2 caracteres del número o referencia del documento y pulse Buscar, " +
            "o use Últimas 20 / Detalle general.";

        const string LegendLast20 =
            "20 documentos con saldo pendiente más antiguos (facturas y notas de débito), " +
            "ordenados por fecha de factura (antigua → reciente).";

        const string LegendGeneral =
            "Todos los documentos con saldo pendiente (facturas y notas de débito), " +
            "ordenados por fecha de factura (antigua → reciente).";

        const string EmptyViewInitial =
            "No hay documentos listados todavía. Escriba al menos 2 caracteres y pulse Buscar, " +
            "o seleccione Últimas 20 / Detalle general.";

        const string SearchRequiredTitle = "Búsqueda requerida";
        const string SearchRequiredMessage =
            "Debe ingresar al menos 2 caracteres en el campo de búsqueda para continuar.";

        public res_company Company { get; set; }
        public res_partner partner { get; set; }
        ObservableCollection<account_move> resultItemsSearch { get; set; }
        public bool LoadAuto { get; set; } = false;

        public PopupSelectInvoice(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants, true)
        {
            DataField = "id, docnum_mask, name, invoice_date, payment_state, amount_residual, amount_total";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            _collectionViewSearch.MinimumHeightRequest = UseCompactPopupTopLayout() ? 220 : 400;
        }

        static string ResolveDbName()
        {
            var session = Constants.Session;
            if (session?.odooConnection?.DbNameSqlite == null)
                throw new InvalidOperationException("No hay sesión activa o base de datos local configurada.");

            return session.odooConnection.DbNameSqlite;
        }

        async Task LoadData()
        {
            if ((TextForSearch ?? string.Empty).Trim().Length < MinSearchLength)
                return;

            await LoadInvoicesAsync(
                LegendDefault,
                async database =>
                {
                    var result = await database.GetItemsAsync(Company.id, partner.id, TextForSearch, 25);
                    await EnrichForBalanceViewAsync(result);
                    return result;
                });
        }

        async Task EnrichForBalanceViewAsync(List<account_move> items)
        {
            if (items == null || items.count == 0)
                return;

            PrepareItemsForDisplay(items);

            var dbName = ResolveDbName();
            var accountMoveDb = new AccountMoveDb(dbName);
            await accountMoveDb.EnrichPostdatedAmountsAsync(items);

            ResPartnerDb resPartnerDb = new ResPartnerDb(dbName);
            var res_Partner = await resPartnerDb.GetItemsAsync(x => x.is_salesman);
            var partnerMap = res_Partner
                .GroupBy(x => x.id)
                .ToDictionary(g => g.Key, g => g.First().name ?? string.Empty);

            foreach (var accountMoveItem in items)
            {
                accountMoveItem.l10n_ec_authorization_number = partnerMap.TryGetValue(
                    accountMoveItem._partner_sale_id, out var name)
                    ? name
                    : string.Empty;
            }
        }

