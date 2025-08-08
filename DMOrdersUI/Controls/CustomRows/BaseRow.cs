using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Diagnostics;
using System.Windows.Input;

namespace DMOrdersUI.Controls.CustomRows
{
    public class BaseRow<T> : ContentView where T : class
    {
        // Bindable Properties
        public static readonly BindableProperty ItemProperty =
            BindableProperty.Create(nameof(Item), typeof(T), typeof(BaseRow<T>), propertyChanged: OnItemChanged);

        public T Item
        {
            get => (T)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(T), typeof(BaseRow<T>), propertyChanged: OnSelectedItemChanged);

        public T SelectedItem
        {
            get => (T)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty IsSelectedProperty =
            BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(BaseRow<T>), false, propertyChanged: OnIsSelectedChanged);

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly BindableProperty LongPressCommandProperty =
            BindableProperty.Create(nameof(LongPressCommand), typeof(ICommand), typeof(BaseRow<T>));

        public ICommand LongPressCommand
        {
            get => (ICommand)GetValue(LongPressCommandProperty);
            set => SetValue(LongPressCommandProperty, value);
        }

        public static readonly BindableProperty TapCommandProperty =
            BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(BaseRow<T>));

        public ICommand TapCommand
        {
            get => (ICommand)GetValue(TapCommandProperty);
            set => SetValue(TapCommandProperty, value);
        }

        // Layout elements
        protected Grid LeftGrid { get; private set; }
        protected Grid ToolGrid { get; private set; }
        private Border _mainBorder;

        public BaseRow()
        {
            BuildLayout();

            SetupGestures();

            // Default LongPressCommand example (can be overwritten)
            LongPressCommand = new Command(() =>
            {
                Debug.WriteLine("Long press detected");
            });
        }

        private void BuildLayout()
        {
            _mainBorder = new Border
            {
                Stroke = Colors.LightGray,
                StrokeThickness = 0.5,
                Padding = new Thickness(2),
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent,
                MinimumHeightRequest = 30,
            };

            var rootGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            LeftGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                ColumnSpacing = 0,
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = new GridLength(1) }, // Separator
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            ToolGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            rootGrid.Children.Add(LeftGrid);
            Grid.SetRow(LeftGrid, 0);
            Grid.SetColumn(LeftGrid, 0);

            rootGrid.Children.Add(ToolGrid);
            Grid.SetRow(ToolGrid, 0);
            Grid.SetColumn(ToolGrid, 1);

            _mainBorder.Content = rootGrid;
            Content = _mainBorder;

            // Build content in grids (can be overridden)
            BuildLeftGridContent(LeftGrid);
            BuildToolGridContent(ToolGrid);
        }

        public View CreateCell(
                View content,
                Thickness? padding = null,
                Color? strokeColor = null,
                float strokeThickness = (float) 0.4,
                float cornerRadius = 4,
                Color? backgroundColor = null)
        {
            return new Border
            {
                Stroke = strokeColor ?? Colors.LightGray,
                StrokeThickness = strokeThickness,
                BackgroundColor = backgroundColor ?? Colors.Transparent,
                Padding = padding ?? new Thickness(0),
                Margin = new Thickness(0, 0),
                //StrokeShape = new RoundRectangle
                //{
                //    CornerRadius = new CornerRadius(cornerRadius)
                //},
                Content = content
            };
        }

        public void AddCell(View cell, string region = "left", int row = 0, int column = 0)
        {
            var targetGrid = region.ToLowerInvariant() switch
            {
                "left" => LeftGrid,
                "tool" => ToolGrid,
                _ => throw new ArgumentException($"Región desconocida: {region}. Usa 'left' o 'tool'.")
            };

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, column);
            targetGrid.Children.Add(cell);
        }

        protected virtual void BuildLeftGridContent(Grid leftGrid)
        {
            // By default do nothing; subclasses can override to add content
        }

        protected virtual void BuildToolGridContent(Grid toolGrid)
        {
            // By default do nothing; subclasses can override to add content
        }

        private void SetupGestures()
        {
            var longPress = new TouchBehavior
            {
                LongPressCommand = new Command(() =>
                {
                    if (LongPressCommand?.CanExecute(null) == true)
                        LongPressCommand.Execute(null);
                }),
                LongPressCommandParameter = LeftGrid,
                ShouldMakeChildrenInputTransparent = false,
                DisallowTouchThreshold = 10,
            };

            var tapGesture = new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (TapCommand?.CanExecute(Item) == true)
                        TapCommand.Execute(Item);
                })
            };

            //LeftGrid.Behaviors.Add(longPress);
            //LeftGrid.GestureRecognizers.Add(tapGesture);
        }

        private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseRow<T> row)
            {
                row.UpdateSelectionVisual();
            }
        }

        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseRow<T> row)
            {
                row.UpdateSelectionVisual();
            }
        }

        private static void OnIsSelectedChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BaseRow<T> row)
            {
                row.UpdateSelectionVisual();
            }
        }

        protected virtual void UpdateSelectionVisual()
        {
            IsSelected = Item != null && Item.Equals(SelectedItem);

            this.BackgroundColor = IsSelected ? Colors.LightBlue : Colors.Transparent;
        }
    }
}
