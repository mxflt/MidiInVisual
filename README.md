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

- `wasm/MidiInVisual.Wasm.csproj`  
  Minimal C# browser experiment. Web MIDI still comes through JavaScript interop, but the visualization is rendered directly into an Avalonia Browser Skia surface through a deliberately tiny `TPanel` / `TGraphics` layer.

- `index.html`  
  Tiny redirect to `web/index.html`, useful for GitHub Pages.

## Current Platform Notes

| Platform / browser | Observed result | Note |
| --- | --- | --- |
| iOS native app | Works | CoreMIDI and Bluetooth MIDI work on a real device |
| iOS Simulator | Not useful | Bluetooth MIDI device discovery is not available as a realistic test |
| Chrome desktop on macOS | Works | Shows a browser permission dialog for MIDI access |
| Opera desktop on macOS | Works | Behaves like Chrome for this test |
| Firefox desktop on macOS, `file://` | Unsupported | `navigator.requestMIDIAccess` is not exposed for local HTML files |
| Firefox desktop on macOS, `localhost` | Works | Web MIDI is available when served from a local HTTP server |
| Safari desktop on macOS | Unsupported | `navigator.requestMIDIAccess` is not exposed |
| iPad Safari | Unsupported | Web MIDI is not available |
| VS Code Live Preview / embedded Electron browser | Permission denied | Web MIDI API is visible, but `requestMIDIAccess` fails with `NotAllowedError`; use a normal Chrome/Opera tab for this test |

## iOS

Open `ios/MidiInVisualIOS.swiftpm` in Xcode, select a real iPhone or iPad, and run.

Use the `Bluetooth MIDI` button to open Apple's Bluetooth MIDI device browser. After connecting a device, use `Refresh` if the source list did not update automatically.

## Web

Open `web/index.html` in a Web MIDI capable browser such as Chrome on macOS.

The browser may ask for MIDI permission. Incoming note-on and note-off events are shown as simple animated bars over an 88-key range.

`web/midi_in_visual_01.html` is the earlier minimal version. It is useful for comparison because it contains almost no surrounding structure.

## WASM

Run the C# / Skia WASM variant locally:

```sh
dotnet run --project wasm/MidiInVisual.Wasm.csproj
```

Then open the shown local URL in a Web MIDI capable browser. Press `Start MIDI` to trigger the browser permission dialog.

This variant intentionally keeps the graphics layer tiny:

- `TPanel` is a minimal Avalonia `Control` that paints through `ISkiaSharpApiLeaseFeature`.
- `TGraphics` contains only the few drawing operations needed by this visualizer.
- JavaScript is limited to Web MIDI, fullscreen, window sizing, and HUD text.

## Possible .NET / C# Variants

These variants describe useful follow-up experiments:

| Variant | MIDI access | UI/rendering | Expected result |
| --- | --- | --- | --- |
| Avalonia Browser / WASM | Web MIDI through JavaScript interop | C# + Avalonia + Skia in the browser | Possible in Chrome/Opera, but inherits the same Web MIDI browser limitations as `web/` |
| Minimal C# WASM | Web MIDI through JavaScript interop | C# + Avalonia Browser Skia through tiny `TPanel` / `TGraphics` | Implemented in `wasm/`; good as a small C# rendering probe |
| Avalonia macOS Desktop | CoreMIDI via C# native interop | C# + Avalonia + Skia as a native desktop app | Possible without Web MIDI; similar MIDI access model to a native app |
| Avalonia iOS | CoreMIDI through native iOS APIs | C# + Avalonia on iOS | Theoretically possible, but app signing, iOS lifecycle, and Bluetooth MIDI setup make it a larger experiment |
| .NET MAUI / iOS | CoreMIDI through native iOS APIs | C# + MAUI | Another native iOS route; useful mainly if MAUI is already part of the stack |
| Browser WASM with direct CoreMIDI | Not available | C# in browser sandbox | Not realistic; browser WASM cannot call Apple's CoreMIDI framework directly |

The important distinction is:

- Browser-based C# still needs Web MIDI and JavaScript interop for MIDI input.
- Native C# apps can use CoreMIDI directly on Apple platforms.

## Scope

This repository intentionally avoids additional framework code. It is a small, isolated platform probe for MIDI input.
