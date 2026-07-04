using SkiaSharp;

namespace MidiInVisual.Wasm.Graphics;

public sealed class TGraphics
{
  readonly SKCanvas canvas;

  public TGraphics(SKCanvas canvas)
  {
    this.canvas = canvas;
  }

  public TColor FillColor { get; set; } = TColor.White;
  public TColor StrokeColor { get; set; } = TColor.White;
  public float StrokeWidth { get; set; } = 1;
  public float TextSize { get; set; } = 16;

  public void Clear(TColor color)
  {
    canvas.Clear(color.ToSkColor());
  }

  public void FillRect(double x, double y, double width, double height)
  {
    using var paint = FillPaint();
    canvas.DrawRect((float)x, (float)y, (float)width, (float)height, paint);
  }

  public void DrawRect(double x, double y, double width, double height)
  {
    using var paint = StrokePaint();
    canvas.DrawRect((float)x, (float)y, (float)width, (float)height, paint);
  }

  public void FillCircle(double x, double y, double radius)
  {
    using var paint = FillPaint();
    canvas.DrawCircle((float)x, (float)y, (float)radius, paint);
  }

  public void DrawLine(double x0, double y0, double x1, double y1)
  {
    using var paint = StrokePaint();
    canvas.DrawLine((float)x0, (float)y0, (float)x1, (float)y1, paint);
  }

  public void DrawString(string text, double x, double y)
  {
    using var paint = FillPaint();
    paint.TextSize = TextSize;
    paint.Typeface = SKTypeface.FromFamilyName("Menlo", SKFontStyle.Normal);
    canvas.DrawText(text, (float)x, (float)y, paint);
  }

  SKPaint FillPaint()
  {
    return new SKPaint
    {
      Color = FillColor.ToSkColor(),
      IsAntialias = true,
      Style = SKPaintStyle.Fill
    };
  }

  SKPaint StrokePaint()
  {
    return new SKPaint
    {
      Color = StrokeColor.ToSkColor(),
      IsAntialias = true,
      Style = SKPaintStyle.Stroke,
      StrokeWidth = StrokeWidth
    };
  }
}
