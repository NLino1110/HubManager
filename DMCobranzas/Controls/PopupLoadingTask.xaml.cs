using CommunityToolkit.Maui.Views;
using DMSA.Sync.Core.Controls.Popups;

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