# MouseJigglerTray

Windows 10+ tray app that moves the mouse periodically to prevent idle/sleep.

- Configurable: interval (seconds) + move distance (pixels)
- Tray icon only (no taskbar window)
- Drift = 0 (moves then returns)

## Run (Windows)
dotnet run

## Publish (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
