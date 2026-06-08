using Android.Views;
using Microsoft.Maui.Handlers;
using DMOrders.Controls;
using System.Collections.Generic;
using System.Linq;

namespace DMOrders.Platforms.Android
{
    public class NativeSpinnerViewHandler : ViewHandler<NativeSpinnerView, global::Android.Widget.Spinner>
    {
        global::Android.Widget.ArrayAdapter<string> _adapter;
        List<object> _items = new();

        public NativeSpinnerViewHandler()
            : base(ViewHandler.ViewMapper)
        {
        }

        protected override global::Android.Widget.Spinner CreatePlatformView()
        {
            var spinner = new global::Android.Widget.Spinner(
                Context,
                global::Android.Widget.SpinnerMode.Dropdown
            );

            spinner.LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent);

            spinner.ItemSelected += OnItemSelected;

            return spinner;
        }

        protected override void ConnectHandler(global::Android.Widget.Spinner platformView)
        {
            base.ConnectHandler(platformView);
            SetItems();
        }

        public static void MapItemsSource(NativeSpinnerViewHandler handler, NativeSpinnerView view)
        {
            handler.SetItems();
        }

        public static void MapSelectedItem(NativeSpinnerViewHandler handler, NativeSpinnerView view)
        {
            handler.UpdateSelectedItem();
        }

        void SetItems()
        {
            if (VirtualView?.ItemsSource == null || PlatformView == null)
                return;

            _items = VirtualView.ItemsSource.Cast<object>().ToList();

            var stringItems = _items.Select(x => x?.ToString() ?? "").ToList();

            _adapter = new global::Android.Widget.ArrayAdapter<string>(
                Context,
                global::Android.Resource.Layout.SimpleSpinnerDropDownItem,
                stringItems
            );

            PlatformView.Adapter = _adapter;

            UpdateSelectedItem();
        }

        void UpdateSelectedItem()
        {
            if (VirtualView?.SelectedItem == null || _items == null)
                return;

            var index = _items.IndexOf(VirtualView.SelectedItem);
            if (index >= 0)
                PlatformView.SetSelection(index);
        }

        void OnItemSelected(object sender, global::Android.Widget.AdapterView.ItemSelectedEventArgs e)
        {
            if (_items == null || e.Position < 0 || e.Position >= _items.Count)
                return;

            VirtualView.SelectedItem = _items[e.Position];
        }
    }
}