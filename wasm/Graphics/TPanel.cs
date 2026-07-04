using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using MidiInVisual.Wasm.Midi;
using MidiInVisual.Wasm.Visual;
using SkiaSharp;

namespace MidiInVisual.Wasm.Graphics;

public sealed class TPanel : Control
{
  readonly MidiState midi;

  public TPanel(MidiState midi)
  {
    this.midi = midi;
    Program.Attach(this);
  }

  protected override void OnSizeChanged(SizeChangedEventArgs e)
  {
    base.OnSizeChanged(e);
    InvalidateVisual();
  }

  public override void Render(DrawingContext context)
  {
    base.Render(context);

    var bounds = new Rect(Bounds.Size);
    if (bounds.Width <= 1 || bounds.Height <= 1)
      return;

    context.Custom(new DrawOperation(bounds, midi.Snapshot()));
  }

  sealed class DrawOperation(Rect bounds, MidiSnapshot snapshot) : ICustomDrawOperation
  {
    public Rect Bounds { get; } = bounds;

    public void Dispose()
    {
    }

    public bool HitTest(Point p) => Bounds.Contains(p);

    public bool Equals(ICustomDrawOperation? other) => false;

    public void Render(ImmediateDrawingContext context)
    {
      if (context.TryGetFeature(typeof(ISkiaSharpApiLeaseFeature)) is not ISkiaSharpApiLeaseFeature feature)
        return;

      using var lease = feature.Lease();
      var canvas = lease.SkCanvas;
      var width = Math.Max(1, (int)Math.Round(Bounds.Width));
      var height = Math.Max(1, (int)Math.Round(Bounds.Height));

      canvas.Save();
      try
      {
        canvas.ClipRect(SKRect.Create(0, 0, width, height));
        var g = new TGraphics(canvas);
        MidiVisualizer.Draw(g, snapshot, width, height);
      }
      finally
      {
        canvas.Restore();
      }
    }
  }
}
