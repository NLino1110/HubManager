using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMCobranzas.Controls
{
    public class IndicatorUI : ContentView
    {
        public IndicatorUI()
        {
            Border border = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle()
                {                    
                    CornerRadius = new CornerRadius(0, 0, 0, 0)
                },
                Background = new SolidColorBrush(Colors.Transparent),
                Padding = new Thickness(13, 2, 2, 2),
                HorizontalOptions = LayoutOptions.Start,
                MaximumHeightRequest = 30
            };

            GradientStop darkGrayStop = new GradientStop { Color = Colors.DarkGray, Offset = 0.1f };
            GradientStop whiteStop = new GradientStop { Color = Colors.White, Offset = 1.0f };
            LinearGradientBrush gradientBrush = new LinearGradientBrush
            {
                EndPoint = new Point(0, 1),
                GradientStops = { darkGrayStop, whiteStop }
            };

            border.Stroke = gradientBrush;

            Label label = new Label
            {
                Text = "\uf0a5",
                FontFamily = "FontAwesome5Solid",
                TextColor = Color.FromArgb("FFCC99"), //Colors.Orange,
                FontSize = 17,
                TextTransform = TextTransform.Uppercase,
                FontAttributes = FontAttributes.Bold
            };

            border.Content = label;
            border.SetBinding(IsVisibleProperty, new Binding(nameof(IsVisible), source: this));

            Content = border;
        }
    }
}
