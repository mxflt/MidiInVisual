import CoreMIDI
import Foundation

@MainActor
final class MidiInputManager: ObservableObject {
    nonisolated static let firstMidi = 21
    nonisolated static let lastMidi = 108

    @Published var keyStates = Array(repeating: false, count: 88)
    @Published var noteStates = Array(repeating: 0.0, count: 88)
    @Published var sources: [String] = []
    @Published var status = "Starting MIDI..."
    @Published var lastMessage = "-"
    @Published var eventCount = 0

    private var client = MIDIClientRef()
    private var inputPort = MIDIPortRef()
    private var timer: Timer?
    private var connectedSources = Set<MIDIEndpointRef>()

    init() {
        startMidi()
        startAnimationTimer()
    }

    deinit {
        if inputPort != 0 {
            MIDIPortDispose(inputPort)
        }
        if client != 0 {
            MIDIClientDispose(client)
        }
    }

    func refreshSources() {
        connectSources()
    }

    private func startMidi() {
        let clientStatus = MIDIClientCreate("MidiInVisualIOS Client" as CFString, nil, nil, &client)
        guard clientStatus == noErr else {
            status = "MIDIClientCreate failed: \(clientStatus)"
            return
        }

        let refCon = UnsafeMutableRawPointer(Unmanaged.passUnretained(self).toOpaque())
        let portStatus = MIDIInputPortCreate(
            client,
            "MidiInVisualIOS Input" as CFString,
            midiReadProc,
            refCon,
            &inputPort)

        guard portStatus == noErr else {
            status = "MIDIInputPortCreate failed: \(portStatus)"
            return
        }

        connectSources()
    }

    private func connectSources() {
        let count = MIDIGetNumberOfSources()
        var names: [String] = []

        for index in 0..<count {
            let source = MIDIGetSource(index)
            guard source != 0 else { continue }

            names.append(endpointName(source))

            if connectedSources.contains(source) {
                continue
            }

            let connectStatus = MIDIPortConnectSource(inputPort, source, nil)
            if connectStatus == noErr {
                connectedSources.insert(source)
            }
        }

        sources = names
        status = count == 0
            ? "No MIDI sources. Try Bluetooth MIDI."
            : "Connected MIDI sources: \(count)"
    }

    private func startAnimationTimer() {
        timer = Timer.scheduledTimer(withTimeInterval: 1.0 / 60.0, repeats: true) { [weak self] _ in
            Task { @MainActor in
                self?.decay()
            }
        }
    }

    private func decay() {
        for i in noteStates.indices {
            noteStates[i] *= keyStates[i] ? 0.995 : 0.97
            if noteStates[i] < 0.001 {
                noteStates[i] = 0
            }
        }
    }

    nonisolated fileprivate func handlePacketList(_ packetList: UnsafePointer<MIDIPacketList>) {
        var packet = packetList.pointee.packet

        for _ in 0..<packetList.pointee.numPackets {
            let bytes = packetBytes(packet)
            handleBytes(bytes)

            packet = withUnsafePointer(to: &packet) { pointer in
                MIDIPacketNext(pointer).pointee
            }
        }
    }

    nonisolated private func handleBytes(_ bytes: [UInt8]) {
        var index = 0
        while index + 2 < bytes.count {
            let status = bytes[index]
            if status < 0x80 {
                index += 1
                continue
            }

            let messageType = status & 0xF0
            let pitch = bytes[index + 1]
            let velocity = bytes[index + 2]

            switch messageType {
            case 0x90:
                handleNote(midi: pitch, velocity: velocity, isOn: velocity > 0)
                index += 3
            case 0x80:
                handleNote(midi: pitch, velocity: velocity, isOn: false)
                index += 3
            default:
                index += midiMessageLength(status)
            }
        }
    }

    nonisolated private func handleNote(midi: UInt8, velocity: UInt8, isOn: Bool) {
        guard midi >= UInt8(Self.firstMidi), midi <= UInt8(Self.lastMidi) else {
            return
        }

        let keyIndex = Int(midi) - Self.firstMidi
        Task { @MainActor in
            keyStates[keyIndex] = isOn
            if isOn {
                noteStates[keyIndex] = 1.0
            }

            eventCount += 1
            let name = Self.noteName(Int(midi))
            lastMessage = "\(isOn ? "On " : "Off") \(name) midi=\(midi) velocity=\(velocity)"
        }
    }

    nonisolated private func packetBytes(_ packet: MIDIPacket) -> [UInt8] {
        var packet = packet
        let length = Int(packet.length)
        return withUnsafeBytes(of: &packet.data) { rawBuffer in
            Array(rawBuffer.prefix(length))
        }
    }

    nonisolated private func midiMessageLength(_ status: UInt8) -> Int {
        switch status & 0xF0 {
        case 0xC0, 0xD0:
            return 2
        default:
            return 3
        }
    }

    private func endpointName(_ endpoint: MIDIEndpointRef) -> String {
        var value: Unmanaged<CFString>?
        let status = MIDIObjectGetStringProperty(endpoint, kMIDIPropertyDisplayName, &value)
        if status == noErr, let value {
            return value.takeRetainedValue() as String
        }

        var name: Unmanaged<CFString>?
        let nameStatus = MIDIObjectGetStringProperty(endpoint, kMIDIPropertyName, &name)
        if nameStatus == noErr, let name {
            return name.takeRetainedValue() as String
        }

        return "<unknown>"
    }

    static func noteName(_ midi: Int) -> String {
        let names = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"]
        let name = names[midi % 12]
        let octave = midi / 12 - 1
        return "\(name)\(octave)"
    }
}

private let midiReadProc: MIDIReadProc = { packetList, refCon, _ in
    guard let refCon else {
        return
    }

    let manager = Unmanaged<MidiInputManager>.fromOpaque(refCon).takeUnretainedValue()
    manager.handlePacketList(packetList)
}
