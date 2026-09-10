using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectProductInMove : PopupSelectBase<account_move_line_view>
    {
        const int MinSearchLength = 2;

        const string LegendDefault =
            "Escriba al menos 2 caracteres del nombre o código del producto en el campo de búsqueda y pulse Buscar.";

        const string EmptyViewInitial =
            "No hay productos listados todavía. Use el campo de búsqueda superior e ingrese al menos 2 caracteres para encontrar productos.";

        const string SearchRequiredTitle = "Búsqueda requerida";
        const string SearchRequiredMessage =
            "Debe ingresar al menos 2 caracteres en el campo de búsqueda para continuar.";

        public res_company Company { get; set; }
        public res_partner ResPartner { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<account_move_line_view> resultItemsSearch { get; set; }

        public PopupSelectProductInMove(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants, true)
        {
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<account_move_line_view>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            _collectionViewSearch.MinimumHeightRequest = UseCompactPopupTopLayout() ? 220 : 400;
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < MinSearchLength)
                return 0;

            await SetWorkingStatus();
            var dbItemsDb = new AccountMoveLineDb(Constants.Session.odooConnection.DbNameSqlite);
            resultItemsSearch = new ObservableCollection<account_move_line_view>(
                await dbItemsDb.GetByProductName(TextForSearch, ResPartner));

            _collectionViewSearch.ItemsSource = resultItemsSearch;
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Productos");
            SetSubtitle(Company.name);
            SetGridTitles("Productos");
            SetGridLegend(LegendDefault);
            SearchEntry.Placeholder = "Nombre o código del producto...";
            ContentCustomToolBox = WrapSearchButton(() => _ = TrySearchAsync());
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
                var row = new AccountMoveLineRow();
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

