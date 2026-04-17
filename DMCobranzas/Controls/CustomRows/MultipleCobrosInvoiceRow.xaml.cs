using DMSA.Models.Odoo.DebitCollection;
using Microsoft.Maui;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DMCobranzas.Controls.CustomRows;

public partial class MultipleCobrosInvoiceRow : SwipeView
{
    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(
            nameof(Item),
            typeof(MultipleCobrosInvoice),
            typeof(MultipleCobrosInvoiceRow),
            null,
            propertyChanged: OnItemChanged);

    public MultipleCobrosInvoice Item
    {
        get => (MultipleCobrosInvoice)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }


    public MultipleCobrosInvoiceRow()
	{
		InitializeComponent();
        //BindingContext = this;
        //SwipeStarted += SwipeView_SwipeStarted;
        //SwipeChanging += SwipeView_SwipeChanging;
        //SwipeEnded += SwipeView_SwipeEnded;
    }

    static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultipleCobrosInvoiceRow row)
        {
            row.BindingContext = newValue;
        }
    }

    //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //    base.OnPropertyChanged(propertyName);

    //    //if (propertyName == nameof(DataItem) && DataItem != null)
    //    //{
    //    //    BindingContext = DataItem;
    //    //    Debug.WriteLine(DataItem.recipe_name);
    //    //}
    //}

    private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e) { }
    private void SwipeView_SwipeChanging(object sender, SwipeChangingEventArgs e) { }
    private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e) { }
}