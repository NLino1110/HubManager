using DMCobranzas.Models;
using CommunityToolkit.Maui.Alerts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DMSA.Models.Odoo.Accounting;

namespace DMCobranzas.Controls
{
    public class InvoicePaymentItemV2 : ContentView
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

        public static readonly BindableProperty dataItemProperty =
            BindableProperty.Create(nameof(dataItem), typeof(AccountPaymentInvoiceLineAuxiliar), typeof(InvoicePaymentItemV2), null);

        public AccountPaymentInvoiceLineAuxiliar dataItem
        {
            get => (AccountPaymentInvoiceLineAuxiliar) GetValue(dataItemProperty);
            set => SetValue(dataItemProperty, value);
        }

        Entry entryPagoImporte { get; set; }
        Label labelArticulo { get; set; } //Nombre Documento
        Label labelTotal { get; set; } //Valor Factura
        
        Label labelAmountResidual { get; set; } //Saldo
        Label labelFechaRegistro { get; set; } //Fecha Factura
        Label labelDias { get; set; } //Dias
        Label labelVendedor { get; set; } //Dias

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
            Debug.Write(dataItem.id);

            entryPagoImporte.Text = "0.00";

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

            if (propertyName == "dataItem")
            {
                labelArticulo.Text = dataItem.invoice_line_id_name;
                labelTotal.Text = "   ($ " + dataItem.invoice_amount_total.ToString() + ")";
                labelFechaRegistro.Text = "FECHA: " + dataItem.invoice_date.ToString("yyyy-MM-dd");
                labelVendedor.Text = "VEND: " + dataItem.seller;

                DateTime fechaActual = DateTime.Now;
                TimeSpan diferenciaDeTiempo = fechaActual - dataItem.invoice_date;

                int totalDays = 0;
                if (diferenciaDeTiempo.TotalDays > 0)
                {
                    totalDays = (int) diferenciaDeTiempo.TotalDays;
                }

                labelDias.Text = $"(DIAS: {totalDays})";
                labelAmountResidual.Text = "SALDO: $ " + dataItem.invoice_amount_residual.ToString();
                entryPagoImporte.Text = Settings.helpers.ParseTool.ConvertirAMoneda(dataItem.reconcile_amount.ToString());
            }
        }

        public InvoicePaymentItemV2()
        {
            ClearValueCommand = new Command(ClearValue);

            entryPagoImporte = new Entry
            {
                HorizontalTextAlignment = TextAlignment.End,
                Placeholder = "Pago",
                HorizontalOptions = LayoutOptions.Fill,
                Keyboard = Keyboard.Numeric,
                Text = "0.00",
                WidthRequest = 75,
                HeightRequest = 40,
            };

            entryPagoImporte.TextChanged += (sender, e) => {
                if (dataItem != null)
                {
                    decimal rec_am = 0;

                    //WINDOWS: se debe programar un comportamiento especial porque en Windows
                    // el teclado no limita el ingreso de caracteres
                    if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        if (entryPagoImporte.Text != "")
                        {
                            entryPagoImporte.Text = entryPagoImporte.Text.Replace(".", ",");
                            bool parseResult = decimal.TryParse(entryPagoImporte.Text, out rec_am);
                            if (parseResult)
                            {
                                dataItem.reconcile_amount = rec_am;
                            }
                            else
                            {
                                entryPagoImporte.Text = "";
                            }
                        }
                    }
                    else
                    {
                        decimal.TryParse(entryPagoImporte.Text, out rec_am);
                    }
                    
                    Debug.WriteLine(rec_am);
                }
            };

            entryPagoImporte.Focused += (sender, e) =>
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
            Grid.SetColumnSpan(labelArticulo, 2);

            mainGrid.Children.Add(labelTotal);
            Grid.SetRow(labelTotal, 0);
            Grid.SetColumn(labelTotal, 2);

            mainGrid.Children.Add(labelVendedor);
            Grid.SetRow(labelVendedor, 1);
            Grid.SetColumn(labelVendedor, 0);
            Grid.SetColumnSpan(labelVendedor, 3);

            mainGrid.Children.Add(labelFechaRegistro);
            Grid.SetRow(labelFechaRegistro, 2);
            Grid.SetColumn(labelFechaRegistro, 0);

            mainGrid.Children.Add(labelDias);
            Grid.SetRow(labelDias, 2);
            Grid.SetColumn(labelDias, 1);
            
            mainGrid.Children.Add(labelAmountResidual);
            Grid.SetRow(labelAmountResidual, 3);
            Grid.SetColumn(labelAmountResidual, 0);


            //TODO: Revisar si es requerido el cheque posfechado



            mainGrid.Children.Add(entryPagoImporte);
            Grid.SetColumn(entryPagoImporte, 5);
            Grid.SetRowSpan(entryPagoImporte, 4);
            mainGrid.Children.Add(buttonClearValue);
            Grid.SetColumn(buttonClearValue, 6);
            Grid.SetRowSpan(buttonClearValue, 4);
            //frame.Content.Children.Add(stackLayoutButtons, 5, 0);

            frame.Content = mainGrid;            

            Content = frame; // grid;
        }
    }
}
