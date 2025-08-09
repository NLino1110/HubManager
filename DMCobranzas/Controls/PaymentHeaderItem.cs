using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DMCobranzas.Models;
using DMCobranzas.Models.Specials;
using DMSA.Models.Odoo.DMCobranzas;
using static System.Net.Mime.MediaTypeNames;

namespace DMCobranzas.Controls
{
    public class PaymentHeaderItem : SwipeView
    {
        public static readonly BindableProperty DataItemProperty =
            BindableProperty.Create(nameof(DataItem), typeof(AccountPaymentHeader), typeof(PaymentHeaderItem), null);

        public AccountPaymentHeader DataItem
        {
            get => (AccountPaymentHeader)GetValue(DataItemProperty);
            set => SetValue(DataItemProperty, value);
        }

        Label LabelRow01 { get; set; }
        Label LabelPrefixRow02 { get; set; }
        Label LabelRow02 { get; set; }
        Label LabelPrefixRow03 { get; set; }
        Label LabelRow03 { get; set; }
        Span spn_recipe_name {  get; set; }
        Span spn_payment_amount { get; set; }        
        Span spn_partner_id { get; set; }
        Span spn_partner_name { get; set; }
        IndicatorLabelUI indicatorLabel { get; set; }

        private bool isWindows { get; set; } = false;
        public PaymentHeaderItem()
        {            
            IsClippedToBounds = true;

            // Eventos
            SwipeEnded += SwipeView_SwipeEnded;
            SwipeChanging += SwipeView_SwipeChanging;
            SwipeStarted += SwipeView_SwipeStarted;

            // Configuración de SwipeItems
            var swipeItems = new SwipeItems { SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen };
            
            var swipeItemView = new SwipeItemView();

            var stackLayout = BuildButtonsBox();
                        
            swipeItemView.Content = stackLayout;
                        
            swipeItems.Add(swipeItemView);
                        
            RightItems = swipeItems;
                        
            var mainContainer = new Frame
            {
                BorderColor = Colors.Transparent,
                Margin = new Thickness(0, 1, 0, 1),
                BackgroundColor = Colors.GhostWhite,
                CornerRadius = 0,
                Padding = new Thickness(5, 0, 5, 0),
                MinimumHeightRequest = 70,                
            };
                        
            var grid = new Grid
            {
                BackgroundColor = Colors.GhostWhite,
                Margin = new Thickness(2),
                Padding = new Thickness(2),
                RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Star },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Star }
                    },
                ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
            };

            mainContainer.Content = grid;

            LabelRow01 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };

            //BuildPartnerLabelWithFormat();
            BuildPartnerLabel(LabelRow01);

            grid.Children.Add(LabelRow01);
            Grid.SetColumn(LabelRow01, 0);
            Grid.SetRow(LabelRow01, 0);
            Grid.SetColumnSpan(LabelRow01, 2);

            LabelPrefixRow02 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                TextColor = Colors.Black,
                Text = "TK:"
            };

            LabelRow02 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                TextColor = Colors.Blue,
                Padding = new Thickness(25, 0, 0, 0)
            };

            //BuildRecipeLabelWithFormat();
            BuildRecipeLabel(LabelRow02);

            //recipeLabel.SetBinding(Label.TextProperty, new Binding(nameof(DataItem.recipe_name), source: this));

            grid.Children.Add(LabelRow02);
            grid.Children.Add(LabelPrefixRow02);
            Grid.SetColumn(LabelPrefixRow02, 0);
            Grid.SetRow(LabelPrefixRow02, 1);
            Grid.SetColumn(LabelRow02, 0);
            Grid.SetRow(LabelRow02, 1);

            LabelPrefixRow03 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 12,
                TextColor = Colors.Black,
                Text = "$"
            };

            LabelRow03 = new Label
            {
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.None,
                FontSize = 12,
                TextColor = Colors.Black,
                Padding = new Thickness(10, 0, 0, 0)
            };

            //BuildRecipeLabelWithFormat();
            BuildPaymentLabel(LabelRow03);
            grid.Children.Add(LabelRow03);
            grid.Children.Add(LabelPrefixRow03);
            Grid.SetColumn(LabelPrefixRow03, 0);
            Grid.SetRow(LabelPrefixRow03, 2);
            Grid.SetColumn(LabelRow03, 0);
            Grid.SetRow(LabelRow03, 2);

            indicatorLabel = new IndicatorLabelUI
            {                
                TextColor = Colors.White,
                TextBackgroundColor = Colors.DarkSeaGreen,
                HorizontalOptions = LayoutOptions.Start
            };

            indicatorLabel.SetBinding(IndicatorLabelUI.TextProperty, new Binding("payment_status", source: DataItem));

            var dataTrigger = new DataTrigger(typeof(IndicatorLabelUI))
            {
                Binding = new Binding("payment_status", source: DataItem),
                Value = "PENDIENTE"
            };

            dataTrigger.Setters.Add(new Setter { Property = IndicatorLabelUI.TextColorProperty, Value = Colors.White });
            dataTrigger.Setters.Add(new Setter { Property = IndicatorLabelUI.TextBackgroundColorProperty, Value = Colors.Orange });

            indicatorLabel.Triggers.Add(dataTrigger);

            var dataTrigger_error = new DataTrigger(typeof(IndicatorLabelUI))
            {
                Binding = new Binding("payment_status", source: DataItem),
                Value = "ERROR"
            };

            dataTrigger_error.Setters.Add(new Setter { Property = IndicatorLabelUI.TextColorProperty, Value = Colors.White });
            dataTrigger_error.Setters.Add(new Setter { Property = IndicatorLabelUI.TextBackgroundColorProperty, Value = Colors.OrangeRed });

            indicatorLabel.Triggers.Add(dataTrigger_error);

            grid.Children.Add(indicatorLabel);
            Grid.SetColumn(indicatorLabel, 1);
            Grid.SetRow(indicatorLabel, 0);
            Grid.SetRowSpan(indicatorLabel, 3);

            var stackLayout_indicator = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,                
            };

            IndicatorUI indicatorUI = new IndicatorUI();
            stackLayout_indicator.Children.Add(indicatorUI);

            grid.Children.Add(stackLayout_indicator);
            Grid.SetColumn(stackLayout_indicator, 2);
            Grid.SetRow(stackLayout_indicator, 0);
            Grid.SetRowSpan(stackLayout_indicator, 3);

            isWindows = DeviceInfo.Current.Platform == DevicePlatform.WinUI;

            if (isWindows)
            {
                var stackLayout_win = BuildButtonsBox();

                grid.Children.Add(stackLayout_win);
                Grid.SetColumn(stackLayout_win, 3);
                Grid.SetRow(stackLayout_win, 0);
                Grid.SetRowSpan(stackLayout_win, 3);
            }

            var boxView = new BoxView
            {
                //HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0, 0, 5, 0),
                Color = Colors.LightGray,
                HeightRequest = 0.5,
                //WidthRequest = 20
            };
            grid.Children.Add(boxView);
            Grid.SetColumn(boxView, 0);
            Grid.SetRow(boxView, 2);
            Grid.SetColumnSpan(boxView, 4);

            // Añadir el Frame como contenido del SwipeView
            Content = mainContainer;
            IsClippedToBounds = true;
        }

        private void BuildRecipeLabelWithFormat()
        {
            spn_recipe_name = new Span
            {
                //Text = "---",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Blue
            };

            spn_payment_amount = new Span
            {
                //Text = "---",
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };

            //spn_recipe_name.BindingContext = DataItem;            
            //label.SetBinding(Label.TextProperty, new Binding(nameof(Text), source: this));

            //recipeLabel.BindingContext = DataItem;

            LabelRow02.FormattedText = new FormattedString
            {
                Spans = {
                    new Span { Text = "TK: ", FontSize = 14, TextColor = Colors.Black },
                    spn_recipe_name,
                    new Span { Text = "    $ ", FontSize = 11, TextColor = Colors.Black },
                    spn_payment_amount
                }
            };

            spn_recipe_name.SetBinding(Span.TextProperty, new Binding("recipe_name", source: DataItem));
            spn_payment_amount.SetBinding(Span.TextProperty, new Binding("payment_amount", source: DataItem));
        }

        private void BuildRecipeLabel(Label LabelObject)
        {
            LabelObject.SetBinding(Label.TextProperty, new Binding("recipe_name", source: DataItem));            
        }

        private void BuildPaymentLabel(Label LabelObject)
        {
            LabelObject.SetBinding(Label.TextProperty, new Binding("payment_amount", source: DataItem));
        }

        private void BuildPartnerLabelWithFormat()
        {
            spn_partner_id = new Span
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black
            };

            spn_partner_name = new Span
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                TextTransform = TextTransform.Uppercase
            };

            LabelRow01.FormattedText = new FormattedString
            {
                Spans = {
                    spn_partner_id,
                    new Span { Text = "-", FontSize = 14, TextColor = Colors.Black },
                    spn_partner_name
                }
            };

            spn_partner_id.SetBinding(Span.TextProperty, new Binding("partner_id", source: DataItem));
            spn_partner_name.SetBinding(Span.TextProperty, new Binding("partner_name", source: DataItem));
        }

        private void BuildPartnerLabel(Label LabelObject)
        {
            LabelObject.SetBinding(Label.TextProperty, new Binding("partner_display", source: DataItem));
            //LabelObject.SetBinding(Span.TextProperty, new Binding("partner_name", source: DataItem));
        }

        private StackLayout BuildButtonsBox()
        {
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(5, 5, 5, 5)
            };

            var btnReversar = BuildButton("REVERSAR", "\uf1da", "ReversarCommand", Colors.WhiteSmoke, Colors.Black, Colors.Black, "DataItem");
            var btnTicked = BuildButton("TICKET", "\uf02f", "TicketCommand", Colors.WhiteSmoke, Colors.Black, Colors.Black, "DataItem");
            var btnEditar = BuildButton("EDITAR", "\uf303", "EditCommand", Colors.LimeGreen, Colors.WhiteSmoke, Colors.WhiteSmoke, "DataItem");
            var btnEnviar = BuildButton("ENVIAR", "\uf1d8", "EnviarCobroCommand", Colors.DeepSkyBlue, Colors.WhiteSmoke, Colors.WhiteSmoke, "DataItem");

            btnEditar.IsVisible = false;
            btnEnviar.IsVisible = false;
            //stackLayout.Children.Add(btnReversar);
            stackLayout.Children.Add(btnTicked);
            stackLayout.Children.Add(btnEditar);
            stackLayout.Children.Add(btnEnviar);

            btnEditar.Triggers.Add(new DataTrigger(typeof(Button))
            {
                Binding = new Binding("payment_status", source: DataItem),
                Value = "PENDIENTE",
                Setters =
                {
                    new Setter { Property = Button.IsVisibleProperty, Value = true }
                }
            });

            btnEditar.Triggers.Add(new DataTrigger(typeof(Button))
            {
                Binding = new Binding("payment_status", source: DataItem),
                Value = "ERROR",
                Setters =
                {
                    new Setter { Property = Button.IsVisibleProperty, Value = true }
                }
            });


            btnEnviar.Triggers.Add(new DataTrigger(typeof(Button))
            {
                Binding = new Binding("payment_status"),
                Value = "PENDIENTE",
                Setters =
                {
                    new Setter { Property = Button.IsVisibleProperty, Value = true }
                }
            });

            btnEnviar.Triggers.Add(new DataTrigger(typeof(Button))
            {
                Binding = new Binding("payment_status"),
                Value = "ERROR",
                Setters =
                {
                    new Setter { Property = Button.IsVisibleProperty, Value = true }
                }
            });

            return stackLayout;
        }

        private Button BuildButton(string buttonText, string Glyph, string commandName, Color backgroundColor, Color textColor, Color imageColor, string binding)
        {
            var btnBuildButton = new Button
            {
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top,0),
                CornerRadius = 5,
                MaximumWidthRequest = 120,
                BackgroundColor = backgroundColor, //Colors.WhiteSmoke,
                Text = buttonText,
                FontAttributes = FontAttributes.Bold,
                FontSize = 13,
                TextColor = textColor,
                HorizontalOptions = LayoutOptions.Center,
                //IsVisible = false,
                Margin = new Thickness(1, 1, 1, 1),
            };

            btnBuildButton.SetBinding(Button.CommandProperty, new Binding(commandName, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ContentPage))));
            btnBuildButton.SetBinding(Button.CommandParameterProperty, new Binding(binding, source: this));

            btnBuildButton.ImageSource = new FontImageSource
            {
                FontFamily = "FontAwesome5Solid",
                Color = imageColor,
                Size = 20,
                Glyph = Glyph
            };

            return btnBuildButton;
        }

        private void SwipeView_SwipeStarted(object sender, SwipeStartedEventArgs e)
        {
            // Lógica del evento SwipeStarted
        }

        private void SwipeView_SwipeChanging(object sender, SwipeChangingEventArgs e)
        {
            // Lógica del evento SwipeChanging
        }

        private void SwipeView_SwipeEnded(object sender, SwipeEndedEventArgs e)
        {
            // Lógica del evento SwipeEnded
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == "DataItem")
            {
                Debug.WriteLine(DataItem.recipe_name);
                //indicatorLabel.Text = DataItem.payment_status;
                //Open(OpenSwipeItem.RightItems);
            }
        }
    }
}
