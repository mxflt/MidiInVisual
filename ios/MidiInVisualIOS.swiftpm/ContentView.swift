import SwiftUI
import CoreAudioKit

struct ContentView: View {
    @StateObject private var midi = MidiInputManager()
    @State private var showBluetoothMidi = false

    var body: some View {
        ZStack(alignment: .topLeading) {
            MidiCanvas(noteStates: midi.noteStates)
                .ignoresSafeArea()

            VStack(alignment: .leading, spacing: 8) {
                HStack(spacing: 10) {
                    Button {
                        midi.refreshSources()
                    } label: {
                        Label("Refresh", systemImage: "arrow.clockwise")
                    }

                    Button {
                        showBluetoothMidi = true
                    } label: {
                        Label("Bluetooth MIDI", systemImage: "dot.radiowaves.left.and.right")
                    }
                }
                .buttonStyle(.borderedProminent)

                Text(midi.status)
                    .font(.system(.caption, design: .monospaced))
                    .foregroundStyle(.white)

                Text("Events: \(midi.eventCount)")
                    .font(.system(.caption, design: .monospaced))
                    .foregroundStyle(.white.opacity(0.9))

                Text("Last: \(midi.lastMessage)")
                    .font(.system(.caption, design: .monospaced))
                    .foregroundStyle(.white.opacity(0.9))

                if !midi.sources.isEmpty {
                    Text("Sources: \(midi.sources.joined(separator: ", "))")
                        .font(.system(.caption2, design: .monospaced))
                        .foregroundStyle(.white.opacity(0.75))
                        .lineLimit(3)
                }
            }
            .padding(14)
            .background(.black.opacity(0.35))
            .clipShape(RoundedRectangle(cornerRadius: 10))
            .padding()
        }
        .sheet(isPresented: $showBluetoothMidi) {
            BluetoothMidiView()
                .ignoresSafeArea()
                .onDisappear {
                    midi.refreshSources()
                }
        }
    }
}

struct MidiCanvas: View {
    let noteStates: [Double]

    var body: some View {
        Canvas { context, size in
            let background = Path(CGRect(origin: .zero, size: size))
            context.fill(background, with: .color(.black))

            guard !noteStates.isEmpty else { return }

            let keyCount = 88.0
            let bw = size.width / keyCount
            context.blendMode = .difference

            for i in 0..<min(88, noteStates.count) {
                let state = noteStates[i]
                guard state > 0.005 else { continue }

                let height = size.height * state
                let width = size.width / 3.0 * (1.0 - state)
                let x = bw * Double(i) + (bw - width) / 2.0
                let y = (size.height - height) / 2.0
                let rect = CGRect(x: x, y: y, width: bw + width, height: height)
                let path = Path(rect)

                context.fill(path, with: .color(.white))
                context.stroke(path, with: .color(.white.opacity(0.8)), lineWidth: 1)
            }
        }
    }
}

struct BluetoothMidiView: UIViewControllerRepresentable {
    @MainActor
    final class Coordinator {
        let controller = CABTMIDICentralViewController()
        lazy var navigationController = UINavigationController(rootViewController: controller)
    }

    func makeCoordinator() -> Coordinator {
        Coordinator()
    }

    func makeUIViewController(context: Context) -> UINavigationController {
        context.coordinator.controller.title = "Bluetooth MIDI"
        return context.coordinator.navigationController
    }

    func updateUIViewController(_ uiViewController: UINavigationController, context: Context) {
    }
}
