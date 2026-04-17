using System;
using System.Reflection;
using CommunityToolkit.Maui.Views;

namespace BeebTech.Maui.Controls.Extensions
{
    internal static class PopupExtensions
    {
        // Provide a compatibility Close extension that calls underlying Close/Dismiss via reflection
        public static void Close(this Popup popup)
        {
            if (popup == null) return;
            var type = popup.GetType();
            // Try common method names
            var method = type.GetMethod("Close", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(popup, null);
                return;
            }
            method = type.GetMethod("Dismiss", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
            {
                method.Invoke(popup, null);
                return;
            }
            // No known method, try to raise Closed event via reflection (best effort)
            var onClosed = type.GetMethod("OnClosed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (onClosed != null)
            {
                try { onClosed.Invoke(popup, null); } catch { }
            }
        }
    }
}
