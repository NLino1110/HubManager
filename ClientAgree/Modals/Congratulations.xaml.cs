using System;
using System.Drawing;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace ClientAgree.Modals;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class Congratulations : ContentPage
{
    private List<Particle> particles;
    private Random random;
    public Congratulations()
	{
        InitializeComponent();

        particles = new List<Particle>();
        random = new Random();

        //Device.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        //{
        //    UpdateParticles();
        //    canvasView.InvalidateSurface();
        //    return true;
        //});

        IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += (s, e) =>
        {
            UpdateParticles();
            canvasView.InvalidateSurface();
        };
        timer.Start();
    }

    private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        SKSurface surface = e.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();

        foreach (Particle particle in particles)
        {
            SKPaint paint = new SKPaint
            {
                Color = particle.Color,
                IsAntialias = true
            };

            canvas.DrawCircle(particle.Position, particle.Size, paint);
        }

        SKPaint textPaint = new SKPaint
        {
            Color = SKColors.Red,
            TextSize = 100,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true,
        };

        string text = "MUCHAS GRACIAS!!!";

        //SKFont fontText = new SKFont(SKTypeface.FromFamilyName("OpenSansRegular"), 200);
        //fontText.Embolden = true;

        SKRect textBounds = new SKRect();
        textPaint.MeasureText(text, ref textBounds);
        float x = (canvasView.CanvasSize.Width / 2); // - textBounds.Width ;
        float y = (canvasView.CanvasSize.Height + textBounds.Height) / 2;

        canvas.DrawText(text, x, y, textPaint);
    }

    private void UpdateParticles()
    {
        // Agregar nuevas partículas
        for (int i = 0; i < 10; i++)
        {
            float x = (float)random.NextDouble() * canvasView.CanvasSize.Width;
            float y = (float)random.NextDouble() * canvasView.CanvasSize.Height;
            float size = (float)random.NextDouble() * 10 + 5;
            SKColor color = SKColor.FromHsl(random.Next(0, 360), 100, 50);
            particles.Add(new Particle(new SKPoint(x, y), size, color));
        }

        // Actualizar posición y tamaño de las partículas existentes
        foreach (Particle particle in particles.ToList())
        {
            //particle.Position.X += (float)random.NextDouble() * 2 - 1;
            //particle.Position.Y += (float)random.NextDouble() * 2 - 1;
            particle.Position = new SKPoint(particle.Position.X + (float)random.NextDouble() * 2 - 1,
                particle.Position.Y + (float)random.NextDouble() * 2 - 1);
            particle.Size -= 0.1f;

            if (particle.Size <= 0)
                particles.Remove(particle);
        }
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