using DMOrders.Controls.CustomRows.Lite;
using DMSA.Models.Odoo.Native;
using DMSA.Sync.Core.Database.Sqlite;
using System.Collections.ObjectModel;

namespace DMSA.Sync.Core.Controls.Popups
{
    public class PopupSelectProductCategory : PopupSelectBase<product_categoria>
    {
        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<product_categoria> resultItemsSearch { get; set; }
       
        public PopupSelectProductCategory(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<product_categoria>();
            Padding = new Thickness(0);
            Margin = new Thickness(0);
            _collectionViewSearch.MinimumHeightRequest = 400;
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }
            
            await SetWorkingStatus();
            var dbItemsDb = new ProductCategoriaDb(Constants.Session.odooConnection.DbNameSqlite);
            resultItemsSearch = new ObservableCollection<product_categoria>(await dbItemsDb.GetItemsAsync(TextForSearch));
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Categorias");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Categoria");            
        }

        async void _searchBar_BeginSearch(object sender, EventArgs e)
        {
            await LoadData();
        }
        
        public override CollectionView builCollectionViewCustom()
        {
            var collectionView = new CollectionView
            {                
                HorizontalOptions = LayoutOptions.Fill,
                SelectionMode = SelectionMode.Single,
                EmptyView = "Datos no encontrados...",
                ItemsLayout = new GridItemsLayout(4, ItemsLayoutOrientation.Vertical)
            };
                        
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var row = new ProductCategoryRow();
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
                        //row.SetBinding(ProductCategoryRow.SelectedItemProperty, selectedItemBinding);
                    }
                };

                return row;
            });

            return collectionView;

        }
    }
}
