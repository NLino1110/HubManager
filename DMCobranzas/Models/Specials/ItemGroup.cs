using CommunityToolkit.Mvvm.ComponentModel;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.DebitCollection;
using DMSA.Models.Odoo.DMCobranzas;
using DMSA.Models.Odoo.Native;

namespace DMCobranzas.Models.Specials
{
    public class ItemsGroup : List<MultipleCobrosInvoice>
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

        public ItemsGroup(string title, string header, string groupdata, List<MultipleCobrosInvoice> items) : base(items)
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

    public class ItemsGroupNC : List<account_move_send>
    {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string GroupData { get; private set; }
        public ItemsGroupNC(string title, string header, string groupdata, List<account_move_send> items) : base(items)
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

    public class ItemsGroupMoveSend : List<AccountMoveSendHeader>
    {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string GroupData { get; private set; }
        public ItemsGroupMoveSend(string title, string header, string groupdata, List<AccountMoveSendHeader> items) : base(items)
        {
            Title = title;
            Header = header;
            GroupData = groupdata;            
        }

        public bool Cerrado { get; set; }
        public bool showButtonCierre { get; set; } = true;
    }

    //public class ItemsGroupColG : ObservableCollection<ItemsGroupCol>
    //{
    //    protected override void ClearItems()
    //    {
    //        base.ClearItems();
    //        //base.Items.Clear();

    //        //for (int indexer = 0; indexer < Items.Count(); indexer++)
    //        //{
    //        //    base.Items.RemoveAt(indexer);
    //        //}
    //        //base.Items.Add(
    //        //        new ItemsGroupCol("PRUEBA 256", new List<CobReciboCab>()
    //        //        {
    //        //            new CobReciboCab()
    //        //            {
    //        //                NOMBRECLIENTE = "RONALD ZYYY",
    //        //            }
    //        //        })
    //        //    );

    //        //Debug.WriteLine(base.Items.Count());
    //        Debug.WriteLine("Borrados");
    //    }

    //    //protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    //    //{
    //    //    base.OnCollectionChanged(e);
    //    //    Debug.WriteLine("Cambiado");
    //    //}

    //    public ItemsGroupColG(List<ItemsGroupCol> items) : base(items)
    //    {            
    //        //Línea innecesaria
    //        //AddRange(items);
    //    }
    //}

    //public class ItemsGroupCol : List<CobReciboCab>
    //{
    //    public string Header { get; private set; }
    //    public ItemsGroupCol(string header, List<CobReciboCab> items) : base(items)
    //    {
    //        Header = header;
    //        //Línea innecesaria
    //        //AddRange(items);
    //    }
    //}
}
