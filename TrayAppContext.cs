using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseJiggler
{
    public sealed class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly System.Windows.Forms.Timer _timer;
        private AppConfig _cfg;
        private bool _isTickRunning = false;
        private const int VisibleDelayMs = 80;
        private SettingsForm? _settingsForm;
        private readonly Random _rng = new Random();

        public TrayAppContext()
        {
            _cfg = AppConfig.Load();

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += async (_, __) => await OnTickAsync();
            ApplyTimerInterval(resetNow: true);

            var startItem = new ToolStripMenuItem("Start", null, (_, __) => Start());
            var stopItem  = new ToolStripMenuItem("Stop", null, (_, __) => Stop()) { Enabled = false };
            var settingsItem = new ToolStripMenuItem("Settings...", null, (_, __) => OpenSettings());
            var jiggleNowItem = new ToolStripMenuItem("Jiggle now", null, async (_, __) => await JiggleOnceAsync(force: true));
            var exitItem = new ToolStripMenuItem("Exit", null, (_, __) => Exit());

            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[]
            {
                startItem,
                stopItem,
                new ToolStripSeparator(),
                settingsItem,
                jiggleNowItem,
                new ToolStripSeparator(),
                exitItem
            });

            _trayIcon = new NotifyIcon
            {
                Text = "MouseJiggler",
                Icon = LoadEmbeddedIconOrFallback(),
                ContextMenuStrip = menu,
                Visible = true
            };

            _trayIcon.DoubleClick += (_, __) => OpenSettings();

            void SyncMenu()
            {
                startItem.Enabled = !_timer.Enabled;
                stopItem.Enabled = _timer.Enabled;
            }

            startItem.Click += (_, __) => SyncMenu();
            stopItem.Click  += (_, __) => SyncMenu();

            ShowBalloon("MouseJiggler is ready.");

            if (_cfg.IsFirstRun || _cfg.StartOnLaunch)
            {
                Start();
                SyncMenu();
            }

            AppConfig.Save(_cfg);
        }

        private Icon LoadEmbeddedIconOrFallback()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using var s = asm.GetManifestResourceStream("MouseJiggler.Assets.mouse.ico");
                if (s != null) return new Icon(s);
            }
            catch { }
            return SystemIcons.Application;
        }

        private void ShowBalloon(string message)
        {
            try
            {
                _trayIcon.BalloonTipTitle = "MouseJiggler";
                _trayIcon.BalloonTipText = message;
                _trayIcon.ShowBalloonTip(2000);
            }
            catch { }
        }

        private void ApplyTimerInterval(bool resetNow)
        {
            int baseMs = Math.Max(1000, _cfg.Seconds * 1000);

            if (_cfg.SafeMode && _cfg.RandomJitterPercent > 0)
            {
                int p = Math.Clamp(_cfg.RandomJitterPercent, 0, 80);
                double factor = 1.0 + (_rng.NextDouble() * 2.0 - 1.0) * (p / 100.0);
                int ms = (int)Math.Round(baseMs * factor);
                _timer.Interval = Math.Max(1000, ms);
            }
            else
            {
                _timer.Interval = baseMs;
            }

            if (resetNow)
            {
                // nothing else required for WinForms Timer
            }
        }

        private void Start()
        {
            if (_timer.Enabled) return;

            ApplyTimerInterval(resetNow: true);
            _timer.Start();
            ShowBalloon("Started.");
        }

        private void Stop()
        {
            if (!_timer.Enabled) return;

            _timer.Stop();
            ShowBalloon("Stopped.");
        }

        private void OpenSettings()
        {
            if (_settingsForm != null && !_settingsForm.IsDisposed)
            {
                _settingsForm.Activate();
                _settingsForm.BringToFront();
                return;
            }

            _settingsForm = new SettingsForm(
                _cfg,
                isRunning: () => _timer.Enabled,
                start: Start,
                stop: Stop
            );

            _settingsForm.FormClosed += (_, __) => _settingsForm = null;

            var result = _settingsForm.ShowDialog();
            if (result != DialogResult.OK) return;

            _cfg.Seconds = _settingsForm.Seconds;
            _cfg.Pixels = _settingsForm.Pixels;

            _cfg.StartOnLaunch = _settingsForm.StartOnLaunch;

            _cfg.IdleAware = _settingsForm.IdleAware;
            _cfg.IdleThresholdSeconds = _settingsForm.IdleThresholdSeconds;

            _cfg.SafeMode = _settingsForm.SafeMode;
            _cfg.RandomJitterPercent = _settingsForm.RandomJitterPercent;

            AppConfig.Save(_cfg);

            ApplyTimerInterval(resetNow: true);

            ShowBalloon("Settings saved.");
        }

        private async Task OnTickAsync()
        {
            if (_isTickRunning) return;
            _isTickRunning = true;

            try
            {
                ApplyTimerInterval(resetNow: false);

                await JiggleOnceAsync(force: false);
            }
            finally
            {
                _isTickRunning = false;
            }
        }

        private async Task JiggleOnceAsync(bool force)
        {
            if (!force && _cfg.IdleAware)
            {
                int idleMs = GetIdleTimeMs();
                int thresholdMs = Math.Max(1000, _cfg.IdleThresholdSeconds * 1000);

                if (idleMs < thresholdMs)
                    return;
            }

            var p = Cursor.Position;
            int dx = 0, dy = 0;
            int distance = _cfg.Pixels;

            if (_cfg.SafeMode)
            {
                bool useY = _rng.Next(0, 2) == 0;
                int sign = _rng.Next(0, 2) == 0 ? -1 : 1;

                double factor = 0.7 + _rng.NextDouble() * 0.6;
                int dist = (int)Math.Round(distance * factor);
                dist = Math.Clamp(dist, 1, 200);

                if (useY) dy = sign * dist;
                else dx = sign * dist;
            }
            else
            {
                bool useY = (Environment.TickCount & 1) == 0;
                if (useY) dy = distance;
                else dx = distance;
            }

            Cursor.Position = new Point(p.X + dx, p.Y + dy);

            await Task.Delay(VisibleDelayMs);

            Cursor.Position = p;
        }

        private void Exit()
        {
            try { Stop(); } catch { }

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _timer.Dispose();

            Application.Exit();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private static int GetIdleTimeMs()
        {
            try
            {
                var lii = new LASTINPUTINFO();
                lii.cbSize = (uint)Marshal.SizeOf(lii);

                if (!GetLastInputInfo(ref lii))
                    return int.MaxValue;

                uint tickCount = (uint)Environment.TickCount;
                uint lastInputTick = lii.dwTime;

                uint idle = tickCount - lastInputTick;
                return (int)Math.Min(idle, int.MaxValue);
            }
            catch
            {
                return int.MaxValue;
            }
        }
    }
}
