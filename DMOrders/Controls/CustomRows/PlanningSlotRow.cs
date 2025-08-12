using CobranzasDMSA_Odoo.Models;
using CommunityToolkit.Maui.Behaviors;
using DMOrders.Pages.Fragments.Activities;
using DMSA.Models.Odoo.DMOrders;
using DMSA.Models.Odoo.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DMOrders.Controls.CustomRows
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class PlanningSlotRow : BaseRow<PlanningSlot>
    {
        Label labelId { get; set; }
        Label labelCompany { get; set; }
        Label labelReason { get; set; }
        Label labelPartner { get; set; }
        Label labelStartDate { get; set; }
        Label labelEndDate { get; set; }
        Label labelStandby { get; set; }

        public PlanningSlotRow()
        {            
            
        }

        protected override void BuildLeftGridContent(Grid leftGrid)
        {
            labelId = new Label { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, TextColor = Colors.Black, FontSize = 10, BackgroundColor = Colors.Transparent, Padding = new Thickness(3), Margin = new Thickness(0) };
            labelCompany = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelReason = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelPartner = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, FontSize = 13, BackgroundColor = Colors.Transparent };
            labelStartDate = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.DarkSlateGray, FontSize = 12 };
            labelEndDate = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.OrangeRed, FontSize = 10 };
            labelStandby = new Label { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill, FontAttributes = FontAttributes.Bold, TextColor = Colors.Green, FontSize = 10 };
            
            labelId.SetBinding(Label.TextProperty, new Binding(nameof(Item.id), source: Item));
            labelCompany.SetBinding(Label.TextProperty, new Binding(nameof(Item.res_company_display), source: Item));
            labelReason.SetBinding(Label.TextProperty, new Binding(nameof(Item.res_company_display), source: Item));
            labelPartner.SetBinding(Label.TextProperty, new Binding(nameof(Item.res_partner_display), source: Item));
            labelStartDate.SetBinding(Label.TextProperty, new Binding(nameof(Item.start_datetime), source: Item, stringFormat: "{0:hh\\:mm}"));
            labelEndDate.SetBinding(Label.TextProperty, new Binding(nameof(Item.end_datetime), source: Item, stringFormat: "{0:hh\\:mm}"));
            labelStandby.SetBinding(Label.TextProperty, new Binding(nameof(Item.end_datetime), source: Item, stringFormat: "{0:hh\\:mm}"));

            var cellGrid = new Grid
            {
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(0),
                ColumnSpacing = 0,
                RowSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },                    
                }
            };

            cellGrid.Children.Add(labelCompany);
            cellGrid.Children.Add(labelReason);
            Grid.SetRow(labelReason, 1);

            AddCell(CreateCell(labelId), "left", 0, 0);
            AddCell(CreateCell(cellGrid), "left", 0, 2);
            AddCell(CreateCell(labelPartner), "left", 0, 3);
            AddCell(CreateCell(labelStartDate), "left", 0, 4);
            AddCell(CreateCell(labelEndDate), "left", 0, 5);
            AddCell(CreateCell(labelStandby), "left", 0, 6);
        }

        protected override void BuildToolGridContent(Grid toolGrid)
        {
            var buttonEdit = new Button
            {
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.DodgerBlue,
                Text = "",
                TextColor = Colors.White,
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
                    Glyph = "\uf303"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            buttonEdit.SetBinding(Button.CommandProperty, new Binding("EditCommand", source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ActivityRow))));
            buttonEdit.SetBinding(Button.CommandParameterProperty, new Binding("Item", source: this));

            var buttonDelete = new Button
            {
                //Command = EditCommand,
                CommandParameter = "",
                HeightRequest = 35,
                WidthRequest = 35,
                BackgroundColor = Colors.OrangeRed,
                Text = "",
                TextColor = Colors.White,
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
                    Glyph = "\uf2ed"
                },
                Padding = new Thickness(3),
                Margin = new Thickness(2),
            };

            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0),
                BackgroundColor = Colors.Transparent
            };

            stackLayout.Children.Add(buttonEdit);
            stackLayout.Children.Add(buttonDelete);

            toolGrid.Children.Add(stackLayout);
            Grid.SetRow(stackLayout, 0);
            Grid.SetRowSpan(stackLayout, 2);
            Grid.SetColumn(stackLayout, 4);
        }
    }
}
