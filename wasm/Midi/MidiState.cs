namespace MidiInVisual.Wasm.Midi;

public sealed class MidiState
{
  const int FirstMidi = 21;
  const int LastMidi = 108;
  const int KeyCount = LastMidi - FirstMidi + 1;

  readonly double[] intensities = new double[KeyCount];
  readonly bool[] pressed = new bool[KeyCount];
  readonly List<MidiEvent> events = [];

  double lastTickMs = double.NaN;
  int eventCount;
  string lastMessage = "-";
  string status = "Web MIDI not started";

  public string HudText
    => $"{status}\nEvents: {eventCount}\nLast: {lastMessage}";

  public bool HasActivity
    => intensities.Any(v => v > 0.005);

  public void SetStatus(string value)
  {
    status = value;
  }

  public void HandleMessage(int statusByte, int pitch, int velocity)
  {
    var type = statusByte & 0xF0;
    if (type != 0x80 && type != 0x90) return;

    var isOn = type == 0x90 && velocity > 0;
    var index = pitch - FirstMidi;
    if (index < 0 || index >= KeyCount) return;

    pressed[index] = isOn;
    if (isOn)
    {
      intensities[index] = Math.Max(intensities[index], Math.Clamp(velocity / 127.0, 0.25, 1.0));
    }

    eventCount++;
    lastMessage = $"{(isOn ? "On " : "Off")} {NoteName(pitch)} midi={pitch} velocity={velocity}";
    events.Add(new MidiEvent(isOn, pitch, velocity, eventCount, lastMessage));
    if (events.Count > 12) events.RemoveAt(0);
  }

  public void Tick(double nowMs)
  {
    if (double.IsNaN(lastTickMs))
    {
      lastTickMs = nowMs;
      return;
    }

    var frames = Math.Clamp((nowMs - lastTickMs) / 16.666, 0, 6);
    lastTickMs = nowMs;

    for (var i = 0; i < intensities.Length; i++)
    {
      var decay = pressed[i] ? Math.Pow(0.995, frames) : Math.Pow(0.965, frames);
      intensities[i] *= decay;
      if (intensities[i] < 0.001) intensities[i] = 0;
    }
  }

  public MidiSnapshot Snapshot()
  {
    return new MidiSnapshot(
      (double[])intensities.Clone(),
      (bool[])pressed.Clone(),
      events.ToArray(),
      eventCount,
      lastMessage,
      status);
  }

  static string NoteName(int midi)
  {
    ReadOnlySpan<string> names = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
    return $"{names[midi % 12]}{midi / 12 - 1}";
  }
}
