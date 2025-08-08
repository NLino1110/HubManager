using ApiManager;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobranzasDMSA_Odoo.AppPages
{
    public class WebViewerCustom: ContentPage
    {
        public WebViewerCustom()
        {
            Grid grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },                   
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),                    
                }
            };

            // Row 0
            // The BoxView and Label are in row 0 and column 0, and so only need to be added to the
            // Grid to obtain the default row and column settings.
            grid.Add(new BoxView
            {
                Color = Colors.DarkGray
            });

            var task = Task.Run(async () =>
            {
                HubReportes reportes = new HubReportes(App.Session);
                //App.Session.CurrentUser.empresas = "1";
                int empresa = 0;
                if(App.Session.CurrentUser.empresas.Length > 0)
                {
                    empresa = App.Session.CurrentUser.empresas[0].id;
                }
                var repResponse = await reportes.Comisiones(empresa, App.Session.CurrentUser.uid);
                string finalUrlReport = App.Session.UrlReportServer + repResponse.url;

                var webView = new Microsoft.Maui.Controls.WebView
                {
                    Source = finalUrlReport //"https://www.google.com",
                };

                webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
                .EnableZoomControls(true);
                webView.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
                .DisplayZoomControls(true);

                grid.Add(webView);
            });

            Task.WaitAll(task);


            
            
            //Etiqueta de prueba
            //grid.Add(new Label
            //{
            //    Text = "Row 0, Column 1",
            //    HorizontalOptions = LayoutOptions.Center,
            //    VerticalOptions = LayoutOptions.Center
            //}, 1, 0);

            Title = "Visor de Reporte";
            Content = grid;
        }

        //public WebViewerCustom()
        //{
        //    Grid grid = new Grid
        //    {
        //        RowDefinitions =
        //        {
        //            new RowDefinition { Height = new GridLength(2, GridUnitType.Star) },
        //            new RowDefinition(),
        //            new RowDefinition { Height = new GridLength(100) }
        //        },
        //            ColumnDefinitions =
        //        {
        //            new ColumnDefinition(),
        //            new ColumnDefinition()
        //        }
        //    };

        //    // Row 0
        //    // The BoxView and Label are in row 0 and column 0, and so only need to be added to the
        //    // Grid to obtain the default row and column settings.
        //    grid.Add(new BoxView
        //        {
        //            Color = Colors.Green
        //    });
        //        grid.Add(new Label
        //        {
        //            Text = "Row 0, Column 0",
        //            HorizontalOptions = LayoutOptions.Center,
        //            VerticalOptions = LayoutOptions.Center
        //});

        //    // This BoxView and Label are in row 0 and column 1, which are specified as arguments
        //    // to the Add method.
        //    grid.Add(new BoxView
        //    {
        //        Color = Colors.Blue
        //    }, 1, 0);

        //    grid.Add(new Label
        //    {
        //        Text = "Row 0, Column 1",
        //        HorizontalOptions = LayoutOptions.Center,
        //        VerticalOptions = LayoutOptions.Center
        //    }, 1, 0);

        //    // Row 1
        //    // This BoxView and Label are in row 1 and column 0, which are specified as arguments
        //    // to the Add method overload.
        //    grid.Add(new BoxView
        //    {
        //        Color = Colors.Teal
        //    }, 0, 1);

        //    grid.Add(new Label
        //    {
        //        Text = "Row 1, Column 0",
        //        HorizontalOptions = LayoutOptions.Center,
        //        VerticalOptions = LayoutOptions.Center
        //    }, 0, 1);

        //    // This BoxView and Label are in row 1 and column 1, which are specified as arguments
        //    // to the Add method overload.
        //    grid.Add(new BoxView
        //    {
        //        Color = Colors.Purple
        //    }, 1, 1);

        //    grid.Add(new Label
        //    {
        //        Text = "Row1, Column 1",
        //        HorizontalOptions = LayoutOptions.Center,
        //        VerticalOptions = LayoutOptions.Center
        //    }, 1, 1);

        //    // Row 2
        //    // Alternatively, the BoxView and Label can be positioned in cells with the Grid.SetRow
        //    // and Grid.SetColumn methods.
        //    BoxView boxView = new BoxView { Color = Colors.Red };
        //    Grid.SetRow(boxView, 2);
        //    Grid.SetColumnSpan(boxView, 2);
        //    Label label = new Label
        //    {
        //        Text = "Row 2, Column 0 and 1",
        //        HorizontalOptions = LayoutOptions.Center,
        //        VerticalOptions = LayoutOptions.Center
        //    };
        //    Grid.SetRow(label, 2);
        //    Grid.SetColumnSpan(label, 2);

        //    grid.Add(boxView);
        //    grid.Add(label);

        //    Title = "Basic Grid demo";
        //    Content = grid;
        //}

    }
}
