using Microsoft.Maui.Graphics;
//using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Text;
using Microsoft.Maui;
using static System.Net.Mime.MediaTypeNames;
using Image = Microsoft.Maui.Controls.Image;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.ApplicationModel;

namespace BeebTech.Maui.Controls
{
    public class FloatingEntry : ContentView
    {
        private readonly Grid _customGrid;
        private readonly Entry _entryControl;
        private readonly Label _placeholderLabel;

        public FloatingEntry()
        {
            _entryControl = new Entry
            {
                //Placeholder = new Binding(nameof(Placeholder)),
                //Text = new Binding(nameof(Text)),
                //IsPassword = new Binding(nameof(HidePassword)),                
                //TextColor = new Binding(nameof(TextColor)),
                //IsReadOnly = new Binding(nameof(IsReadOnly)),
                BackgroundColor = Colors.Aqua,
                VerticalTextAlignment = TextAlignment.End,
                //VerticalOptions = LayoutOptions.End,
                MinimumHeightRequest = 100,
                Margin = new Thickness(30,30,30,30),                
            };
            
            _placeholderLabel = new Label
            {
                IsVisible = false,
                FontSize = _entryControl.FontSize - 4,
                TextColor = Colors.Gray,
                TranslationX = 5,
                TranslationY = -12
            };

            var image = new Image
            {
                WidthRequest = 20,
                Source = new FontImageSource
                {
                    //FontFamily = new Binding(nameof(FontFamily)),
                    //Color = new Binding(nameof(IconColor)),
                    Size = 20,
                    //Glyph = new Binding(nameof(Glyph)),
                    FontAutoScalingEnabled = true
                }
            };

            _customGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,                
                BackgroundColor = Colors.WhiteSmoke,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Auto }
                }
            };
            //_customGrid.HorizontalOptions = LayoutOptions.Fill;
            //_customGrid.Padding = new Thickness(10, 10, 10, 10);
            //_customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 36 });
            //_customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            //_customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = 36 });

            _entryControl.TextChanged += OnEntryTextChanged;
            _entryControl.Focused += OnEntryFocused;
            _entryControl.Unfocused += OnEntryUnfocused;
            //ContentLayout = "Top,0"
            _entryControl.Placeholder = string.Empty;
            
            _customGrid.Children.Add(_entryControl);
            Grid.SetRow(_entryControl, 0);
            Grid.SetColumn(_entryControl,1);
            Grid.SetRowSpan(_entryControl, 3);

            Picker _pickerBank = new Picker
            {
                Title = "Banco",
                Margin = new Thickness(15, 5, 15, 5)
            };

            Picker _pickerBank2 = new Picker
            {
                Title = "Banco_2",
                Margin = new Thickness(15, 5, 15, 5)
            };

            _customGrid.Children.Add(_pickerBank);
            Grid.SetRow(_pickerBank, 2);
            Grid.SetColumn(_pickerBank, 2);

            _customGrid.Children.Add(_pickerBank2);
            Grid.SetRow(_pickerBank2, 1);
            Grid.SetColumn(_pickerBank2, 2);

            Content = _customGrid;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            _placeholderLabel.IsVisible = string.IsNullOrEmpty(e.NewTextValue);
        }

        private void OnEntryFocused(object sender, FocusEventArgs e)
        {
            _placeholderLabel.TextColor = Colors.Blue; // Color del título cuando el campo está enfocado
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            _placeholderLabel.TextColor = Colors.Gray; // Color del título cuando el campo no está enfocado
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent is StackLayout stackLayout)
            {
                stackLayout.Children.Add(_placeholderLabel);
                AbsoluteLayout.SetLayoutBounds(_placeholderLabel, new Rect(5, -12, Width, Height));
            }
        }
    }
}
