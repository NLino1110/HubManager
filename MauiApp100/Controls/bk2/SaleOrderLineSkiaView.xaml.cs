using DMSA.Models.Odoo.Native;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Diagnostics;
using System.Windows.Input;

namespace MauiApp100.Controls;

public partial class SaleOrderLineSkiaView : ContentView
{
    public static readonly BindableProperty ItemProperty =
        BindableProperty.Create(
            nameof(Item),
            typeof(sale_order_line),
            typeof(SaleOrderLineSkiaView),
            propertyChanged: OnItemChanged);

    public sale_order_line Item
    {
        get => (sale_order_line)GetValue(ItemProperty);
        set => SetValue(ItemProperty, value);
    }

    private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (SaleOrderLineSkiaView)bindable;
        control.canvas?.InvalidateSurface();
    }

    // HITBOXES
    private readonly List<(SKRect rect, string action)> _hits = new();

    // Estado temporal para saber qué botón se está presionando actualmente
    private string _pressedAction = string.Empty;
    private bool _isRowPressed = false;

    public ICommand? DeleteCommand { get; set; }
    public ICommand? GiftCommand { get; set; }

    public SaleOrderLineSkiaView()
    {
        InitializeComponent();
        canvas.EnableTouchEvents = true;
        canvas.Touch += OnTouch;

        this.SizeChanged += (s, e) => canvas?.InvalidateSurface();

        _ = LoadFontAwesomeAsync();
    }

    private void OnTouch(object sender, SKTouchEventArgs e)
    {
        // Evitar división por cero si la vista no se ha renderizado completamente
        if (Width <= 0 || canvas == null) return;

        float scale = (float)(canvas.CanvasSize.Width / Width);
        var p = new SKPoint(e.Location.X / scale, e.Location.Y / scale);

        if (e.ActionType == SKTouchAction.Pressed)
        {
            foreach (var h in _hits)
            {
                if (h.rect.Contains(p))
                {
                    _pressedAction = h.action;
                    canvas.InvalidateSurface();
                    e.Handled = true;
                    return;
                }
            }
        }
        else if (e.ActionType == SKTouchAction.Moved)
        {
            // En Windows, mover el ratón un píxel puede disparar este evento.
            // Si nos salimos del área del botón que se estaba presionando, cancelamos el estado visual.
            if (!string.IsNullOrEmpty(_pressedAction))
            {
                bool stillInButton = false;
                foreach (var h in _hits)
                {
                    if (h.action == _pressedAction && h.rect.Contains(p))
                    {
                        stillInButton = true;
                        break;
                    }
                }

                if (!stillInButton)
                {
                    _pressedAction = string.Empty;
                    canvas.InvalidateSurface();
                }
                e.Handled = true;
            }
        }
        else if (e.ActionType == SKTouchAction.Released)
        {
            if (!string.IsNullOrEmpty(_pressedAction))
            {
                // Verificar si el puntero se liberó dentro del botón objetivo
                foreach (var h in _hits)
                {
                    if (h.action == _pressedAction && h.rect.Contains(p))
                    {
                        switch (h.action)
                        {
                            case "DELETE":
                                DeleteCommand?.Execute(Item);
                                Debug.WriteLine("DELETE ejecutado");
                                break;

                            case "GIFT":
                                GiftCommand?.Execute(Item);
                                Debug.WriteLine("GIFT ejecutado");
                                break;
                        }
                        break;
                    }
                }

                _pressedAction = string.Empty;
                canvas.InvalidateSurface(); // Quita el efecto visual de presionado
                e.Handled = true;
            }
        }
        else if (e.ActionType == SKTouchAction.Cancelled)
        {
            if (!string.IsNullOrEmpty(_pressedAction))
            {
                _pressedAction = string.Empty;
                canvas.InvalidateSurface();
                e.Handled = true;
            }
        }
    }

    private SKTypeface? _fontAwesomeTypeface;

    private async Task LoadFontAwesomeAsync()
    {
        try
        {
            using (Stream fontStream = await FileSystem.Current.OpenAppPackageFileAsync("Font Awesome 5 Free-Solid-900.otf"))
            {
                _fontAwesomeTypeface = SKTypeface.FromStream(fontStream);
                canvas?.InvalidateSurface();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error cargando FontAwesome: {ex.Message}");
        }
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;

        canvas.Clear(SKColors.Transparent);

        // Si no hay ítem o la superficie de Skia está vacía, no dibujamos
        if (Item == null || info.Width <= 0 || info.Height <= 0) return;

        // Capturar el ancho lógico de forma segura
        float mauiWidth = (float)Width;

        // Si MAUI aún no calcula el Width (está en 0 o -1), calculamos una escala temporal basada en Windows
        if (mauiWidth <= 0)
        {
            // En Windows, habitualmente la escala por defecto es 1, o puedes usar un fallback defensivo
            mauiWidth = info.Width;
        }

        canvas.Save();
        float scale = info.Width / mauiWidth;
        canvas.Scale(scale);

        _hits.Clear();

        // Usar la variable segura de ancho
        float width = mauiWidth;

        // 3. Definición de columnas adaptativas en base al ancho lógico
        float[] cols = new float[]
        {
        30,           // Secuencia
        width * 0.15f, // Código / Producto
        width * 0.06f, // UOM
        width * 0.06f, // Cantidad Real
        width * 0.06f, // Cantidad
        width * 0.08f, // Precio Unitario
        width * 0.08f, // Subtotal
        width * 0.06f, // Descuento %
        width * 0.06f, // Importe Descuento
        width * 0.06f, // Impuesto
        width * 0.08f, // Total
        85            // Espacio fijo para botones (GIFT + DELETE)
        };

        float[] colX = new float[cols.Length];
        float acc = 0;
        for (int i = 0; i < cols.Length; i++)
        {
            colX[i] = acc;
            acc += cols[i];
        }

        // Coordenadas Y lógicas y tamaños de fuente más balanceados para pantallas
        float row1Y = 22;
        float row2Y = 42;

        var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

        // Fuentes escaladas a tamaños independientes de la densidad (DIU)
        var fontNormal = new SKFont { Size = 14 };
        var fontSmall = new SKFont { Size = 11 };
        var fontAwesome = new SKFont { Typeface = _fontAwesomeTypeface, Size = 14 };
        var fontBold = new SKFont { Size = 13, Embolden = true };
        var fontRight = new SKFont { Size = 13, Embolden = true };

        // COLUMNA 0: Secuencia
        paint.Color = SKColors.Green;
        canvas.DrawText(Item.sequence.ToString(), colX[0] + 2, row1Y, fontNormal, paint);

        // COLUMNA 1: Producto
        paint.Color = SKColors.Black;
        canvas.DrawText(Item.product_code ?? "", colX[1], row1Y, fontBold, paint);

        paint.Color = SKColors.DarkGray;
        // Desplazamiento dinámico para que no se encima con el código
        var codeBounds = new SKRect();
        fontBold.MeasureText(Item.product_code ?? "", out codeBounds);
        canvas.DrawText($"({Item.product_id})", colX[1] + codeBounds.Width + 8, row1Y, fontSmall, paint);

        paint.Color = SKColors.Black;
        canvas.DrawText(Item.product_display ?? "", colX[1], row2Y, fontSmall, paint);

        // COLUMNAS ESTÁNDAR
        canvas.DrawText(Item.uom_category_display ?? "", colX[2], row1Y, fontBold, paint);
        canvas.DrawText(Item.product_uom_qty_real.ToString("N2"), colX[3], row1Y, fontBold, paint);
        canvas.DrawText(Item.product_uom_qty.ToString("N2"), colX[4], row1Y, fontBold, paint);

        // Función auxiliar local para alineación derecha
        void DrawRight(string text, float x, float widthCol)
        {
            var bounds = new SKRect();
            fontRight.MeasureText(text, out bounds);
            canvas.DrawText(text, x + widthCol - bounds.Width - 4, row1Y, fontRight, paint);
        }

        DrawRight(Item.price_unit.ToString("N4"), colX[5], cols[5]);
        DrawRight(Item.virtual_line_subtotal.ToString("N4"), colX[6], cols[6]);
        DrawRight(Item.discount.ToString("N2"), colX[7], cols[7]);
        DrawRight(Item.amount_discount.ToString("N2"), colX[8], cols[8]);
        DrawRight(Item.price_tax.ToString("N2"), colX[9], cols[9]);
        DrawRight(Item.price_total.ToString("N2"), colX[10], cols[10]);

        // ==========================================
        // RENDERIZADO DE BOTONES (Dimensiones lógicas)
        // ==========================================
        float btnSize = 32; // Tamaño óptimo en unidades lógicas
        float startX = colX[11] + 5;
        float btnY = row1Y - 10;

        // BOTÓN DE REGALO (GIFT)
        if (Item.is_gift)
        {
            DrawButton(canvas, startX, btnY, size: btnSize, SKColors.DodgerBlue, "\uf06b", "GIFT", fontAwesome, isCircle: true);
        }

        // BOTÓN DE ELIMINAR (DELETE)
        float deleteBtnX = Item.is_gift ? (startX + btnSize + 6) : startX;
        DrawButton(canvas, deleteBtnX, btnY, size: btnSize, SKColors.OrangeRed, "\uf2ed", "DELETE", fontAwesome, isCircle: true);

        // Restaurar el lienzo a su estado original
        canvas.Restore();
    }


    private void DrawButton(
    SKCanvas canvas,
    float x,
    float y,
    float size,
    SKColor color,
    string text,
    string actionName,
    SKFont font,
    bool isCircle = false,
    float cornerRadius = 0)
    {
        // CAMBIO: Eliminamos el "- 20" para usar la "y" limpia que calculamos arriba
        var rect = new SKRect(x, y, x + size, y + size);

        // Registrar el Hitbox siempre para mantener la consistencia
        _hits.Add((rect, actionName));

        // Evaluar si este botón específico está siendo presionado actualmente
        bool isPressed = _pressedAction == actionName;

        // Si está presionado, alteramos levemente el tamaño para dar efecto "Click" de escala hacia el centro
        if (isPressed)
        {
            float offset = size * 0.06f; // Reducción del 6% del tamaño
            x += offset;
            y += offset;
            size -= (offset * 2);
            rect = new SKRect(rect.Left + offset, rect.Top + offset, rect.Right - offset, rect.Bottom - offset);
        }

        // Dibujar la forma del botón (Si está presionado, usamos un color con menor opacidad)
        var targetColor = isPressed ? color.WithAlpha((byte)(color.Alpha * 0.7f)) : color;

        using (var paint = new SKPaint { IsAntialias = true, Color = targetColor })
        {
            if (isCircle)
            {
                // Ajuste para el círculo centrado perfectamente en su nuevo bounding box
                canvas.DrawCircle(rect.MidX, rect.MidY, size / 2, paint);
            }
            else if (cornerRadius > 0)
            {
                canvas.DrawRoundRect(rect, cornerRadius, cornerRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }

        // Dibujar el Texto o Ícono centrado
        using (var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White })
        {
            var bounds = new SKRect();
            font.MeasureText(text, out bounds);

            float textX = x + (size - bounds.Width) / 2 - bounds.Left;
            float textY = (rect.Top + rect.Bottom) / 2 - bounds.MidY;

            canvas.DrawText(text, textX, textY, font, textPaint);
        }
    }

}