using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Controls.Base
{
    public class CustomBorderDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;

            // Dibujar solo 3 lados
            canvas.DrawLine(0, dirtyRect.Height, dirtyRect.Width, dirtyRect.Height); // abajo
            canvas.DrawLine(0, 0, 0, dirtyRect.Height);                             // izquierda
            canvas.DrawLine(dirtyRect.Width, 0, dirtyRect.Width, dirtyRect.Height); // derecha
        }
    }

}
