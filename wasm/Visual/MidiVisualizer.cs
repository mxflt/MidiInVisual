using MidiInVisual.Wasm.Graphics;
using MidiInVisual.Wasm.Midi;

namespace MidiInVisual.Wasm.Visual;

public static class MidiVisualizer
{
  const int FirstMidi = 21;

  public static void Draw(TGraphics g, MidiSnapshot state, int width, int height)
  {
    g.Clear(TColor.Black);

    var keyCount = state.Intensities.Length;
    var keyWidth = width / (double)keyCount;
    var centerY = height / 2.0;

    DrawGrid(g, width, height, keyCount, keyWidth);
    DrawBars(g, state, width, height, keyCount, keyWidth, centerY);
    DrawRecentEvents(g, state, width, height);
  }

  static void DrawGrid(TGraphics g, int width, int height, int keyCount, double keyWidth)
  {
    g.StrokeWidth = Math.Max(1, width / 1200f);
    g.StrokeColor = new TColor(55, 60, 68);

    for (var i = 0; i <= keyCount; i += 12)
    {
      var x = i * keyWidth;
      g.DrawLine(x, 0, x, height);
    }

    g.StrokeColor = new TColor(35, 40, 48);
    g.DrawLine(0, height / 2.0, width, height / 2.0);
  }

  static void DrawBars(TGraphics g, MidiSnapshot state, int width, int height, int keyCount, double keyWidth, double centerY)
  {
    for (var i = 0; i < keyCount; i++)
    {
      var intensity = state.Intensities[i];
      if (intensity <= 0.005) continue;

      var midi = FirstMidi + i;
      var isBlackKey = IsBlackKey(midi);
      var rectHeight = height * Math.Clamp(intensity, 0, 1);
      var extraWidth = width / 3.0 * (1 - intensity);
      var x = keyWidth * i + (keyWidth - extraWidth) / 2.0;
      var y = centerY - rectHeight / 2.0;
      var rectWidth = keyWidth + extraWidth;

      var baseColor = isBlackKey ? TColor.Magenta : TColor.Cyan;
      g.FillColor = baseColor.WithAlpha(0.22 + intensity * 0.58);
      g.StrokeColor = TColor.White.WithAlpha(0.35 + intensity * 0.5);
      g.StrokeWidth = Math.Max(1, width / 900f);
      g.FillRect(x, y, rectWidth, rectHeight);
      g.DrawRect(x, y, rectWidth, rectHeight);
    }
  }

  static void DrawRecentEvents(TGraphics g, MidiSnapshot state, int width, int height)
  {
    var pad = Math.Max(18, width * 0.018);
    var y = pad + 24;

    g.TextSize = Math.Max(18, width / 60f);
    g.FillColor = TColor.Amber;
    g.DrawString("MidiInVisual WASM / Skia", pad, y);

    g.TextSize = Math.Max(14, width / 86f);
    y += g.TextSize * 1.8;
    g.FillColor = TColor.Gray;
    g.DrawString(state.Status, pad, y);

    y += g.TextSize * 1.5;
    g.FillColor = TColor.White;
    g.DrawString($"Events: {state.EventCount}   Last: {state.LastMessage}", pad, y);

    y = height - pad - state.Events.Count * g.TextSize * 1.35;
    foreach (var e in state.Events)
    {
      g.FillColor = e.IsOn ? TColor.Cyan : TColor.Gray;
      g.DrawString(e.Text, pad, y);
      y += g.TextSize * 1.35;
    }
  }

  static bool IsBlackKey(int midi)
  {
    var pitchClass = midi % 12;
    return pitchClass is 1 or 3 or 6 or 8 or 10;
  }
}
