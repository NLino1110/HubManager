using DMOrders.Controls.CustomRows.Lite;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectPartnerCreditData : PopupSelectBase<res_partner>
    {
        const int MinSearchLength = 2;

        const string LegendDefault =
            "Escriba al menos 2 caracteres del nombre o código del cliente en el campo de búsqueda y pulse Buscar.";

        const string EmptyViewInitial =
            "No hay clientes listados todavía. Use el campo de búsqueda superior e ingrese al menos 2 caracteres para encontrar clientes.";

        const string SearchRequiredTitle = "Búsqueda requerida";
        const string SearchRequiredMessage =
            "Debe ingresar al menos 2 caracteres en el campo de búsqueda para continuar.";

        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<res_partner> resultItemsSearch { get; set; }
        
        public PopupSelectPartnerCreditData(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants, true)
        {            
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<res_partner>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            MinimumHeightRequest = 400;            
        }

        async Task<int> LoadData()
        {
            await SetWorkingStatus();
            ResPartnerDb partnerBankDb = new ResPartnerDb(Constants.Session.odooConnection.DbNameSqlite);
            resultItemsSearch = new ObservableCollection<res_partner>(
                await partnerBankDb.GetItemsBySearchAsync(Company.id, TextForSearch.ToUpper(), 50));
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();

            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Clientes");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Cliente");
            SetGridLegend(LegendDefault);
            SearchEntry.Placeholder = "Nombre o código del cliente...";

            var btnSearch = new Button
            {
                Text = "Buscar",
                BackgroundColor = Colors.SeaGreen,
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(5, 5, 5, 5),
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 18,
                    Glyph = "\uf002"
                }
            };
            btnSearch.Clicked += (_, _) => RunSearch();

            ContentCustomToolBox = new ContentView
            {
                Content = btnSearch
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

        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {
                BackgroundColor = Colors.WhiteSmoke,
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = EmptyViewInitial,
                ItemsLayout = new GridItemsLayout(1, ItemsLayoutOrientation.Vertical)
            };

            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new ResPartnerCreditDataRow();
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
