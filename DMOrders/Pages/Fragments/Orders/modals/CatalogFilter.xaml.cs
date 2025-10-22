using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace DMOrders.Pages.Fragments.Orders.modals;

public partial class CatalogFilter : ContentView
{
    private int span_columns = 4;
    public CatalogFilter()
	{
		InitializeComponent();
	}

    private async void SelectionView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Debug.WriteLine($"SelectionView Property Changed: {e.PropertyName}");
        if (e.PropertyName.Equals("SelectedIndex"))
        {
            await SetViewMode(-1);
        }
    }

    private async Task SetViewMode(int ViewMode)
    {
        var vm = BindingContext as CatalogViewerModel;
        var selector = Resources["ProductTemplateSelector"] as ProductTemplateSelector;
        if (selector != null && vm != null)
        {
            selector.ViewMode = vm.ViewModesListSelectedIndex;

            if (vm.ViewModesListSelectedIndex == 0)
            {
                vm.PageSize = 40;
                span_columns = 1;
                SelectButtonUnique.IsVisible = false;
                //GridTitleSearch.IsVisible = true;
            }
            else if (vm.ViewModesListSelectedIndex == 1)
            {
                vm.PageSize = 40;
                span_columns = 4;
                SelectButtonUnique.IsVisible = false;
                //GridTitleSearch.IsVisible = false;
            }
            else if (vm.ViewModesListSelectedIndex == 2)
            {
                vm.PageSize = 1;
                span_columns = 1;
                SelectButtonUnique.IsVisible = true;
                //GridTitleSearch.IsVisible = false;
            }

            //(MyCollectionView.ItemsLayout as GridItemsLayout).Span = span_columns;

            Debug.WriteLine(vm.PageSize);
            Debug.WriteLine(span_columns);
            //await Task.Delay(500);
            await vm.LoadData();
        }
    }

    private void SelectSingleItem(object sender, EventArgs e)
    {
        var objItem = ((CatalogViewerModel)this.BindingContext).SelectedItem;
        if (objItem != null)
        {
            //ItemPickedCommand
            //_parentPopup.Close(objItem);
            this.IsVisible = false;

            //if (ItemPickedCommand?.CanExecute(objItem) != null)
            //    ItemPickedCommand.Execute(objItem);

        }
        else
        {
            Debug.WriteLine("Error de objeto");
        }
    }
}