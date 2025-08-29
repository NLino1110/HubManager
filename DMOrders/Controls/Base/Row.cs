using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Windows.Input;

namespace DMOrders.Controls.Base
{
    public class Row<T> : ContentView where T : class
    {
        #region Bindable Properties

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem), typeof(T), typeof(Row<T>));

        public T SelectedItem
        {
            get => (T)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty ItemProperty =
            BindableProperty.Create(nameof(Item), typeof(T), typeof(Row<T>));

        public T Item
        {
            get => (T)GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        public static readonly BindableProperty TapCommandProperty =
            BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(Row<T>));

        public ICommand TapCommand
        {
            get => (ICommand)GetValue(TapCommandProperty);
            set => SetValue(TapCommandProperty, value);
        }

        public static readonly BindableProperty LongPressCommandProperty =
            BindableProperty.Create(nameof(LongPressCommand), typeof(ICommand), typeof(Row<T>));

        public ICommand LongPressCommand
        {
            get => (ICommand)GetValue(LongPressCommandProperty);
            set => SetValue(LongPressCommandProperty, value);
        }

        #endregion

        #region Layout

        protected Grid LeftGrid { get; private set; }
        protected Grid ToolGrid { get; private set; }
        private Border _borderFrame;

        #endregion

        public Row()
        {
            BuildLayout();
            //SetupGestures();
        }

        private void BuildLayout()
        {
            // Frame como borde ligero
            _borderFrame = new Border
            {
                Stroke = Colors.LightGray,
                StrokeThickness = 0.5f,                
                Padding = 0,
                Margin = 0,
                BackgroundColor = Colors.Transparent
            };

            // Root Grid
            var rootGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            // Grids internos
            LeftGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            ToolGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }
                }
            };

            rootGrid.Children.Add(LeftGrid);
            Grid.SetRow(LeftGrid, 0);
            Grid.SetColumn(LeftGrid, 0);

            rootGrid.Children.Add(ToolGrid);
            Grid.SetRow(ToolGrid, 0);
            Grid.SetColumn(ToolGrid, 1);

            // Frame como borde envolviendo rootGrid
            _borderFrame.Content = rootGrid;
            Content = _borderFrame;

            BuildLeftGridContent(LeftGrid);
            BuildToolGridContent(ToolGrid);
                        
        }

        protected virtual void BuildLeftGridContent(Grid leftGrid) { }
        protected virtual void BuildToolGridContent(Grid toolGrid) { }

        private void SetupGestures()
        {
            var tapGesture = new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    if (TapCommand?.CanExecute(Item) == true)
                        TapCommand.Execute(Item);
                })
            };
            this.GestureRecognizers.Add(tapGesture);

            var longPressBehavior = new TouchBehavior
            {
                LongPressCommand = new Command(() =>
                {
                    if (LongPressCommand?.CanExecute(Item) == true)
                        LongPressCommand.Execute(Item);
                }),
                LongPressCommandParameter = Item,
                ShouldMakeChildrenInputTransparent = false,
                DisallowTouchThreshold = 10
            };
            this.Behaviors.Add(longPressBehavior);
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

        public View CreateCell(
                View content,
                Thickness? padding = null,
                Color? strokeColor = null,
                float strokeThickness = (float)0.4,
                float cornerRadius = 4,
                Color? backgroundColor = null)
        {
            return content;
        }
    }
}
