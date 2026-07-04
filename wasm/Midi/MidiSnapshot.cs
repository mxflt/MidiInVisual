namespace MidiInVisual.Wasm.Midi;

public sealed record MidiSnapshot(
  double[] Intensities,
  bool[] Pressed,
  IReadOnlyList<MidiEvent> Events,
  int EventCount,
  string LastMessage,
  string Status);
