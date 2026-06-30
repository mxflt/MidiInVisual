// swift-tools-version: 6.0

// WARNING:
// This file follows Xcode's App Playground package format.

import PackageDescription
import AppleProductTypes

let package = Package(
    name: "MidiInVisualIOS",
    platforms: [
        .iOS("16.0")
    ],
    products: [
        .iOSApplication(
            name: "MidiInVisualIOS",
            targets: ["AppModule"],
            bundleIdentifier: "com.frank.MidiInVisualIOS",
            displayVersion: "1.0",
            bundleVersion: "1",
            appIcon: .placeholder(icon: .carrot),
            accentColor: .presetColor(.cyan),
            supportedDeviceFamilies: [
                .pad,
                .phone
            ],
            supportedInterfaceOrientations: [
                .portrait,
                .landscapeRight,
                .landscapeLeft,
                .portraitUpsideDown(.when(deviceFamilies: [.pad]))
            ]
        )
    ],
    targets: [
        .executableTarget(
            name: "AppModule",
            path: "."
        )
    ],
    swiftLanguageVersions: [.v6]
)
