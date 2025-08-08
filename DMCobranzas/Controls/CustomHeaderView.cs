using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CobranzasDMSA_Odoo.Controls
{
    public class CustomHeaderView : ContentView
    {
        public static readonly BindableProperty ShowButton1Property =
            BindableProperty.Create(nameof(ShowButton1), typeof(bool), typeof(CustomHeaderView), true);

        public bool ShowButton1
        {
            get => (bool)GetValue(ShowButton1Property);
            set => SetValue(ShowButton1Property, value);
        }

        public static readonly BindableProperty ShowButton2Property =
            BindableProperty.Create(nameof(ShowButton2), typeof(bool), typeof(CustomHeaderView), true);

        public bool ShowButton2
        {
            get => (bool)GetValue(ShowButton2Property);
            set => SetValue(ShowButton2Property, value);
        }

        public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomHeaderView), string.Empty);

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly BindableProperty Button1CommandProperty =
            BindableProperty.Create(nameof(Button1Command), typeof(ICommand), typeof(CustomHeaderView), null);

        public ICommand Button1Command
        {
            get => (ICommand)GetValue(Button1CommandProperty);
            set => SetValue(Button1CommandProperty, value);
        }

        public static readonly BindableProperty Button2CommandProperty =
            BindableProperty.Create(nameof(Button2Command), typeof(ICommand), typeof(CustomHeaderView), null);

        public ICommand Button2Command
        {
            get => (ICommand)GetValue(Button2CommandProperty);
            set => SetValue(Button2CommandProperty, value);
        }

        public static readonly BindableProperty CommandParameterProperty =
            BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomHeaderView), null);

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public CustomHeaderView()
        {
            //base.BackgroundColor = Colors.Aqua;
            //base.HorizontalOptions = LayoutOptions.Fill;
            
            // Define el contenido y diseño de tu control personalizado aquí
            var titleLabel = new Label
            {
                TextColor = Colors.WhiteSmoke,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Margin = new Thickness(5, 0, 0, 0)
            };

            titleLabel.SetBinding(Label.TextProperty, new Binding(nameof(Title), source: this));

            var button1 = new Button
            {
                Text = "REPORTE",
                //HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 2,
                BorderColor = Colors.DodgerBlue,
                BorderWidth = 1,
                BackgroundColor = Colors.GhostWhite,
                FontAttributes = FontAttributes.Bold,
                FontSize = 8,
                TextColor = Colors.DodgerBlue,
                //Margin = new Thickness(0, 0, 0, 2),
                //Padding = new Thickness(3),
                //HeightRequest = 20,
            };

            button1.SetBinding(Button.CommandProperty, new Binding(nameof(Button1Command), source: this));
            button1.SetBinding(Button.CommandParameterProperty, new Binding(nameof(CommandParameter), source: this));
            button1.SetBinding(Button.IsVisibleProperty, new Binding(nameof(ShowButton1), source: this));

            var button2 = new Button
            {
                Text = "CIERRE",
                //HorizontalOptions = LayoutOptions.Center,
                //VerticalOptions = LayoutOptions.Center,
                CornerRadius = 2,
                BorderColor = Colors.DodgerBlue,
                BorderWidth = 1,
                BackgroundColor = Colors.GhostWhite,
                FontAttributes = FontAttributes.Bold,
                FontSize = 8,
                TextColor = Colors.DodgerBlue,
                Margin = 0,
                //HeightRequest = 20,
            };

            button2.SetBinding(Button.CommandProperty, new Binding(nameof(Button2Command), source: this));
            button2.SetBinding(Button.CommandParameterProperty, new Binding(nameof(CommandParameter), source: this));
            button2.SetBinding(Button.IsVisibleProperty, new Binding(nameof(ShowButton2), source: this));

            var grid = new Grid
            {
                Padding = new Thickness(0),
                //HorizontalOptions = LayoutOptions.Fill,
                //VerticalOptions = LayoutOptions.Fill,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }, // Título ocupará el espacio disponible
                    new ColumnDefinition { Width = GridLength.Auto }, // Botón 1 ocupará el ancho necesario
                    new ColumnDefinition { Width = GridLength.Auto }, // Botón 2 ocupará el ancho necesario
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star }, // Los controles estarán en una sola fila
                }
            };

            // Crear el borde delgado y gris claro utilizando una BoxView
            //BoxView border = new BoxView
            //{
            //    HeightRequest = 1,
            //    BackgroundColor = Colors.LightGray                
            //};

            Frame frame = new Frame
            {                
                BorderColor = Colors.Transparent,
                BackgroundColor = Colors.SlateGray,
                //HorizontalOptions = LayoutOptions.Fill,
                //VerticalOptions = LayoutOptions.Fill,
                Content = grid,
                CornerRadius = 0,
                Padding = new Thickness (2),
                Margin = new Thickness (0),
                HeightRequest = 35,
                VerticalOptions = LayoutOptions.Center
            };

            grid.Children.Add(titleLabel);
            Grid.SetColumn(titleLabel, 0);
            
            grid.Children.Add(button1);
            Grid.SetColumn(button1, 1);

            grid.Children.Add(button2);
            Grid.SetColumn(button2, 2);

            StackLayout stackLayout = new StackLayout {
                HorizontalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0) ,
                Padding = new Thickness(0),
            };
            //stackLayout.HorizontalOptions = LayoutOptions.Fill;
            stackLayout.VerticalOptions = LayoutOptions.Center;
            //stackLayout.BackgroundColor = Colors.LightSlateGray;
            stackLayout.Children.Add(frame);
            Content = stackLayout; // grid;

            VerticalOptions = LayoutOptions.FillAndExpand;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            //bool isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

            //YA NO ES REQUERIDO PORQUE AL PARECER EL BUG SE CORRIGIO
            //if(isWindows)
            //{
            //    //WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width - 500;
            //    WidthRequest = 800;
            //    MaximumHeightRequest = 800;
            //    HeightRequest = 33;
            //    stackLayout.HeightRequest = 30;
            //    button1.HeightRequest = 20;
            //    button2.HeightRequest = 25;
            //}
        }
    }
}
