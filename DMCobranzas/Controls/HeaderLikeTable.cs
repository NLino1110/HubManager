using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Controls
{
    class HeaderLikeTable : ContentView
    {
        //public static readonly BindableProperty ColumnsProperty =
        //BindableProperty.Create(nameof(Columns), 
        //    typeof(ObservableCollection<string>),
        //    typeof(HeaderLikeTable));

        //public ObservableCollection<string> Columns
        //{
        //    get => (ObservableCollection<string>) GetValue(ColumnsProperty);
        //    set => SetValue(ColumnsProperty, value);
        //}

        public static readonly BindableProperty ColumnsProperty =
        BindableProperty.Create(nameof(Columns),
            typeof(string),
            typeof(HeaderLikeTable));

        public string Columns
        {
            get => (string)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);

        }

        //protected override void OnBindingContextChanged()
        //{
        //    base.OnBindingContextChanged();
        //    Debug.WriteLine("Contexto00000000000000!!");
        //}

        //public List<string>Titles
        //{
        //    get => (List<string>)GetValue(_Titles);
        //    set => SetValue(_Titles, value);
        //}

        private Grid grid;

        public HeaderLikeTable()
        {
            Frame ContentFrame = new Frame()
            {
                BorderColor = Colors.LightGray,
                Padding = new Thickness(10),
                Margin = new Thickness(1),
                BackgroundColor = Colors.Gray,
                CornerRadius = 0,
            };

            grid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            ContentFrame.Content = grid;
            Content = ContentFrame;

            var timer_eventController = Dispatcher.CreateTimer();
            timer_eventController.IsRepeating = false;
            timer_eventController.Interval = TimeSpan.FromMilliseconds(500);
            timer_eventController.Tick += async (s, e) =>
            {
                if (Parent != null)
                {
                    if(Columns != null)
                    {
                        Debug.WriteLine(Columns);

                        SetColumns();

                    }
                }
            };
            timer_eventController.Start();
        }

        private void SetColumns()
        {
            var labels = Columns.Split(",");

            for (int i = 0; i < labels.Length; i++)
            {
                if (i == 3 || i == 6)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                else
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                }
            }

            //var labels = new string[] { "Fecha", "Diario", "Método de Pago", "", "Importe", "Referencia" };

            for (int i = 0; i < labels.Length; i++)
            {
                var label = new Label
                {
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center,
                    Text = labels[i],
                    TextColor = Colors.WhiteSmoke,
                    FontAttributes = FontAttributes.Bold
                };

                Grid.SetRow(label, 0);
                Grid.SetColumn(label, i);

                grid.Children.Add(label);
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            Debug.WriteLine(propertyName);

            if (propertyName == "Columns")
            {
                grid.Children.Clear();
                SetColumns();
            }
        }
    }
}
