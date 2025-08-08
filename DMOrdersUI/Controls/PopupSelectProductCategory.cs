
using DMOrdersUI.Controls.CustomRows;
using DMOrdersUI.Services.Database.Sqlite;
using DMSA.Models.Odoo.Native;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DMOrdersUI.Controls
{
    public class PopupSelectProductCategory : PopupSelectBase
    {
        public res_company Company { get; set; }
        public int DetailMode { get; set; } = 0;
        ObservableCollection<product_category> resultItemsSearch { get; set; }
       
        public PopupSelectProductCategory(PopupSizeConstants popupSizeConstants) : base(popupSizeConstants,true)
        {            
            DataField = "id, name";
            _LaunchSearchEvent += _searchBar_BeginSearch;
            _OnAppearing += _onAppearingCustom;
            resultItemsSearch = new ObservableCollection<product_category>();            
        }

        async Task<int> LoadData()
        {
            if (TextForSearch.Length < 2)
            {
                return 0;
            }

            await SetWorkingStatus();
            ProductCategoryDb dbItemsDb = new ProductCategoryDb();
            resultItemsSearch = new ObservableCollection<product_category>((await dbItemsDb.GetItemsAsync()).Where(data=>data.name.Contains(TextForSearch.ToUpper())));            
            _collectionViewSearch.ItemsSource = resultItemsSearch;            
            await SetDoneStatus();
            return 1;
        }

        async void _onAppearingCustom(object sender, EventArgs e)
        {
            SetTitle("Categorias");
            SetSubtitle(Company.name);
            SetGridTitles("Datos de Marca");            
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
                row.SetBinding(ProductCategoryRow.ItemProperty, new Binding("."));

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
                        row.SetBinding(ProductCategoryRow.SelectedItemProperty, selectedItemBinding);
                    }
                };

                return row;
            });

            return collectionView;

        }
    }
}
