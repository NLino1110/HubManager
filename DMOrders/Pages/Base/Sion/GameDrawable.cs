using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Pages.Base.Sion
{
    public class Card
    {
        public ShapeType Shape { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsMatched { get; set; }
    }

    public enum ShapeType
    {
        Circle,
        Square,
        Triangle,
        Star,
        Cross,
        Xmark,
        Diamond,
        Ring
    }

    // -------------------------------------------
    // DRAWABLE
    // -------------------------------------------

    public class GameDrawable : IDrawable
    {
        private List<Card> Cards;
        public GameDrawable(List<Card> cards) => Cards = cards;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float width = dirtyRect.Width;
            float height = dirtyRect.Height;

            int rows = 4;
            int cols = 4;

            float cellW = width / cols;
            float cellH = height / rows;

            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(dirtyRect);

            for (int i = 0; i < Cards.Count; i++)
            {
                int r = i / cols;
                int c = i % cols;

                float x = c * cellW;
                float y = r * cellH;

                DrawCard(canvas, Cards[i], new RectF(x, y, cellW, cellH));
            }
        }

        private void DrawCard(ICanvas canvas, Card card, RectF rect)
        {
            if (!card.IsRevealed && !card.IsMatched)
            {
                canvas.FillColor = Colors.DarkGray;
                canvas.FillRectangle(rect);
                return;
            }

            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(rect);

            canvas.StrokeColor = Colors.White;
            canvas.StrokeSize = 3;

            float cx = rect.Center.X;
            float cy = rect.Center.Y;
            float size = Math.Min(rect.Width, rect.Height) * 0.35f;

            switch (card.Shape)
            {
                case ShapeType.Circle:
                    canvas.DrawCircle(cx, cy, size);
                    break;

                case ShapeType.Square:
                    canvas.DrawRectangle(cx - size, cy - size, size * 2, size * 2);
                    break;

                case ShapeType.Triangle:
                    PathF tri = new();
                    tri.MoveTo(cx, cy - size);
                    tri.LineTo(cx - size, cy + size);
                    tri.LineTo(cx + size, cy + size);
                    tri.Close();
                    canvas.DrawPath(tri);
                    break;

                case ShapeType.Star:
                    DrawStar(canvas, cx, cy, size);
                    break;

                case ShapeType.Cross:
                    canvas.DrawLine(cx - size, cy, cx + size, cy);
                    canvas.DrawLine(cx, cy - size, cx, cy + size);
                    break;

                case ShapeType.Xmark:
                    canvas.DrawLine(cx - size, cy - size, cx + size, cy + size);
                    canvas.DrawLine(cx - size, cy + size, cx + size, cy - size);
                    break;

                case ShapeType.Diamond:
                    PathF dia = new();
                    dia.MoveTo(cx, cy - size);
                    dia.LineTo(cx + size, cy);
                    dia.LineTo(cx, cy + size);
                    dia.LineTo(cx - size, cy);
                    dia.Close();
                    canvas.DrawPath(dia);
                    break;

                case ShapeType.Ring:
                    canvas.DrawCircle(cx, cy, size);
                    canvas.DrawCircle(cx, cy, size * 0.55f);
                    break;
            }
        }

        private void DrawStar(ICanvas canvas, float cx, float cy, float size)
        {
            const int points = 5;
            float angle = -90;

            PathF path = new();

            for (int i = 0; i < points * 2; i++)
            {
                float r = (i % 2 == 0) ? size : size * 0.4f;
                float rad = (float)(Math.PI * angle / 180.0);

                float x = cx + (float)(Math.Cos(rad) * r);
                float y = cy + (float)(Math.Sin(rad) * r);

                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);

                angle += 360f / (points * 2);
            }

            path.Close();
            canvas.DrawPath(path);
        }

        public Card? GetCardFromPoint(Point p, double width, double height, int rows, int cols)
        {
            double cellW = width / cols;
            double cellH = height / rows;

            int c = (int)(p.X / cellW);
            int r = (int)(p.Y / cellH);

            int index = r * cols + c;

            if (index < 0 || index >= Cards.Count)
                return null;

            return Cards[index];
        }
    }
}
