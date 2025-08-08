using CommunityToolkit.Maui.Views;

namespace DMOrdersUI.Controls;

public partial class PopupLoadingTask : Popup
{    
	public PopupLoadingTask(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();
		Size = popupSizeConstants.SmallWide;		
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