using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace DMOrdersUI.Controls
{
    public class CustomImageHeaderView : ContentView
    {
        public static readonly BindableProperty ImageSourceProperty =
            BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(CustomImageHeaderView), default(ImageSource));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomImageHeaderView), string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty SubtitleProperty =
            BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(CustomImageHeaderView), string.Empty);

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public CustomImageHeaderView()
        {
            var image = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HeightRequest = 20,
                WidthRequest = 20,                
                Aspect = Aspect.AspectFit,
                Margin = new Thickness(4, 4, 4, 4)
            };
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ImageSource), source: this));

            var titleLabel = new Label
            {
                TextColor = Colors.Black,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center
            };
            titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var subtitleLabel = new Label
            {
                TextColor = Colors.Gray,
                FontSize = 11,
                VerticalOptions = LayoutOptions.Center
            };
            subtitleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Subtitle), source: this));

            var textLayout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                Spacing = 1,
                Children = { titleLabel, subtitleLabel }
            };

            var layout = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(5)
            };

            layout.Children.Add(image);
            layout.Children.Add(textLayout);

            Grid.SetColumn(image, 0);
            Grid.SetRow(image, 0);
            Grid.SetColumnSpan(image, 0);

            Grid.SetColumn(textLayout, 1);
            Grid.SetRow(textLayout, 0);
            Grid.SetColumnSpan(textLayout, 0);

            Content = layout;
        }
    }
}
