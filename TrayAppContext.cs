using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseJiggler
{
    public sealed class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly System.Windows.Forms.Timer _timer;

        private readonly ToolStripMenuItem _startItem;
        private readonly ToolStripMenuItem _stopItem;

        private int _seconds;
        private int _pixels;

        private const int VisibleDelayMs = 80;

        private bool _useY = false;
        private bool _isTickRunning = false;

        public TrayAppContext()
        {
            var cfg = AppConfig.Load();
            _seconds = cfg.Seconds <= 0 ? 30 : cfg.Seconds;
            _pixels  = cfg.Pixels  <= 0 ? 2  : cfg.Pixels;

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += async (_, __) => await OnTickAsync();
            ApplyTimerInterval();

            _startItem = new ToolStripMenuItem("Start", null, (_, __) => Start());
            _stopItem  = new ToolStripMenuItem("Stop",  null, (_, __) => Stop()) { Enabled = false };
            var settingsItem = new ToolStripMenuItem("Settings...", null, (_, __) => OpenSettings());
            var jiggleNowItem = new ToolStripMenuItem("Jiggle now", null, async (_, __) => await JiggleOnceAsync());
            var exitItem = new ToolStripMenuItem("Exit", null, (_, __) => Exit());

            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[]
            {
                _startItem,
                _stopItem,
                new ToolStripSeparator(),
                settingsItem,
                jiggleNowItem,
                new ToolStripSeparator(),
                exitItem
            });

            var trayIcon = LoadTrayIcon();

            _trayIcon = new NotifyIcon
            {
                Text = "MouseJiggler",
                Icon = trayIcon,
                ContextMenuStrip = menu,
                Visible = true
            };

            _trayIcon.DoubleClick += (_, __) =>
            {
                if (_timer.Enabled) Stop();
                else Start();
            };

            ShowBalloon($"Ready. Every {_seconds}s, move {_pixels}px (drift=0).");
        }

        private Icon LoadTrayIcon()
        {
            try
            {
                var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "mouse.ico");
                if (File.Exists(iconPath))
                    return new Icon(iconPath);
            }
            catch { /* ignore */ }

            return SystemIcons.Application;
        }

        private void ApplyTimerInterval()
        {
            var ms = Math.Max(1000, _seconds * 1000);
            _timer.Interval = ms;
        }

        private void Start()
        {
            if (_timer.Enabled) return;

            ApplyTimerInterval();
            _timer.Start();

            _startItem.Enabled = false;
            _stopItem.Enabled = true;

            ShowBalloon("Started.");
        }

        private void Stop()
        {
            if (!_timer.Enabled) return;

            _timer.Stop();

            _startItem.Enabled = true;
            _stopItem.Enabled = false;

            ShowBalloon("Stopped.");
        }

        private void OpenSettings()
        {
            using var dlg = new SettingsForm(_seconds, _pixels);

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            _seconds = dlg.Seconds;
            _pixels  = dlg.Pixels;

            AppConfig.Save(new AppConfig { Seconds = _seconds, Pixels = _pixels });

            ApplyTimerInterval();
            ShowBalloon($"Saved. Every {_seconds}s, move {_pixels}px (drift=0).");
        }

        private void Exit()
        {
            Stop();
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _timer.Dispose();
            Application.Exit();
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

        private async Task OnTickAsync()
        {
            if (_isTickRunning) return;
            _isTickRunning = true;

            try
            {
                await JiggleOnceAsync();
            }
            finally
            {
                _isTickRunning = false;
            }
        }

        private async Task JiggleOnceAsync()
        {
            var p = Cursor.Position;

            if (_useY)
                Cursor.Position = new Point(p.X, p.Y + _pixels);
            else
                Cursor.Position = new Point(p.X + _pixels, p.Y);

            await Task.Delay(VisibleDelayMs);

            Cursor.Position = p;
            _useY = !_useY;
        }
    }
}
