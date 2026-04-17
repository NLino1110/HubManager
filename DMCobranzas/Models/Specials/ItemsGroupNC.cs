using DMSA.Models.Odoo.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Models.Specials
{
    public class ItemsGroupNC : List<credit_note_request>
    {
        public string Title { get; private set; }
        public string Header { get; private set; }
        public string GroupData { get; private set; }
        public ItemsGroupNC(string title, string header, string groupdata, List<credit_note_request> items) : base(items)
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
