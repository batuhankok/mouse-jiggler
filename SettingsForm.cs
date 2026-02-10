using System;
using System.Windows.Forms;

namespace MouseJiggler
{
    public sealed class SettingsForm : Form
    {
        private readonly NumericUpDown _seconds;
        private readonly NumericUpDown _pixels;

        private readonly CheckBox _startOnLaunch;

        private readonly CheckBox _idleAware;
        private readonly NumericUpDown _idleThresholdSeconds;

        private readonly CheckBox _safeMode;
        private readonly NumericUpDown _jitterPercent;

        private readonly Func<bool> _isRunning;
        private readonly Action _start;
        private readonly Action _stop;

        private readonly Button _startBtn;
        private readonly Button _stopBtn;

        public int Seconds => (int)_seconds.Value;
        public int Pixels => (int)_pixels.Value;

        public bool StartOnLaunch => _startOnLaunch.Checked;

        public bool IdleAware => _idleAware.Checked;
        public int IdleThresholdSeconds => (int)_idleThresholdSeconds.Value;

        public bool SafeMode => _safeMode.Checked;
        public int RandomJitterPercent => (int)_jitterPercent.Value;

        public SettingsForm(
            AppConfig cfg,
            Func<bool> isRunning,
            Action start,
            Action stop
        )
        {
            _isRunning = isRunning;
            _start = start;
            _stop = stop;

            Text = "MouseJiggler Settings";
            Width = 450;
            Height = 360;
            TopMost = true;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            Shown += (_, __) =>
            {
                Activate();
                BringToFront();
                RefreshRunButtons();
            };

            var y = 18;

            var l1 = new Label { Text = "Interval (seconds):", Left = 15, Top = y + 3, Width = 200 };
            _seconds = new NumericUpDown { Left = 230, Top = y, Width = 180, Minimum = 1, Maximum = 3600, Value = cfg.Seconds };
            y += 35;

            var l2 = new Label { Text = "Distance (pixels):", Left = 15, Top = y + 3, Width = 200 };
            _pixels = new NumericUpDown { Left = 230, Top = y, Width = 180, Minimum = 1, Maximum = 200, Value = cfg.Pixels };
            y += 40;

            _startOnLaunch = new CheckBox
            {
                Text = "Start automatically on launch",
                Left = 15,
                Top = y,
                Width = 300,
                Checked = cfg.StartOnLaunch
            };
            y += 40;

            _idleAware = new CheckBox
            {
                Text = "Idle-aware mode (only jiggle when user is idle)",
                Left = 15,
                Top = y,
                Width = 420,
                Checked = cfg.IdleAware
            };
            y += 35;

            var l3 = new Label { Text = "Idle threshold (seconds):", Left = 35, Top = y + 3, Width = 200 };
            _idleThresholdSeconds = new NumericUpDown
            {
                Left = 230,
                Top = y,
                Width = 180,
                Minimum = 1,
                Maximum = 3600,
                Value = Math.Clamp(cfg.IdleThresholdSeconds, 1, 3600)
            };
            y += 40;

            _safeMode = new CheckBox
            {
                Text = "Randomized safe mode (jitter interval & direction)",
                Left = 15,
                Top = y,
                Width = 420,
                Checked = cfg.SafeMode
            };
            y += 35;

            var l4 = new Label { Text = "Interval jitter (%):", Left = 35, Top = y + 3, Width = 200 };
            _jitterPercent = new NumericUpDown
            {
                Left = 230,
                Top = y,
                Width = 180,
                Minimum = 0,
                Maximum = 80,
                Value = Math.Clamp(cfg.RandomJitterPercent, 0, 80)
            };
            y += 45;

            _startBtn = new Button { Text = "Start", Left = 15, Top = y, Width = 90 };
            _stopBtn  = new Button { Text = "Stop", Left = 115, Top = y, Width = 90 };

            _startBtn.Click += (_, __) => { _start(); RefreshRunButtons(); };
            _stopBtn.Click  += (_, __) => { _stop();  RefreshRunButtons(); };

            var okBtn = new Button { Text = "OK", Left = 250, Top = y, Width = 75, DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", Left = 335, Top = y, Width = 75, DialogResult = DialogResult.Cancel };

            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            Controls.AddRange(new Control[]
            {
                l1, _seconds,
                l2, _pixels,
                _startOnLaunch,
                _idleAware,
                l3, _idleThresholdSeconds,
                _safeMode,
                l4, _jitterPercent,
                _startBtn, _stopBtn,
                okBtn, cancelBtn
            });

            _idleAware.CheckedChanged += (_, __) => _idleThresholdSeconds.Enabled = _idleAware.Checked;
            _safeMode.CheckedChanged += (_, __) => _jitterPercent.Enabled = _safeMode.Checked;

            _idleThresholdSeconds.Enabled = _idleAware.Checked;
            _jitterPercent.Enabled = _safeMode.Checked;
        }

        private void RefreshRunButtons()
        {
            var running = _isRunning();
            _startBtn.Enabled = !running;
            _stopBtn.Enabled = running;
        }
    }
}
