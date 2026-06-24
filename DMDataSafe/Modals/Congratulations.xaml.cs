using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace DMDataSafe.Modals;

public partial class Congratulations : ContentPage
{
    private readonly List<Particle> particles = new();
    private readonly Random random = new();

    private readonly SKPaint particlePaint = new()
    {
        IsAntialias = true
    };

    private readonly SKPaint textPaint = new()
    {
        Color = SKColors.Red,        
        //TextSize = 100,
        //TextAlign = SKTextAlign.Center,                
        IsAntialias = true
    };

    private IDispatcherTimer timer;

    public Congratulations()
    {
        InitializeComponent();

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);

        timer.Tick += (s, e) =>
        {
            canvasView.InvalidateSurface();
        };

        timer.Start();
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var width = e.Info.Width;
        var height = e.Info.Height;

        canvas.Clear();

        UpdateParticles(width, height);

        // Dibujar partículas
        foreach (var particle in particles)
        {
            particlePaint.Color = particle.Color;
            canvas.DrawCircle(particle.Position, particle.Size, particlePaint);
        }
        
        string text = "MUCHAS GRACIAS!!!";

        SKRect textBounds = new();
        //textPaint.MeasureText(text, ref textBounds);
        
        SKFont sKFont = new SKFont();
        sKFont.Size = 100;
        sKFont.MeasureText(text, out textBounds);

        float x = width / 2f;
        float y = (height / 2f) + (textBounds.Height / 2f);

        //SKTextAlign textAlign = SKTextAlign.Center;
        
        canvas.DrawText(text, x, y, SKTextAlign.Center, sKFont,  textPaint);
    }

    private void UpdateParticles(int width, int height)
    {        
        for (int i = 0; i < 10; i++)
        {
            float x = (float)random.NextDouble() * width;
            float y = (float)random.NextDouble() * height;
            float size = (float)random.NextDouble() * 10 + 5;

            var color = SKColor.FromHsl(
                random.Next(0, 360),
                100,
                50
            );

            particles.Add(new Particle(new SKPoint(x, y), size, color));
        }
                
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];

            p.Position = new SKPoint(
                p.Position.X + ((float)random.NextDouble() * 2 - 1),
                p.Position.Y + ((float)random.NextDouble() * 2 - 1)
            );

            p.Size -= 0.1f;

            if (p.Size <= 0)
                particles.RemoveAt(i);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        timer?.Stop(); 
    }
}

public class Particle
{
    public SKPoint Position { get; set; }
    public float Size { get; set; }
    public SKColor Color { get; set; }

    public Particle(SKPoint position, float size, SKColor color)
    {
        Position = position;
        Size = size;
        Color = color;
    }
}