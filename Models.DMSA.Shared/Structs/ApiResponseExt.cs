using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Structs
{
    public class ApiResponseExt<T> : ApiResponseGlobal
    {
        public List<T> Items { get; set; }
    }
}
