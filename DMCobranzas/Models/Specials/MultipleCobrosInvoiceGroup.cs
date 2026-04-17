using CommunityToolkit.Mvvm.ComponentModel;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;

namespace DMCobranzas.Models.Specials
{
    public class MultipleCobrosInvoiceGroup : List<MultipleCobrosInvoice>
    {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string GroupData { get; private set; }                
        private bool _isVisible { get; set; }        
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    //OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public MultipleCobrosInvoiceGroup(string title, string header, string groupdata, List<MultipleCobrosInvoice> items) : base(items)
        {
            Title = title;
            Header = header;
            GroupData = groupdata;
            //Línea innecesaria
            //AddRange(items);
        }

        public bool Cerrado { get; set; }
        public bool showButtonCierre { get; set; } = true;
    }
    
    
}
