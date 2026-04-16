# Audio Device Switcher

A lightweight Windows 11 system-tray app that lets you quickly switch your default audio output and input devices.

## Features

- **System tray icon** — always one click away
- **Left-click** the tray icon to instantly cycle your output device
- **Right-click** to open the full device list
- **Global hotkeys** — switch without touching the mouse:
  - `Ctrl+Alt+Page Up` — cycle output device
  - `Ctrl+Alt+Page Down` — cycle input device
- **Real-time updates** — detects when devices are plugged/unplugged
- **Modern WinUI 3 UI** with Windows 11 styling

## Requirements

- Windows 10 (19041) or later / Windows 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Windows App SDK 1.5+](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/)

## Build & Run

```bash
dotnet restore
dotnet build
dotnet run --project src/AudioDevSwitcher
```

Or open `AudioDevSwitcher.sln` in Visual Studio 2022 and press F5.

## Project Structure

```
AudioDevSwitcher.sln
├── src/
│   ├── AudioDevSwitcher/              # WinUI 3 app (UI, tray icon, hotkeys)
│   └── AudioDevSwitcher.Core/         # Core library (audio COM interop, services)
└── tests/
    └── AudioDevSwitcher.Core.Tests/   # Unit tests (xUnit + NSubstitute)
```

### Key Components

| Component | Purpose |
|---|---|
| `AudioDeviceService` | Enumerates devices and switches defaults via Windows Core Audio COM APIs |
| `TrayIconHelper` | System tray icon — left-click cycles, right-click opens window |
| `GlobalHotkeyHelper` | Registers system-wide keyboard shortcuts |
| `MainViewModel` | MVVM view model binding the device lists to the UI |

## How It Works

The app uses the Windows **Core Audio API** (`IMMDeviceEnumerator`) to list audio endpoints and the undocumented **`IPolicyConfig`** COM interface to change the default device — the same approach used by all major audio-switching utilities. Device change notifications (`IMMNotificationClient`) keep the UI in sync when devices are plugged in or removed.

## License

MIT
