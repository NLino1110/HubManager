using CommunityToolkit.Maui.Views;
using System.Diagnostics;

namespace DMOrders.Controls;

public partial class PopupLoadingTask : Popup
{    
	public PopupLoadingTask(PopupSizeConstants popupSizeConstants)
	{
		InitializeComponent();
		DesiredSize = popupSizeConstants.Medium;        
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        //base.OnSizeAllocated(width, height);
        base.OnSizeAllocated(500, 500);
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