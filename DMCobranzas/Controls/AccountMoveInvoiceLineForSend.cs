using CobranzasDMSA_Odoo.Models;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CobranzasDMSA_Odoo.Controls
{
    public class AccountMoveInvoiceLineForSend : ContentView
    {
        //public static readonly BindableProperty ShowButton2Property =
        //    BindableProperty.Create(nameof(ShowButton2), typeof(bool), typeof(CustomHeaderView), true);

        //public bool ShowButton2
        //{
        //    get => (bool)GetValue(ShowButton2Property);
        //    set => SetValue(ShowButton2Property, value);
        //}

        //public static readonly BindableProperty TitleProperty =
        //    BindableProperty.Create(nameof(Title), typeof(string), typeof(CustomHeaderView), string.Empty);

        //public string Title
        //{
        //    get => (string)GetValue(TitleProperty);
        //    set => SetValue(TitleProperty, value);
        //}

        //public static readonly BindableProperty Button1CommandProperty =
        //    BindableProperty.Create(nameof(Button1Command), typeof(ICommand), typeof(CustomHeaderView), null);

        //public ICommand Button1Command
        //{
        //    get => (ICommand)GetValue(Button1CommandProperty);
        //    set => SetValue(Button1CommandProperty, value);
        //}

        //public static readonly BindableProperty Button2CommandProperty =
        //    BindableProperty.Create(nameof(Button2Command), typeof(ICommand), typeof(CustomHeaderView), null);

        //public ICommand Button2Command
        //{
        //    get => (ICommand)GetValue(Button2CommandProperty);
        //    set => SetValue(Button2CommandProperty, value);
        //}

        //public static readonly BindableProperty CommandParameterProperty =
        //    BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CustomHeaderView), null);

        //public object CommandParameter
        //{
        //    get => GetValue(CommandParameterProperty);
        //    set => SetValue(CommandParameterProperty, value);
        //}

        public static readonly BindableProperty AccountMoveLineSendProperty =
            BindableProperty.Create(nameof(AccountMoveLineSend), 
                typeof(account_move_line_send), //Clase de datos auxiliar
                typeof(AccountMoveInvoiceLineForSend), //Clase contenedora
                null);

        public account_move_line_send AccountMoveLineSend
        {
            get => (account_move_line_send) GetValue(AccountMoveLineSendProperty);
            set => SetValue(AccountMoveLineSendProperty, value);
        }

        Entry entryCantidad { get; set; }
        Label labelArticulo { get; set; }
        Label labelFechaRegistro { get; set; }
        Label labelQuantity { get; set; }        
        Label labelDiscountPercentage { get; set; }
        Label labelPrice { get; set; }
        Label labelReturnQuantity { get; set; } //CAMPO INEXISTENTE EN ODOO

        //Entry entryPagoImporte = new Entry
        //{
        //    HorizontalTextAlignment = TextAlignment.End,
        //    Placeholder = "Pago",
        //    HorizontalOptions = LayoutOptions.FillAndExpand,
        //    Keyboard = Keyboard.Numeric,
        //    Text = "0.00"
        //};

        public ICommand ClearValueCommand { get; set; }

        private async void ClearValue(object obj)
        {
            Debug.Write(AccountMoveLineSend.name);

            entryCantidad.Text = "0";

            //((FacNotaCreditoDetAuxiliar)obj).reconcile_amount = 0;
            Debug.WriteLine(obj);

            //bool answer = await DisplayAlert("Envío de cobro", "Está seguro que desea enviar este cobro?", "Confirmar", "Cancelar");
            ////Debug.WriteLine("Answer: " + answer);
            //if (!answer)
            //{
            //    return;
            //}
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            //Debug.WriteLine(propertyName);

            if (propertyName == "AccountMoveLineSend")
            {
                //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
                labelArticulo.Text = AccountMoveLineSend.name;
                labelFechaRegistro.Text = AccountMoveLineSend.price_unit.ToString();
                labelQuantity.Text = "CANT. FACT.: " + AccountMoveLineSend.original_quantity.ToString();
                labelPrice.Text = "PRE. FACT.: $" + AccountMoveLineSend.price_unit.ToString();
                labelDiscountPercentage.Text = "% DESC. " + AccountMoveLineSend.discount_percentage.ToString();
                //labelReturnQuantity.Text = "CANTIDAD DEVUELTA " + AccountMoveLineSend.price_unit.ToString();
                entryCantidad.Text = Settings.helpers.ParseTool.ConvertirAMoneda(AccountMoveLineSend.quantity.ToString());
            }
        }

        public AccountMoveInvoiceLineForSend()
        {
            ClearValueCommand = new Command(ClearValue);

            entryCantidad = new Entry
            {
                HorizontalTextAlignment = TextAlignment.End,
                Placeholder = "Cantidad",
                HorizontalOptions = LayoutOptions.Fill,
                Keyboard = Keyboard.Numeric,
                Text = "0",
                WidthRequest = 75
            };

            entryCantidad.TextChanged += (sender, e) => {
                if (AccountMoveLineSend != null)
                {
                    bool boolIsNumber = true;

                    decimal rec_q = 0;

                    boolIsNumber = decimal.TryParse(entryCantidad.Text, out rec_q);

                    AccountMoveLineSend.quantity = rec_q;

                    if(!boolIsNumber)
                    {
                        entryCantidad.Text = "0";
                    }
                    else
                    {
                        if(AccountMoveLineSend.quantity > AccountMoveLineSend.original_quantity)
                        {
                            entryCantidad.Text = AccountMoveLineSend.original_quantity.ToString();
                        }
                    }

                    Debug.WriteLine(rec_q);
                }
            };

            entryCantidad.Focused += (sender, e) =>
            {
                Dispatcher.Dispatch(() =>
                {
                    var entry = sender as Entry;
                    entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
                });
            };

            var mainGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.WhiteSmoke,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var frame = new Frame
            {
                BorderColor = Colors.LightGray,
                Padding = new Thickness(5),
                Margin = new Thickness(1),
                BackgroundColor = Colors.WhiteSmoke,
                CornerRadius = 0,
                MinimumHeightRequest = 30,
                //Content = 
            };

            labelArticulo = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, FontSize = 13 };
            labelFechaRegistro = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, FontSize = 12 };
            labelQuantity = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, FontSize = 10, Margin = new Thickness(0,0,2,0)};
            labelPrice = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, FontAttributes = FontAttributes.Bold, TextColor = Colors.OrangeRed, FontSize = 10, Margin = new Thickness(2, 0, 0, 0) };
            labelDiscountPercentage = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start , FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10, Margin = new Thickness(2, 0, 0, 0) };

            //var labelTotal = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };

            var buttonClearValue = new Button
            {
                CornerRadius = 5,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
                Command = ClearValueCommand,
                CommandParameter = "",
                WidthRequest = 40,
                HeightRequest = 35,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = true,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 15,
                    FontAutoScalingEnabled = true,
                    Glyph = "\uf55a"
                }
            };

            var stackLayoutButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                IsVisible = true
            };

            //var buttonAbono = new Button
            //{
            //    CornerRadius = 0,
            //    ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
            //    Command = new Command(parameter => /* Tu lógica aquí */),
            //    CommandParameter = /* Tu lógica aquí */,
            //    MaximumWidthRequest = 120,
            //    BackgroundColor = Colors.LimeGreen,
            //    Text = "ABONO",
            //    FontAttributes = FontAttributes.Bold,
            //    FontSize = 13,
            //    HorizontalOptions = LayoutOptions.Center,
            //    IsVisible = false,
            //    ImageSource = new FontImageSource
            //    {
            //        FontFamily = "FontAwesome5Solid",
            //        Color = Colors.White,
            //        Size = 20,
            //        FontAutoScalingEnabled = true,
            //        Glyph = "\uf155"
            //    }
            //};            

            var imageButton = new ImageButton
            {
                BackgroundColor = Colors.LimeGreen,
                Padding = new Thickness(2),
                HeightRequest = 25,
                //CommandParameter = /* Tu lógica aquí */,
                IsVisible = false
            };

            imageButton.Source = new FontImageSource
            {
                FontFamily = "FontAwesome5Solid",
                Color = Colors.Black,
                Size = 10,
                FontAutoScalingEnabled = true,
                Glyph = "\uf0d9"
            };

            //stackLayoutButtons.Children.Add(buttonAbono);
            //stackLayoutButtons.Children.Add(buttonDesbloqueoItem);
            stackLayoutButtons.Children.Add(imageButton);

            mainGrid.Children.Add(labelArticulo);
            Grid.SetRow(labelArticulo, 0);
            Grid.SetColumn(labelArticulo, 0);
            Grid.SetColumnSpan(labelArticulo, 4);

            //mainGrid.Children.Add(labelFechaRegistro);
            //Grid.SetRow(labelFechaRegistro, 1);
            //Grid.SetColumn(labelFechaRegistro, 0);

            //mainGrid.Children.Add(labelTotal);
            //Grid.SetColumn(labelTotal, 2);

            mainGrid.Children.Add(labelQuantity);
            Grid.SetRow(labelQuantity, 1);
            Grid.SetColumn(labelQuantity, 0);

            mainGrid.Children.Add(labelPrice);
            Grid.SetRow(labelPrice, 1);
            Grid.SetColumn(labelPrice, 1);

            mainGrid.Children.Add(labelDiscountPercentage);
            Grid.SetRow(labelDiscountPercentage, 1);
            Grid.SetColumn(labelDiscountPercentage, 2);



            mainGrid.Children.Add(entryCantidad);
            Grid.SetRow(entryCantidad, 0);
            Grid.SetColumn(entryCantidad, 4);
            mainGrid.Children.Add(buttonClearValue);
            Grid.SetRow(buttonClearValue, 0);
            Grid.SetColumn(buttonClearValue, 5);
            //frame.Content.Children.Add(stackLayoutButtons, 5, 0);

            frame.Content = mainGrid;            

            Content = frame; // grid;
        }
    }
}
