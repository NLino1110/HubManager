using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Shared
{
    public interface IBackButtonHandler
    {
        Task<bool> OnBackButtonPressedAsync();
    }
}
