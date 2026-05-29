using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract
{
    public class RootSettings
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? value { get; set; }
        public bool ? visible { get; set; }
    }
}
