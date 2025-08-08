using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VtexStrucsGen2.Strucs.Vtex
{
    internal class ServicesReturn
    {        
        public int Id { get; set; }
        public int ServiceTypeId { get; set; }
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public bool IsGiftCard { get; set; }
        public bool IsRequired { get; set; }
        public ServiceOption[] Options { get; set; }
        public object[] Attachments { get; set; }       

    }
}
