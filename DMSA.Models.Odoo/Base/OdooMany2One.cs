using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Base
{
    public class OdooMany2One
    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        public static implicit operator int?(OdooMany2One m) => m?.Id;
    }
}
