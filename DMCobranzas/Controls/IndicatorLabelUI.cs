using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.Controls
{
    public class IndicatorLabelUI : ContentView
    {
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
                typeof(string), //Clase de datos auxiliar
                typeof(IndicatorLabelUI) //Clase contenedora
                );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        //public static readonly BindableProperty TextSwitchProperty =
        //    BindableProperty.Create(nameof(TextSwitch),
        //        typeof(string), 
        //        typeof(IndicatorLabelUI), 
        //        null);

        public static readonly BindableProperty TextColorProperty =
        BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(IndicatorLabelUI), Colors.DimGray);

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public static readonly BindableProperty TextBackgroundColorProperty =
        BindableProperty.Create(nameof(TextBackgroundColor),
        typeof(Color),
        typeof(IndicatorLabelUI),
        Colors.DarkGrey);

        public Color TextBackgroundColor
        {
            get => (Color)GetValue(TextBackgroundColorProperty);
            set => SetValue(TextBackgroundColorProperty, value);
        }

        //public string TextSwitch
        //{
        //    get => (string)GetValue(TextSwitchProperty);
        //    set => SetValue(TextSwitchProperty, value);
        //}

        private Label label {  get; set; }

        public IndicatorLabelUI()
        {
            Border border = new Border
            {
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle()
                {                    
                    CornerRadius = new CornerRadius(5, 5, 5, 5)
                },
                //Background = new SolidColorBrush(Color.FromArgb("2B0B98")),
                //Background = new SolidColorBrush(Color.FromArgb("E2E2E2")),
                Padding = new Thickness(8,4),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                MaximumHeightRequest = 30
            };

            GradientStop darkGrayStop = new GradientStop { Color = Colors.Orange, Offset = 0.1f };
            GradientStop whiteStop = new GradientStop { Color = Colors.Brown, Offset = 1.0f };
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                EndPoint = new Point(0, 1),
                //GradientStops = { darkGrayStop, whiteStop }
            };

            border.Stroke = gradientBrush;

            Debug.WriteLine(Text);

            label = new Label
            {
                Text = "---",
                //FontFamily = "FontAwesome5Solid",
                TextColor = Colors.DimGray,
                FontSize = 9,
                TextTransform = TextTransform.Uppercase,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
            };

            border.SetBinding(Border.BackgroundColorProperty, new Binding(nameof(TextBackgroundColor), source: this));
            label.SetBinding(Label.TextProperty, new Binding(nameof(Text), source: this));
            label.SetBinding(Label.TextColorProperty, new Binding(nameof(TextColor), source: this));

            border.Content = label;
            border.SetBinding(IsVisibleProperty, new Binding(nameof(IsVisible), source: this));

            Content = border;
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch(propertyName)
            {
                case "Text":
                    {
                        //label.Text = Text;
                    }
                    break;
                //case "TextSwitch":
                //    {
                //        var dataTrigger = new DataTrigger(typeof(Label))
                //        {
                //            Binding = new Binding(nameof(Text), source: this),
                //            Value = TextSwitch
                //        };

                //        dataTrigger.Setters.Add(new Setter { Property = Label.TextColorProperty, Value = Colors.Orange });

                //        label.Triggers.Add(dataTrigger);
                //    }
                //    break;
            }
        }
    }
}
