namespace MidiInVisual.Wasm.Midi;

public readonly record struct MidiEvent(
  bool IsOn,
  int Pitch,
  int Velocity,
  int Count,
  string Text);
