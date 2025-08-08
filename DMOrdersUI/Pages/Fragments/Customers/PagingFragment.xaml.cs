namespace DMOrdersUI.Pages.Fragments.Customers;

public partial class PagingFragment : ContentView
{
	public PagingFragment()
	{
		InitializeComponent();
	}

    public void DisableNewButton()
    {
        btnNew.IsEnabled = false; // Desactiva el botón btnNew
    }

    private void TxtUser_TextChanged(object sender, TextChangedEventArgs e)
    {        
        //if (string.IsNullOrWhiteSpace(txtUser.Text))
        //{
        //    lblPlaceholder.TranslationY = 10;
        //    lblPlaceholder.FontSize = 14;
        //    lblPlaceholder.TextColor = Colors.Gray;
        //}
        //else
        //{
        //    lblPlaceholder.TranslationY = -8;
        //    lblPlaceholder.FontSize = 12;
        //    lblPlaceholder.TextColor = Colors.Blue;
        //}
    }

    //private void txtUser_Focused(object sender, FocusEventArgs e)
    //{        
    //    lblPlaceholder.TranslationY = 5;
    //    lblPlaceholder.FontSize = 12;
    //    lblPlaceholder.TextColor = Colors.Blue;        
    //}

    //private void txtUser_Unfocused(object sender, FocusEventArgs e)
    //{
    //    if (string.IsNullOrWhiteSpace(txtUser.Text))
    //    {
    //        lblPlaceholder.TranslationY = 16;
    //        lblPlaceholder.FontSize = 14;
    //        lblPlaceholder.TextColor = Colors.Gray;
    //    }
    //    else
    //    {
    //        lblPlaceholder.TranslationY = 5;
    //        lblPlaceholder.FontSize = 12;
    //        lblPlaceholder.TextColor = Colors.Blue;
    //    }
    //}
}