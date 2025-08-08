using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ResourceBuilder.Services.Sync
{
    public static class TaskStatus
    {
        public static bool SyncInWorking { get; set; }
        public static bool SyncOutWorking { get; set; }
    }
}
