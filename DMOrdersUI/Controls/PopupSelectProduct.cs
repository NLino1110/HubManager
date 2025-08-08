using CommunityToolkit.Maui.Views;
using DMOrdersUI.Pages.Fragments.Orders.modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Controls
{
    public class PopupSelectProduct : Popup
    {
        public PopupSelectProduct()
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);            
            Size = popupSizeConstants.Large;

            //var contentView = new CatalogViewer(this);
            //Content = contentView;
        }

        private void OnBtnClose_Clicked(object sender, EventArgs e)
        {            
            Close(null);
        }
    }
}
