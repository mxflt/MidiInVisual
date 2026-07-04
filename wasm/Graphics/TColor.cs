using SkiaSharp;

namespace MidiInVisual.Wasm.Graphics;

public readonly record struct TColor(byte R, byte G, byte B, byte A = 255)
{
  public static readonly TColor Black = new(0, 0, 0);
  public static readonly TColor White = new(255, 255, 255);
  public static readonly TColor Gray = new(120, 128, 136);
  public static readonly TColor Cyan = new(30, 220, 255);
  public static readonly TColor Magenta = new(255, 60, 180);
  public static readonly TColor Amber = new(255, 190, 50);

  public SKColor ToSkColor()
  {
    return new SKColor(R, G, B, A);
  }

  public TColor WithAlpha(double alpha)
  {
    return this with { A = (byte)Math.Clamp((int)(alpha * 255), 0, 255) };
  }
}
