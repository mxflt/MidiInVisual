import { dotnet } from './_framework/dotnet.js'

const hud = document.getElementById('hud');
const startButton = document.getElementById('start-midi');
const fullscreenButton = document.getElementById('fullscreen');

let app = null;
let sourceNames = [];
let animating = false;

const runtime = await dotnet
  .withDiagnosticTracing(false)
  .withApplicationArgumentsFromQuery()
  .create();

const config = runtime.getConfig();
const exports = await runtime.getAssemblyExports(config.mainAssemblyName);
app = exports.MidiInVisual.Wasm.Program;

function resize() {
  applyResult(app.Resize(performance.now()));
}

function applyResult(json) {
  const result = JSON.parse(json);
  hud.textContent = result.hud;
  return result;
}

function setStatus(status) {
  applyResult(app.SetMidiStatus(status, performance.now()));
}

async function startMidi() {
  if (!navigator.requestMIDIAccess) {
    setStatus('MIDI is not supported in this browser');
    return;
  }

  try {
    const midiAccess = await navigator.requestMIDIAccess();
    connectInputs(midiAccess);
    midiAccess.addEventListener('statechange', () => connectInputs(midiAccess));
  } catch (error) {
    setStatus(`Web MIDI status: ${error.name || 'error'}\n${error.message || error}`);
  }
}

function connectInputs(midiAccess) {
  sourceNames = [];
  for (const input of midiAccess.inputs.values()) {
    sourceNames.push(input.name || '<unknown>');
    input.onmidimessage = handleMidiMessage;
  }

  const sourceText = sourceNames.length === 0
    ? 'No MIDI inputs found'
    : `Connected MIDI inputs: ${sourceNames.join(', ')}`;
  setStatus(sourceText);
}

function handleMidiMessage(event) {
  const data = event.data;
  let index = 0;

  while (index + 2 < data.length) {
    const status = data[index];
    const type = status & 0xF0;
    const pitch = data[index + 1];
    const velocity = data[index + 2];

    if (type === 0x80 || type === 0x90) {
      applyResult(app.OnMidiMessage(status, pitch, velocity, performance.now()));
      ensureAnimation();
      index += 3;
    } else {
      index += (type === 0xC0 || type === 0xD0) ? 2 : 3;
    }
  }
}

function tick() {
  const result = applyResult(app.Tick(performance.now()));
  if (result.active) {
    window.requestAnimationFrame(tick);
  } else {
    animating = false;
  }
}

function ensureAnimation() {
  if (animating) return;
  animating = true;
  window.requestAnimationFrame(tick);
}

function toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen().catch(error => setStatus(`Fullscreen failed: ${error.message}`));
  } else {
    document.exitFullscreen();
  }
}

startButton.addEventListener('click', startMidi);
fullscreenButton.addEventListener('click', toggleFullscreen);
window.addEventListener('resize', resize);
document.addEventListener('keydown', event => {
  if (event.key === 'f') toggleFullscreen();
});

await runtime.runMain(config.mainAssemblyName, []);
resize();
