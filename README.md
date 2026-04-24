# Audio Device Switcher

A lightweight Windows 11 system-tray app that lets you quickly switch your default audio output and input devices.

![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- **System tray icon** with a custom speaker glyph — always one click away
- **Left-click** the tray icon to instantly cycle your output device
- **Right-click** for a menu to open the full device list or exit
- **Global hotkeys** — switch without touching the mouse:
  - `Ctrl+Alt+Page Up` — cycle output device
  - `Ctrl+Alt+Page Down` — cycle input device
- **Real-time updates** — detects when devices are plugged in or removed
- **Minimize/close to tray** — the app stays running in the background
- **Tooltip** shows the current default output device
- **Configurable settings** (persisted to `%APPDATA%\AudioDevSwitcher\settings.json`):
  - Start with Windows (adds a per-user `HKCU\...\Run` entry)
  - Start minimized to tray
  - Confirmation tone when switching output — a short "ding" plays through the newly-selected device

## Requirements

- Windows 10 (19041) or later / Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) to build
- No additional runtime required if you publish self-contained

## Build & Run

From the repo root:

```bash
dotnet restore
dotnet build
dotnet run --project src/AudioDevSwitcher
```

Or open `AudioDevSwitcher.sln` in Visual Studio 2022 and press F5.

### Publishing a Release Build

To create a standalone `.exe` you can launch from a shortcut:

```bash
dotnet publish src/AudioDevSwitcher -c Release -r win-x64 --self-contained false -o publish
```

The executable lands at `publish/AudioDevSwitcher.exe`. For a fully portable single-file build (no .NET install needed on the target machine):

```bash
dotnet publish src/AudioDevSwitcher -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

### Making a Shortcut

1. Right-click `publish\AudioDevSwitcher.exe` → **Send to** → **Desktop (create shortcut)**.
2. Drag the shortcut onto the taskbar, or right-click → **Pin to Start**.
3. To launch at login: press `Win+R`, type `shell:startup`, and drop a copy of the shortcut in that folder.

## Project Structure

```
AudioDevSwitcher.sln
├── src/
│   ├── AudioDevSwitcher/              # WPF app (UI, tray icon, hotkeys)
│   └── AudioDevSwitcher.Core/         # Core library (audio COM interop, services)
├── tests/
│   └── AudioDevSwitcher.Core.Tests/   # Unit tests (xUnit + NSubstitute)
└── tools/
    └── GenerateIcon.csproj            # Build-time .ico generator
```

### Key Components

| Component | Purpose |
|---|---|
| `AudioDeviceService` | Enumerates devices and switches defaults via Windows Core Audio COM APIs |
| `TrayIconHelper` | System tray icon — left-click cycles, right-click opens menu |
| `GlobalHotkeyHelper` | Registers system-wide keyboard shortcuts (`Ctrl+Alt+PgUp`/`PgDn`) |
| `MainViewModel` | MVVM view model binding the device lists to the UI |
| `GenerateIcon` | Generates `app.ico` at build time from a Segoe MDL2 glyph |

## How It Works

The app uses the Windows **Core Audio API** (`IMMDeviceEnumerator`) to list audio endpoints and the undocumented **`IPolicyConfig`** COM interface to change the default device — the same approach used by most popular audio-switching utilities. Device change notifications (`IMMNotificationClient`) keep the UI in sync when devices are plugged in or removed.

The UI is built with **WPF** and **CommunityToolkit.Mvvm**. The tray icon is drawn at runtime as a blue circle with a speaker glyph (U+E767 from Segoe MDL2 Assets) using `System.Drawing`.

## License

MIT
