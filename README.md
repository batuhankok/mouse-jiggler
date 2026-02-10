# 🖱️ MouseJiggler

**MouseJiggler** is a lightweight Windows tray app that prevents your PC from going idle by periodically moving the mouse cursor in a subtle, configurable, and drift-free way.

It is designed to be **portable and unobtrusive**: no installer, no taskbar window — just a tray icon.

---

## ✨ Features

- ✅ **Tray-only application**
  - Runs in the Windows system tray (no taskbar window)
  - Double-click tray icon opens **Settings** (does NOT toggle Start/Stop)

- 🖱️ **Configurable movement**
  - Set movement interval (seconds)
  - Set movement distance (pixels)

- 🎯 **Zero drift**
  - Cursor always returns to its exact original position

- 👀 **Visible jiggle**
  - Short delay makes movement noticeable for testing

- 🧠 **Idle-aware mode**
  - Only jiggles when the user has been idle for a defined threshold
  - Skips movement while you actively use the mouse/keyboard

- 🛡️ **Randomized “Safe Mode”**
  - Adds jitter to movement intervals (±%)
  - Randomizes direction/axis and slightly randomizes movement distance
  - Still drift-free (always returns to original position)

- 🔔 **Notifications**
  - Shows tray notifications (balloons) on:
    - Start / Stop
    - Settings saved
    - App ready

- 🎨 **Custom icon**
  - Mouse icon embedded into the executable (works with single-file publish)

- 🚀 **Single portable executable**
  - Self-contained .NET 8 runtime included
  - Works on Windows 10+ (x64)

---

## 🖥️ Requirements

- Windows 10 or later
- x64

---

## 📦 Download & Run

1. Download `MouseJiggler.exe` from the **Releases** page
2. Run it (no installation needed)
3. Look for the tray icon (check the `^` hidden tray icons if needed)
4. Open **Settings** to configure behavior

**First launch behavior:**
- Starts automatically
- “Start automatically on launch” is enabled by default

---

## ⚙️ Settings

You can configure:
- Interval (seconds)
- Distance (pixels)
- Start automatically on launch
- Idle-aware mode + idle threshold (seconds)
- Randomized safe mode + interval jitter (%)
- Start / Stop buttons inside Settings

**Tip for testing movement:**
- Interval: `2 seconds`
- Distance: `10+ pixels`

---

## 🔐 Privacy

MouseJiggler:
- Does not collect data
- Does not use the network
- Does not log keystrokes
- Runs entirely locally

---

## 🛠️ Build Output

CI artifact path:
