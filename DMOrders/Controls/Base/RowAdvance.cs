using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

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
                row.RebuildLayout();
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            leftGrid.Children.Clear();
            if (Item != null && TemplateBuilder != null)
            {
                var customView = TemplateBuilder.Invoke(Item);
                if (customView != null)
                    AddCell(customView, region: "left", row: 0, column: 0);
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            RebuildLayout();
            UpdateSelectionVisual();
        }

        private void RebuildLayout()
        {
            if (Item != null)
                BuildLeftGridContent(LeftGrid);

            // Configurar VisualStates para selección
            VisualStateManager.SetVisualStateGroups(this, new VisualStateGroupList
            {
                new VisualStateGroup
                {
                    Name = "CommonStates",
                    States =
                    {
                        new VisualState
                        {
                            Name = "Normal",
                            Setters = { new Setter { Property = BackgroundColorProperty, Value = Colors.Transparent } }
                        },
                        new VisualState
                        {
                            Name = "Selected",
                            Setters = { new Setter { Property = BackgroundColorProperty, Value = Colors.LightBlue } }
                        }
                    }
                }
            });
        }

        public void UpdateSelectionVisual()
        {
            if (Parent is CollectionView cv)
            {
                var selectedItem = cv.SelectedItem;
                bool isSelected = Item != null && Item.Equals(selectedItem);
                VisualStateManager.GoToState(this, isSelected ? "Selected" : "Normal");
            }
        }
    }
}
