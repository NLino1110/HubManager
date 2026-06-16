using System;
using System.Collections.Generic;
using System.Text;

namespace DMSA.Models.Odoo.Abstract.Server
{

    public class DatabaseStruct
    {
        public string Host { get; set; }
        public string Name { get; set; }
        public string OriginalDBName { get; set; }
        public int Size { get; set; }
        public string Path { get; set; }
        public bool UseAsBase { get; set; }
    }
}
