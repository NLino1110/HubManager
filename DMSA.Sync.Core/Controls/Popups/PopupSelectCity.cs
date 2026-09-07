using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Controls.CustomRows.Lite;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.Payments;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectCity : PopupSelectBase<res_city>
    {
        const int MinSearchLength = 2;

        const string LegendDefault =
            "Escriba al menos 2 caracteres del nombre de la ciudad en el campo de búsqueda y pulse Buscar.";

        const string EmptyViewInitial =
            "No hay ciudades listadas todavía. Use el campo de búsqueda superior e ingrese al menos 2 caracteres para encontrar ciudades.";

        const string SearchRequiredTitle = "Búsqueda requerida";
        const string SearchRequiredMessage =
            "Debe ingresar al menos 2 caracteres en el campo de búsqueda para continuar.";

        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<res_city> resultItemsSearch { get; set; }
       
        public PopupSelectCity(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<res_city>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            _collectionViewSearch.MinimumHeightRequest = 400;
        }

        async Task<int> LoadData()
        {
            await SetWorkingStatus();
            var dbItemsDb = new ResCityDb(Constants.Session.odooConnection.DbNameSqlite);
            var items = await dbItemsDb.SearchByColumnAsync("name", TextForSearch);

            resultItemsSearch = new ObservableCollection<res_city>(items);
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Ciudades");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Ciudades");
            SetGridLegend(LegendDefault);
            SearchEntry.Placeholder = "Nombre de la ciudad...";

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
                ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };
                        
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new ResCityRow();
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
