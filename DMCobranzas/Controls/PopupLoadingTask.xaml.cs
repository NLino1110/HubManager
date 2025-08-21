//using DMCobranzas.Models;
using CommunityToolkit.Maui.Sample.Models;
using CommunityToolkit.Maui.Views;

namespace CommunityToolkit.Maui.Sample;

public partial class PopupLoadingTask : Popup
{    
	public PopupLoadingTask(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();
		DesiredSize = popupSizeConstants.SmallWide;		
	}

	public void SetTitle(string newTitle)
	{
		PopupTitle.Text = newTitle;
	}

    public void SetNotify(string newNotify)
    {
        lblNotify.Text = newNotify;
    }
}