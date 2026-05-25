using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{
    [Obsolete("No encontrada funcionalidad")]
    public class SaleOrderLineHeader: INotifyPropertyChanged
    {
        public string Number { get; set; }
        public string Product { get; set; }
        public string UOM { get; set; }
        public string QtyReal { get; set; }
        public string QtyDisp { get; set; }
        public string Price { get; set; }
        public string PriceTax { get; set; }
        public string SubTotalNt { get; set; }
        public string DiscountPercent { get; set; }
        public string DiscountValue { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
