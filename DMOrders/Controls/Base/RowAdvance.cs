using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Controls.Base
{
    public class RowAdvance<T> : Row<T> where T : class
    {
        public static readonly BindableProperty TemplateBuilderProperty =
            BindableProperty.Create(
                nameof(TemplateBuilder),
                typeof(Func<T, View>),
                typeof(RowAdvance<T>),
                propertyChanged: OnTemplateBuilderChanged
            );

        public Func<T, View> TemplateBuilder
        {
            get => (Func<T, View>)GetValue(TemplateBuilderProperty);
            set => SetValue(TemplateBuilderProperty, value);
        }

        private static void OnTemplateBuilderChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is RowAdvance<T> row)
            {
                row.RebuildLayout();
            }
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            leftGrid.Children.Clear();

            if (Item != null && TemplateBuilder != null)
            {
                var customView = TemplateBuilder.Invoke(Item);
                if (customView != null)
                {
                    AddCell(customView, region: "left", row: 0, column: 0);
                }
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            RebuildLayout();
        }

        private void RebuildLayout()
        {
            if (Item != null)
            {
                BuildLeftGridContent(LeftGrid);
            }
        }
    }


}
