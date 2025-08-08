using CobranzasDMSA_Odoo.Models;
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
    public class InvoicePaymentItem : ContentView
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

        public static readonly BindableProperty facturaItemProperty =
            BindableProperty.Create(nameof(facturaItem), typeof(FacNotaCreditoDetAuxiliar), typeof(InvoicePaymentItem), null);

        public FacNotaCreditoDetAuxiliar facturaItem
        {
            get => (FacNotaCreditoDetAuxiliar) GetValue(facturaItemProperty);
            set => SetValue(facturaItemProperty, value);
        }

        //FacNotaCreditoDetAuxiliar facturaItem;

        Entry entryPagoImporte { get; set; }
        Label labelArticulo { get; set; }
        Label labelFechaRegistro { get; set; }

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
            Debug.Write(facturaItem.ARTICULO);

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

            if (propertyName == "facturaItem")
            {
                //labelArticulo.SetBinding(Label.TextProperty, new Binding(nameof(facturaItem.ARTICULO), source: this));
                labelArticulo.Text = facturaItem.ARTICULO;
                labelFechaRegistro.Text = facturaItem.FECHAREGISTRO;
                entryPagoImporte.Text = Settings.helpers.ParseTool.ConvertirAMoneda(facturaItem.reconcile_amount.ToString());
            }
        }

        public InvoicePaymentItem()
        {
            ClearValueCommand = new Command(ClearValue);

            entryPagoImporte = new Entry
            {
                HorizontalTextAlignment = TextAlignment.End,
                Placeholder = "Pago",
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Keyboard = Keyboard.Numeric,
                Text = "0.00"
            };

            entryPagoImporte.TextChanged += (sender, e) => {
                if (facturaItem != null)
                {
                    decimal rec_am = 0;
                    decimal.TryParse(entryPagoImporte.Text, out rec_am);
                    facturaItem.reconcile_amount = rec_am;
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
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Colors.WhiteSmoke,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star },
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

            labelArticulo = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            labelFechaRegistro = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            var labelTotal = new Label { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
            
            var buttonClearValue = new Button
            {
                CornerRadius = 0,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 0),
                Command = ClearValueCommand,
                CommandParameter = "", 
                MaximumWidthRequest = 120,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                IsVisible = true,
                ImageSource = new FontImageSource
                {
                    FontFamily = "FontAwesome5Solid",
                    Color = Colors.White,
                    Size = 20,
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
            Grid.SetColumn(labelArticulo, 0);

            mainGrid.Children.Add(labelFechaRegistro);
            Grid.SetColumn(labelFechaRegistro, 1);
            mainGrid.Children.Add(labelTotal);
            Grid.SetColumn(labelTotal, 2);
            mainGrid.Children.Add(entryPagoImporte);
            Grid.SetColumn(entryPagoImporte, 4);
            mainGrid.Children.Add(buttonClearValue);
            Grid.SetColumn(buttonClearValue, 5);
            //frame.Content.Children.Add(stackLayoutButtons, 5, 0);

            frame.Content = mainGrid;            

            Content = frame; // grid;
        }
    }
}
