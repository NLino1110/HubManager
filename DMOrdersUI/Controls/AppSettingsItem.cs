using DMSA.Models.Odoo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrdersUI.Controls
{
    public class AppSettingsItem : ContentView
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
            BindableProperty.Create(nameof(dataItem), typeof(AppSettings), typeof(AppSettingsItem), null);

        public AppSettings dataItem
        {
            get => (AppSettings) GetValue(dataItemProperty);
            set => SetValue(dataItemProperty, value);
        }

        Entry entryValue { get; set; }
        Label labelName { get; set; }
        Label labelDescription { get; set; }

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
            entryValue.Text = "";

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
                labelName.Text = dataItem.name;
                labelDescription.Text = dataItem.description;
                entryValue.Text = dataItem.value;//Settings.helpers.ParseTool.ConvertirAMoneda(dataItem.reconcile_amount.ToString());
            }
        }

        public AppSettingsItem()
        {
            ClearValueCommand = new Command(ClearValue);

            entryValue = new Entry
            {
                HorizontalTextAlignment = TextAlignment.End,
                Placeholder = "Valor",
                HorizontalOptions = LayoutOptions.Fill,
                Text = ""
            };

            entryValue.TextChanged += (sender, e) => {
                if (dataItem != null)
                {
                    dataItem.value = entryValue.Text;
                //    decimal rec_am = 0;
                //    decimal.TryParse(entryPagoImporte.Text, out rec_am);
                //    dataItem.reconcile_amount = rec_am;
                //    Debug.WriteLine(rec_am);
                }
            };

            //entryValue.Focused += (sender, e) =>
            //{
            //    Dispatcher.Dispatch(() =>
            //    {
            //        var entry = sender as Entry;
            //        entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
            //    });
            //};

            var mainGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
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
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Star },
                    //new ColumnDefinition { Width = GridLength.Auto }
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

            labelName = new Label {
                LineBreakMode = LineBreakMode.NoWrap,
                HorizontalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.Fill, 
                VerticalOptions = LayoutOptions.Center,
            };

            labelDescription = new Label {
                ///BackgroundColor = Colors.Red,
                LineBreakMode = LineBreakMode.NoWrap,
                //Padding = 0,
                HorizontalTextAlignment = TextAlignment.Start,
                //HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };

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
                IsVisible = false,
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

            mainGrid.Children.Add(labelName);
            Grid.SetColumn(labelName, 0);
            mainGrid.Children.Add(labelDescription);
            Grid.SetColumn(labelDescription, 1);
            //mainGrid.Children.Add(labelTotal);
            //Grid.SetColumn(labelTotal, 2);
            mainGrid.Children.Add(entryValue);
            Grid.SetColumn(entryValue, 2);
            mainGrid.Children.Add(buttonClearValue);
            Grid.SetColumn(buttonClearValue, 5);
            //frame.Content.Children.Add(stackLayoutButtons, 5, 0);

            frame.Content = mainGrid;            

            Content = frame; // grid;
        }
    }
}
