using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Sys
{
    [AttributeUsage(AttributeTargets.Method)]
    public class UpdateActionAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public int Order { get; }
        public string Category { get; }

        public UpdateActionAttribute(string name, string description = "")
        {
            Name = name;
            Description = description;
        }
    }
}
