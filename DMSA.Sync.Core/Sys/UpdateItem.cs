using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Sys
{
    public class UpdateItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public MethodInfo Method { get; set; }
        public bool IsSelected { get; set; }
    }
}
