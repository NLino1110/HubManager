using DMSA.Models.Odoo.Accounting;

namespace DMCobranzas.Controls.CustomRows;

public partial class CreditNoteRequestGroupRow : SwipeView
{
    public static readonly BindableProperty DataItemProperty =
        BindableProperty.Create(
            nameof(DataItem),
            typeof(CreditNoteRequestGroup),
            typeof(CreditNoteRequestGroupRow),
            null,
            propertyChanged: OnDataItemChanged);

    public CreditNoteRequestGroup DataItem
    {
        get => (CreditNoteRequestGroup)GetValue(DataItemProperty);
        set => SetValue(DataItemProperty, value);
    }

    public CreditNoteRequestGroupRow()
	{
		InitializeComponent();        
    }

    static void OnDataItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CreditNoteRequestGroupRow)bindable;
        control.BindingContext = newValue;
    }
}