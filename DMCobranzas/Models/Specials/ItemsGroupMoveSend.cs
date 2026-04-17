using DMSA.Models.Odoo.Accounting;

namespace DMCobranzas.Models.Specials
{
    public class ItemsGroupMoveSend : List<CreditNoteRequestGroup>
    {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string GroupData { get; private set; }
        public ItemsGroupMoveSend(string title, string header, string groupdata, List<CreditNoteRequestGroup> items) : base(items)
        {
            Title = title;
            Header = header;
            GroupData = groupdata;
        }

        public bool Cerrado { get; set; }
        public bool showButtonCierre { get; set; } = true;
    }
}
