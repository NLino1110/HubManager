using DMSA.Sync.Core.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Sys
{
    public class UpdateListHelper
    {
        public static List<(MethodInfo method, UpdateActionAttribute attr)> GetUpdateMethods()
        {
            var type = typeof(ServerPuller);

            return type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Select(m => new
                {
                    Method = m,
                    Attr = m.GetCustomAttribute<UpdateActionAttribute>()
                })
                .Where(x => x.Attr != null)
                .Select(x => (x.Method, x.Attr))
                .ToList();
        }


        //var tasks = new UpdateTasks();

        //var updates = GetUpdateMethods()
        //    .Select(x => new UpdateItem
        //    {
        //        Name = x.attr.Name,
        //        Description = x.attr.Description,
        //        Method = x.method
        //    })
        //    .ToList();

        //public async Task ExecuteSelected(List<UpdateItem> items)
        //{
        //    var instance = new UpdateTasks();

        //    foreach (var item in items.Where(x => x.IsSelected))
        //    {
        //        var result = item.Method.Invoke(instance, null);

        //        // soportar async automáticamente
        //        if (result is Task task)
        //            await task;
        //    }
        //}
    }
}
