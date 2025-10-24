using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;

namespace DMOrders.Controls;

public partial class ConfettiOverlay : ContentView
{
    // ==== Config =====
    public int DurationMs { get; set; } = 600;       // tiempo total del burst
    public int ParticleCount { get; set; } = 24;     // # de partículas por burst
    public float Gravity { get; set; } = 1800f;      // px/s^2 (ajusta a gusto)
    public float Drag { get; set; } = 0.98f;         // amortiguación por frame
    public float StartSpeedMin { get; set; } = 600f; // px/s
    public float StartSpeedMax { get; set; } = 1100f;

    // Centro del burst (en px relativo al Overlay)
    SKPoint _origin;
    bool _animating;
    readonly Stopwatch _sw = new();

    // Pool (sin allocations por frame)
    struct Particle
    {
        public SKPoint Pos;
        public SKPoint Vel;    // px/s
        public float Life;     // 0..1 (1 = nueva, 0 = muerta)
        public float Angle;    // para girar rectángulos
        public float Size;     // px
        public SKColor Color;
        public byte Shape;     // 0=círculo, 1=rectángulo
        public float Spin;     // rad/s
    }

    Particle[] _ps = Array.Empty<Particle>();
    readonly SKPaint _paint = new()
    {
        IsAntialias = true,
        FilterQuality = SKFilterQuality.Medium
    };

    static readonly SKColor[] _palette =
    {
        new SKColor(244, 67, 54),   // rojo
        new SKColor(33, 150, 243),  // azul
        new SKColor(76, 175, 80),   // verde
        new SKColor(255, 193, 7),   // ámbar
        new SKColor(156, 39, 176),  // púrpura
        new SKColor(255, 87, 34)    // naranja
    };
    readonly Random _rng = new();

    public ConfettiOverlay()
	{
		InitializeComponent();
        SizeChanged += (_, __) => Canvas?.InvalidateSurface();
    }

    // Trigger en el centro de la pantalla
    public void TriggerCenter() => TriggerAt(new Point(Width / 2, Height / 2));

    // Trigger en una posición (relativa a este overlay)
    public void TriggerAt(Point origin, int? count = null, int? durationMs = null)
    {
        if (_animating) return; // muy simple: evitar solapamientos fuertes
        if (Width <= 0 || Height <= 0) return;

        _origin = new SKPoint((float)origin.X, (float)origin.Y);
        if (count.HasValue) ParticleCount = count.Value;
        if (durationMs.HasValue) DurationMs = durationMs.Value;

        // Inicializar pool
        if (_ps.Length != ParticleCount) _ps = new Particle[ParticleCount];
        for (int i = 0; i < _ps.Length; i++)
        {
            float angle = (float)(_rng.NextDouble() * Math.PI * 2); // 0..2π
            float speed = Lerp(StartSpeedMin, StartSpeedMax, (float)_rng.NextDouble());

            var p = new Particle
            {
                Pos = _origin,
                Vel = new SKPoint((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed - 200f),
                Life = 1f,
                Angle = (float)(_rng.NextDouble() * Math.PI * 2),
                Size = 6f + (float)_rng.NextDouble() * 8f,
                Color = _palette[_rng.Next(_palette.Length)],
                Shape = (byte)(_rng.Next(2)), // círculo o rectángulo
                Spin = ((float)_rng.NextDouble() - 0.5f) * 6f // -3..+3 rad/s
            };
            _ps[i] = p;
        }

        IsVisible = true;
        _sw.Restart();
        _animating = true;

        // ~33 ms (30 fps) para ahorrar batería; sube a 16 ms si lo quieres más fluido
        Device.StartTimer(TimeSpan.FromMilliseconds(33), () =>
        {
            Canvas.InvalidateSurface();
            if (_sw.ElapsedMilliseconds >= DurationMs)
            {
                _animating = false;
                _sw.Stop();
                IsVisible = false;
                return false;
            }
            return true;
        });
    }

    void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear(SKColors.Transparent);

        if (!_animating) return;

        float dt = 0.033f; // ~33 ms (por el timer)
        float t = Math.Clamp(_sw.ElapsedMilliseconds / (float)DurationMs, 0f, 1f);

        // Easing global para desvanecer todo el burst al final
        float EaseOut(float x) => 1 - (float)Math.Pow(1 - x, 3);
        float globalFade = 1f - EaseOut(t); // 1 -> 0

        for (int i = 0; i < _ps.Length; i++)
        {
            ref var p = ref _ps[i];
            if (p.Life <= 0f) continue;

            // Física simple
            p.Vel.Y += Gravity * dt;
            p.Vel.X *= Drag; p.Vel.Y *= Drag;
            p.Pos.X += p.Vel.X * dt;
            p.Pos.Y += p.Vel.Y * dt;
            p.Angle += p.Spin * dt;

            // Vida: lineal a lo largo de la duración global
            p.Life = 1f - t;

            // Alpha por vida y fade global
            byte a = (byte)(Math.Clamp(p.Life * globalFade, 0f, 1f) * 255);
            if (a == 0) { p.Life = 0; continue; }

            _paint.Color = p.Color.WithAlpha(a);

            // Dibujo
            canvas.Save();
            canvas.Translate(p.Pos.X, p.Pos.Y);
            if (p.Shape == 0)
            {
                // círculo
                canvas.DrawCircle(0, 0, p.Size * 0.6f, _paint);
            }
            else
            {
                // rectángulo rotando
                canvas.RotateRadians(p.Angle);
                var half = p.Size * 0.6f;
                canvas.DrawRect(-half, -half, half * 2, half * 2, _paint);
            }
            canvas.Restore();
        }
    }

    static float Lerp(float a, float b, float t) => a + (b - a) * t;
}