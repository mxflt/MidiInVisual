using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Browser;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MidiInVisual.Wasm.Midi;
using MidiInVisual.Wasm.Graphics;

namespace MidiInVisual.Wasm;

[SupportedOSPlatform("browser")]
public static partial class Program
{
  internal static readonly MidiState Midi = new();
  static TPanel? panel;

  public static Task Main(string[] args)
  {
    return AppBuilder
      .Configure(() => new MidiInVisualApp())
      .WithInterFont()
      .StartBrowserAppAsync("visual-root");
  }

  [JSExport]
  public static string Resize(double nowMs)
  {
    Midi.Tick(nowMs);
    InvalidatePanel();
    return StateJson();
  }

  [JSExport]
  public static string Tick(double nowMs)
  {
    Midi.Tick(nowMs);
    InvalidatePanel();
    return StateJson();
  }

  [JSExport]
  public static string OnMidiMessage(int status, int pitch, int velocity, double nowMs)
  {
    Midi.Tick(nowMs);
    Midi.HandleMessage(status, pitch, velocity);
    InvalidatePanel();
    return StateJson();
  }

  [JSExport]
  public static string SetMidiStatus(string status, double nowMs)
  {
    Midi.Tick(nowMs);
    Midi.SetStatus(status);
    InvalidatePanel();
    return StateJson();
  }

  internal static void Attach(TPanel value)
  {
    panel = value;
  }

  static void InvalidatePanel()
  {
    Dispatcher.UIThread.Post(() => panel?.InvalidateVisual());
  }

  static string StateJson()
  {
    var active = Midi.HasActivity ? "true" : "false";
    return "{\"hud\":\"" + EscapeJson(Midi.HudText) + "\",\"active\":" + active + "}";
  }

  static string EscapeJson(string text)
  {
    return text
      .Replace("\\", "\\\\")
      .Replace("\"", "\\\"")
      .Replace("\n", "\\n")
      .Replace("\r", "\\r")
      .Replace("\t", "\\t");
  }
}

sealed class MidiInVisualApp : Application
{
  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime)
      lifetime.MainView = new TPanel(Program.Midi);

    base.OnFrameworkInitializationCompleted();
  }
}
