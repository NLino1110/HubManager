using CommunityToolkit.Maui.Views;
using DMOrders.Pages.Fragments.Orders.modals;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Controls
{
    public class PopupSelectProduct : Popup<product_template>
    {
        public PopupSelectProduct()
        {
            var popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);            
            DesiredSize = popupSizeConstants.Large;

            //var contentView = new CatalogViewer(this);
            //Content = contentView;
        }

        private async void OnBtnClose_Clicked(object sender, EventArgs e)
        {            
            await CloseAsync(default(product_template));
        }
    }
}
