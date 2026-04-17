namespace DMOrders.Pages.Fragments.Orders.modals
{
    public class ProductTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RowTemplate { get; set; }
        public DataTemplate GridTemplate { get; set; }
        public DataTemplate ViewerTemplate { get; set; }

        public int ViewMode { get; set; } = 0;

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (ViewMode == 0)
                return RowTemplate;

            if (ViewMode == 1)
                return GridTemplate;

            if(ViewMode == 2)
                return ViewerTemplate;

            return RowTemplate;
        }
    }
}
