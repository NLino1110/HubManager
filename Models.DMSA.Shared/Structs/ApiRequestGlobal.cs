using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Structs
{
    public class ApiRequestGlobal<T>
    {
        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 0;
        public string SortBy { get; set; } = "id";
        public string SortOrder { get; set; } = "asc";
        public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
        //public List<T> Items { get; set; }
    }
}
