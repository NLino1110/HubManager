using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeebTech.Maui.Controls.Controls
{
    public class DataExplorer : ContentView
    {
        public DataExplorer() 
        {
            var grid = new Grid
            {
                Padding = new Thickness(1),
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star }, // Título ocupará el espacio disponible
                    new ColumnDefinition { Width = GridLength.Auto }, // Botón 1 ocupará el ancho necesario
                    new ColumnDefinition { Width = GridLength.Auto }, // Botón 2 ocupará el ancho necesario
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto }, // Los controles estarán en una sola fila
                }
            };

            Frame frame = new Frame
            {
                BorderColor = Colors.LightSlateGray,
                BackgroundColor = Colors.LightGray,
                HorizontalOptions = LayoutOptions.Fill,
                Content = grid,
                CornerRadius = 0,
                Padding = new Thickness(4),
                Margin = new Thickness(0),
            };

            StackLayout stackLayout = new StackLayout { Margin = new Thickness(5) };
            stackLayout.HorizontalOptions = LayoutOptions.Fill;
            stackLayout.BackgroundColor = Colors.LightSlateGray;
            stackLayout.Children.Add(frame);
            Content = stackLayout; // grid;
        }

        public void SetData<T>(object DataContent)
        {
            // Obtener el tipo de la clase
            Type tipo = DataContent.GetType();

            // Obtener todas las propiedades públicas de la clase
            PropertyInfo[] propiedades = tipo.GetProperties();

            // Leer y mostrar los valores de las propiedades
            foreach (var propiedad in propiedades)
            {
                string nombrePropiedad = propiedad.Name;
                object valorPropiedad = propiedad.GetValue(DataContent);

                Console.WriteLine($"Nombre de la propiedad: {nombrePropiedad}, Valor de la propiedad: {valorPropiedad}");
            }
        }
    }
}
