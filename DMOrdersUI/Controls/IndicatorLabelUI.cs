using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DMOrdersUI.Controls
{
    public class IndicatorLabelUI : ContentView
    {
        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text),
                typeof(string), 
                typeof(IndicatorLabelUI) 
                );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

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
            };

            border.Stroke = gradientBrush;

            Debug.WriteLine(Text);

            label = new Label
            {
                Text = "---",                
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
            }
        }
    }
}
