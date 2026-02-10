using System;
using System.Drawing;
using System.Windows.Forms;

namespace MouseJigglerTray
{
    public sealed class TrayAppContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ToolStripMenuItem _startItem;
        private readonly ToolStripMenuItem _stopItem;
        private readonly System.Windows.Forms.Timer _timer;

        private int _seconds;
        private int _pixels;

        // drift=0 ama biraz "insansı" olsun diye X/Y dönüşümlü
        private bool _useY = false;

        public TrayAppContext()
        {
            var cfg = AppConfig.Load();
            _seconds = cfg.Seconds <= 0 ? 30 : cfg.Seconds;
            _pixels = cfg.Pixels <= 0 ? 2 : cfg.Pixels;

            _timer = new System.Windows.Forms.Timer();
            _timer.Tick += (_, __) => JiggleOnce();
            ApplyTimerInterval();

            _startItem = new ToolStripMenuItem("Start", null, (_, __) => Start());
            _stopItem  = new ToolStripMenuItem("Stop",  null, (_, __) => Stop()) { Enabled = false };
            var settingsItem = new ToolStripMenuItem("Settings...", null, (_, __) => OpenSettings());
            var exitItem = new ToolStripMenuItem("Exit", null, (_, __) => Exit());

            var menu = new ContextMenuStrip();
            menu.Items.AddRange(new ToolStripItem[]
            {
                _startItem,
                _stopItem,
                new ToolStripSeparator(),
                settingsItem,
                new ToolStripSeparator(),
                exitItem
            });

            _trayIcon = new NotifyIcon
            {
                Text = "Mouse Jiggler (Tray)",
                Icon = SystemIcons.Application,
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
            if (dlg.ShowDialog() != DialogResult.OK) return;

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
                _trayIcon.BalloonTipTitle = "Mouse Jiggler";
                _trayIcon.BalloonTipText = message;
                _trayIcon.ShowBalloonTip(2000);
            }
            catch { }
        }

        // DRIFT = 0: hareket ettir ve eski konuma geri dön
        private void JiggleOnce()
        {
            var p = Cursor.Position;

            if (_useY)
                Cursor.Position = new Point(p.X, p.Y + _pixels);
            else
                Cursor.Position = new Point(p.X + _pixels, p.Y);

            Cursor.Position = p; // drift=0 garanti
            _useY = !_useY;
        }
    }
}
