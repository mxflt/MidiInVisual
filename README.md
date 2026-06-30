# MidiInVisual

Small MIDI input visualizers for comparing native iOS CoreMIDI with the browser Web MIDI API.

This is a practical test project, not a MIDI framework. It exists to answer a narrow question:

Can a device receive live MIDI note events from an electronic piano, and can those events be visualized with very little code?

## Projects

- `ios/MidiInVisualIOS.swiftpm`  
  Native SwiftUI iOS app using CoreMIDI and Apple's Bluetooth MIDI browser.

- `web/index.html`  
  Minimal Web MIDI visualizer for browsers that expose `navigator.requestMIDIAccess`.

- `web/midi_in_visual_01.html`  
  Original tiny browser experiment kept as a baseline/reference.

## Current Platform Notes

| Platform | MIDI input |
| --- | --- |
| iOS native app | Works with CoreMIDI and Bluetooth MIDI on a real device |
| iOS Simulator | Bluetooth MIDI device discovery is not a useful test |
| Chrome desktop on macOS | Web MIDI works |
| Safari desktop on macOS | Web MIDI is not available |
| iPad Safari | Web MIDI is not available |

## iOS

Open `ios/MidiInVisualIOS.swiftpm` in Xcode, select a real iPhone or iPad, and run.

Use the `Bluetooth MIDI` button to open Apple's Bluetooth MIDI device browser. After connecting a device, use `Refresh` if the source list did not update automatically.

## Web

Open `web/index.html` in a Web MIDI capable browser such as Chrome on macOS.

The browser may ask for MIDI permission. Incoming note-on and note-off events are shown as simple animated bars over an 88-key range.

`web/midi_in_visual_01.html` is the earlier minimal version. It is useful for comparison because it contains almost no surrounding structure.

## Scope

This repository intentionally avoids application-specific code from tabla or Ruscelli. It is a small, isolated platform probe for MIDI input.
