using CobranzasDMSA.Models;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Views;

namespace CommunityToolkit.Maui.Sample;

public partial class SimplePopupAutoclose : Popup
{    
	public SimplePopupAutoclose(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();
		DesiredSize = popupSizeConstants.Small;
	}

	public void SetTitle(string newTitle)
	{
		PopupTitle.Text = newTitle;
	}
}