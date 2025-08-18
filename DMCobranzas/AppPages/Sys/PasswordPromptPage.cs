using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Sample.Models;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


namespace DMCobranzas.AppPages.Sys
{
    public partial class PasswordPromptPage : Popup<string>, INotifyPropertyChanged
    {
        private Label passwordLabel {  get; set; }

        public static readonly BindableProperty TitleBoxProperty = 
            BindableProperty.Create(nameof(TitleBox),
                typeof(string),
                typeof(PasswordPromptPage));

        public string TitleBox
        {
            get => (string) GetValue(TitleBoxProperty);
            set => SetValue(TitleBoxProperty, value);
        }

        public PasswordPromptPage()
        {
            InitializeComponent();
        }
        
        public void InitializeComponent()
        {
            //CanBeDismissedByTappingOutsideOfPopup = false;
            //PopupSizeConstants popupSizeConstants = new PopupSizeConstants(DeviceDisplay.Current);
            //popupSizeConstants.CalculateSizes(DeviceDisplay.Current);
            DesiredSize = new Size(350,150); // popupSizeConstants.SmallWide;
            BackgroundColor = Colors.White;

            //TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();
            passwordLabel = new Label
            {
                Text = TitleBox, //"Ingrese el pin correcto para aplicar cambios.",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center
            };

            var passwordEntry = new Entry
            {
                IsPassword = true,
                Placeholder = "PIN",
                Keyboard= Keyboard.Numeric,
                MaxLength = 4
            };

            var acceptButton = new Button
            {
                BackgroundColor = Colors.SeaGreen,
                Margin = new Thickness(2),
                Text = "OK",
                WidthRequest = 100,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            acceptButton.Clicked += async (sender, e) => {
                //taskCompletionSource.SetResult(passwordEntry.Text);                
                await CloseAsync(passwordEntry.Text);
            };

            var cancelButton = new Button
            {
                BackgroundColor = Colors.Red,
                Margin = new Thickness(2),
                Text = "Cancelar",
                WidthRequest = 100,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            cancelButton.Clicked += async (sender, e) => {
                //taskCompletionSource.SetResult(null);
                await CloseAsync(default(string));
            };

            StackLayout stackLayout = new StackLayout
            {
                MinimumHeightRequest = 150,
                MinimumWidthRequest= 300,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Padding = new Thickness(20),
                Children = {
                    passwordLabel,
                    passwordEntry,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Children = { acceptButton, cancelButton }
                    }
                },
                BackgroundColor = Colors.Transparent
            };

            Frame frameContent = new Frame
            {
                BackgroundColor = Colors.WhiteSmoke,
                Content = stackLayout,                
                BorderColor = Colors.Gray,
                CornerRadius = 10,
                Padding = new Thickness(10),
                HasShadow = true
            };
            
            Content = frameContent;
            BackgroundColor = Colors.Transparent;
            //passwordEntry.Focus();            
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case "TitleBox":
                    {
                        passwordLabel.Text = TitleBox;
                    }
                    break;
                case "ShowToolBox":
                    {
                        
                    }
                    break;
                case "ContentCustomToolBox":
                    {
                        
                    }
                    break;
            }
        }
    }
}
