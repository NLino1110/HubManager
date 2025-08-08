using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Controls
{
    public class CustomEvent
    {
        private List<EventHandler> eventHandlers = new List<EventHandler>();

        public void AddHandler(EventHandler handler)
        {
            eventHandlers.Add(handler);
        }

        public void RemoveHandler(EventHandler handler)
        {
            eventHandlers.Remove(handler);
        }

        public void Invoke(object sender, EventArgs args)
        {
            foreach (var handler in eventHandlers)
            {
                handler?.Invoke(sender, args);
            }
        }

        public void ClearHandlers()
        {
            eventHandlers.Clear();
        }
    }
}
