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
            "Escriba al menos 2 caracteres del número o referencia del documento y pulse Enter en el campo, " +
            "o use Últimas 20 / Detalle general.";

        const string LegendLast20 =
            "20 documentos con saldo pendiente y valores a favor más antiguos (facturas, notas de credito y notas de débito), " +
            "ordenados por fecha de factura (antigua → reciente).";

        const string LegendGeneral =
            "Todos los documentos con saldo pendiente y valores a favor (facturas, notas de credito y notas de débito), " +
            "ordenados por fecha de factura (antigua → reciente).";

        const string EmptyViewInitial =
            "No hay documentos listados todavía. Escriba al menos 2 caracteres y pulse Enter en el campo, " +
            "o seleccione Últimas 20 / Detalle general (FACT, NDBI y NCRE).";

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

        /// <summary>
        /// Solo SQLite local: sin login ni llamadas a Odoo (Ver saldos es consulta offline).
        /// </summary>
        async Task EnrichForBalanceViewAsync(List<account_move> items)
        {
            if (items == null || items.Count == 0)
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

        static void PrepareItemsForDisplay(List<account_move> items)
        {
            foreach (var item in items)
            {
                if (AccountMoveDocumentDisplay.IsCreditLikeMoveType(item.move_type))
                    continue;

                if (item.amount_residual_virtual == 0 && item.amount_residual != 0)
                    item.amount_residual_virtual = item.amount_residual;
            }
        }

        async Task LoadInvoicesAsync(string legend, Func<AccountMoveDb, Task<List<account_move>>> fetchItemsAsync)
        {
            if (Company == null || partner == null)
            {
                await ShowPopupAlertAsync(
                    "Datos incompletos",
                    "No se ha definido la compañía o el cliente para consultar saldos.");
                return;
            }

            var working = false;
            try
            {
                var dbName = ResolveDbName();
                await SetWorkingStatus();
                working = true;
                SetGridLegend(legend);

                var database = new AccountMoveDb(dbName);
                var result = await fetchItemsAsync(database);

                resultItemsSearch = new ObservableCollection<account_move>(result ?? new List<account_move>());
                _collectionViewSearch.ItemsSource = resultItemsSearch;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PopupSelectInvoice LoadInvoicesAsync: {ex}");
                await ShowPopupAlertAsync(
                    "Error al cargar",
                    "No se pudieron obtener los documentos. Se mostrarán los datos locales disponibles si existen.");
            }
            finally
            {
                if (working)
                    await SetDoneStatus();
            }
        }

        Task LoadDataLast20() =>
            LoadInvoicesAsync(
                LegendLast20,
                async database =>
                {
                    var result = await database.GetItemsWithBalanceByPartnerForBalanceViewAsync(
                        Company.id,
                        partner.id,
                        LastInvoicesLimit);

                    await EnrichForBalanceViewAsync(result);
                    return result;
                });

        Task LoadDataGeneralWithBalance() =>
            LoadInvoicesAsync(
                LegendGeneral,
                async database =>
                {
                    var result = await database.GetItemsWithBalanceByPartnerForBalanceViewAsync(
                        Company.id,
                        partner.id);
                    await EnrichForBalanceViewAsync(result);
                    return result;
                });

        Task LoadDataForView() => LoadDataGeneralWithBalance();

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle(Company?.name ?? "Documentos");
            SetSubtitle(partner?.name ?? string.Empty);
            SetGridTitles("Documentos");
            SetGridLegend(LegendDefault);
            SearchEntry.Placeholder = "Número o referencia del documento...";

            if (LoadAuto)
                await LoadDataForView();

            var btnLoadLastInvoices = new Button
            {
                Text = "Últimas 20",
                BackgroundColor = Colors.SeaGreen,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(5, 5, 2, 5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf0ae"
                }
            };
            btnLoadLastInvoices.Clicked += OnBtnLoadLast_Clicked;

            var btnLoadGeneral = new Button
            {
                Text = "Detalle general",
                BackgroundColor = Colors.SteelBlue,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(2, 5, 5, 5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf03a"
                }
            };
            btnLoadGeneral.Clicked += OnBtnLoadGeneral_Clicked;

            var stackLayoutToolBox = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(2, 0, 0, 0),
                BackgroundColor = Colors.GhostWhite
            };

            stackLayoutToolBox.Children.Add(btnLoadLastInvoices);
            stackLayoutToolBox.Children.Add(btnLoadGeneral);

            ContentToolBox1 = new ContentView
            {
                Content = stackLayoutToolBox
            };
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await TrySearchAsync();
        }

        async Task TrySearchAsync()
        {
            SyncTextForSearchFromEntry();

            if (TextForSearch.Length < MinSearchLength)
            {
                await ShowPopupAlertAsync(SearchRequiredTitle, SearchRequiredMessage);
                return;
            }

            await LoadData();
        }

        private async void OnBtnLoadLast_Clicked(object sender, EventArgs e)
        {
            await LoadDataLast20();
        }

        private async void OnBtnLoadGeneral_Clicked(object sender, EventArgs e)
        {
            await LoadDataGeneralWithBalance();
        }

        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {
                BackgroundColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = EmptyViewInitial,
            };

            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new AccountMoveRow();
                row.ActionCommand = CommandSelectListItem;

                row.BindingContextChanged += (s, e) =>
                {
                    if (row.BindingContext != null)
                    {
                        var selectedItemBinding = new Binding
                        {
                            Path = "SelectedItem",
                            Source = collectionView,
                            Mode = BindingMode.TwoWay
                        };
                    }
                };

                return row;
            });

            return collectionView;
        }
    }
}
