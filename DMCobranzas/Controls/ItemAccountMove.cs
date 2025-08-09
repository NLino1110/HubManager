using DMCobranzas.Models;
using CommunityToolkit.Maui.Alerts;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMCobranzas.Controls
{
    public class ItemAccountMove : SwipeView
    {
        public static readonly BindableProperty dataItemProperty =
            BindableProperty.Create(nameof(dataItem), 
                typeof(account_move), 
                typeof(ItemAccountMove), 
                null);

        public account_move dataItem
        {
            get => (account_move) GetValue(dataItemProperty);
            set => SetValue(dataItemProperty, value);
        }

        Label labelArticulo { get; set; } //Nombre Documento
        Label labelTotal { get; set; } //Valor Factura
        Label labelAmountResidual { get; set; } //Saldo
        Label labelFechaRegistro { get; set; } //Fecha Factura
        Label labelDias { get; set; } //Dias
        Label labelVendedor { get; set; } //Vendedor

        private ICommand CommandSelectListItem { get; set; }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            
            Debug.WriteLine(propertyName);

            if (propertyName == "BindingContext")
            {
                dataItem = (account_move) BindingContext;
                labelArticulo.Text = dataItem.name;
                labelTotal.Text = "   ($ " + dataItem.amount_total.ToString() + ")";
                labelFechaRegistro.Text = "FECHA: " + dataItem.invoice_date.ToString("yyyy-MM-dd");
                labelVendedor.Text = "VEND:" + ""; //dataItem.seller;

                DateTime fechaActual = DateTime.Now;
                TimeSpan diferenciaDeTiempo = fechaActual - dataItem.invoice_date;

                int totalDays = 0;
                if (diferenciaDeTiempo.TotalDays > 0)
                {
                    totalDays = (int) diferenciaDeTiempo.TotalDays;
                }

                labelDias.Text = $"(DIAS: {totalDays})";
                labelAmountResidual.Text = "SALDO: $ " + dataItem.amount_residual.ToString();
            }
        }

        public ItemAccountMove(ICommand CommandObject)
        {
            CommandSelectListItem = CommandObject;

            Button btnForSwipe = new Button
            {
                CornerRadius = 2,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
                Command = CommandSelectListItem,
                BackgroundColor = Colors.DeepSkyBlue,
                Text = " ",
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HorizontalOptions = LayoutOptions.End,
                WidthRequest = 100,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf058"
                }
            };

            Button btnForSwipeWindows = new Button
            {
                CornerRadius = 2,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
                Command = CommandSelectListItem,
                BackgroundColor = Colors.DeepSkyBlue,
                Text = " ",
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HorizontalOptions = LayoutOptions.End,
                WidthRequest = 100,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf058"
                }
            };

            //btnSelectSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));
            btnForSwipe.SetBinding(Button.CommandParameterProperty, new Binding("."));
            btnForSwipeWindows.SetBinding(Button.CommandParameterProperty, new Binding("."));

            RightItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;

            RightItems.Add(
                new SwipeItemView
                {
                    Content = btnForSwipe
                }
            );


            var mainGrid = new Grid
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
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var frame = new Frame
            {
                BorderColor = Colors.LightGray,
                Padding = new Thickness(2),
                Margin = new Thickness(1),
                BackgroundColor = Colors.WhiteSmoke,
                CornerRadius = 0,
                MinimumHeightRequest = 30,
                //Content = 
            };

            labelArticulo = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, FontAttributes = FontAttributes.Bold};
            labelTotal = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold };
            labelAmountResidual = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, TextColor = Colors.Red, FontAttributes = FontAttributes.Bold};
            labelFechaRegistro = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, FontAttributes = FontAttributes.Bold };
            labelDias = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, Margin = new Thickness(5,0,0,0), FontAttributes = FontAttributes.Bold };
            labelVendedor = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center, FontSize = 12, FontAttributes = FontAttributes.Bold };

            var stackLayoutButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                IsVisible = true
            };

            //var imageButton = new ImageButton
            //{
            //    BackgroundColor = Colors.LimeGreen,
            //    Padding = new Thickness(2),
            //    HeightRequest = 25,
            //    //CommandParameter = /* Tu lógica aquí */,
            //    IsVisible = true
            //};

            //imageButton.Source = new FontImageSource
            //{
            //    FontFamily = "FontAwesome5Solid",
            //    Color = Colors.Black,
            //    Size = 10,
            //    FontAutoScalingEnabled = true,
            //    Glyph = "\uf0d9"
            //};

            //stackLayoutButtons.Children.Add(buttonAbono);
            //stackLayoutButtons.Children.Add(buttonDesbloqueoItem);
            stackLayoutButtons.Children.Add(btnForSwipeWindows);
            //Grid.SetRow(btnForSwipeWindows, 0);
            //Grid.SetColumn(btnForSwipeWindows, 4);
            //Grid.SetColumnSpan(labelArticulo, 2);

            mainGrid.Children.Add(labelArticulo);
            Grid.SetRow(labelArticulo, 0);
            Grid.SetColumn(labelArticulo, 0);
            Grid.SetColumnSpan(labelArticulo, 2);

            mainGrid.Children.Add(labelTotal);
            Grid.SetRow(labelTotal, 0);
            Grid.SetColumn(labelTotal, 2);

            mainGrid.Children.Add(labelVendedor);
            Grid.SetRow(labelVendedor, 1);
            Grid.SetColumn(labelVendedor, 0);

            mainGrid.Children.Add(labelFechaRegistro);
            Grid.SetRow(labelFechaRegistro, 2);
            Grid.SetColumn(labelFechaRegistro, 0);

            mainGrid.Children.Add(labelDias);
            Grid.SetRow(labelDias, 2);
            Grid.SetColumn(labelDias, 1);
            
            mainGrid.Children.Add(labelAmountResidual);
            Grid.SetRow(labelAmountResidual, 3);
            Grid.SetColumn(labelAmountResidual, 0);
            
            mainGrid.Children.Add(stackLayoutButtons);
            Grid.SetRow(stackLayoutButtons, 0);
            Grid.SetRowSpan(stackLayoutButtons, 5);
            Grid.SetColumn(stackLayoutButtons, 6);

            //TODO: Revisar si es requerido el cheque posfechado

            frame.Content = mainGrid;            

            Content = frame; // grid;
        }
    }
}
