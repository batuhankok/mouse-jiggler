using System;
using System.Drawing;
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
            Width = 520;
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

            // ---- Main layout ----
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(14),
                ColumnCount = 1,
                RowCount = 3,
            };
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            int row = 0;
            void AddRow(Control left, Control right)
            {
                grid.RowCount = row + 1;
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                left.Margin = new Padding(0, 6, 10, 6);
                left.Anchor = AnchorStyles.Left;
                right.Margin = new Padding(0, 6, 0, 6);
                right.Anchor = AnchorStyles.Left;

                grid.Controls.Add(left, 0, row);
                grid.Controls.Add(right, 1, row);
                row++;
            }

            void AddFullRow(Control c)
            {
                grid.RowCount = row + 1;
                grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                c.Margin = new Padding(0, 10, 0, 6);
                c.Anchor = AnchorStyles.Left;
                grid.Controls.Add(c, 0, row);
                grid.SetColumnSpan(c, 2);
                row++;
            }

            var intervalLabel = new Label { Text = "Interval (seconds):", AutoSize = true };
            _seconds = new NumericUpDown
            {
                Width = 160,
                Minimum = 1,
                Maximum = 3600,
                Value = Math.Clamp(cfg.Seconds, 1, 3600)
            };
            AddRow(intervalLabel, _seconds);

            var distanceLabel = new Label { Text = "Distance (pixels):", AutoSize = true };
            _pixels = new NumericUpDown
            {
                Width = 160,
                Minimum = 1,
                Maximum = 200,
                Value = Math.Clamp(cfg.Pixels, 1, 200)
            };
            AddRow(distanceLabel, _pixels);

            _startOnLaunch = new CheckBox
            {
                Text = "Start automatically on launch",
                AutoSize = true,
                Checked = cfg.StartOnLaunch
            };
            AddFullRow(_startOnLaunch);

            _idleAware = new CheckBox
            {
                Text = "Idle-aware mode (only jiggle when user is idle)",
                AutoSize = true,
                Checked = cfg.IdleAware
            };
            AddFullRow(_idleAware);

            var idlePanel = new Panel { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(18, 0, 0, 0) };
            var idleInner = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };
            idleInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            idleInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            var idleLabel = new Label { Text = "Idle threshold (seconds):", AutoSize = true, Margin = new Padding(0, 6, 10, 6) };
            _idleThresholdSeconds = new NumericUpDown
            {
                Width = 160,
                Minimum = 1,
                Maximum = 3600,
                Value = Math.Clamp(cfg.IdleThresholdSeconds, 1, 3600),
                Margin = new Padding(0, 6, 0, 6)
            };

            idleInner.Controls.Add(idleLabel, 0, 0);
            idleInner.Controls.Add(_idleThresholdSeconds, 1, 0);
            idlePanel.Controls.Add(idleInner);

            AddFullRow(idlePanel);

            _safeMode = new CheckBox
            {
                Text = "Randomized safe mode (jitter interval & direction)",
                AutoSize = true,
                Checked = cfg.SafeMode
            };
            AddFullRow(_safeMode);

            var jitterPanel = new Panel { AutoSize = true, Dock = DockStyle.Fill, Padding = new Padding(18, 0, 0, 0) };
            var jitterInner = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 2,
                Dock = DockStyle.Fill
            };
            jitterInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            jitterInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));

            var jitterLabel = new Label { Text = "Interval jitter (%):", AutoSize = true, Margin = new Padding(0, 6, 10, 6) };
            _jitterPercent = new NumericUpDown
            {
                Width = 160,
                Minimum = 0,
                Maximum = 80,
                Value = Math.Clamp(cfg.RandomJitterPercent, 0, 80),
                Margin = new Padding(0, 6, 0, 6)
            };

            jitterInner.Controls.Add(jitterLabel, 0, 0);
            jitterInner.Controls.Add(_jitterPercent, 1, 0);
            jitterPanel.Controls.Add(jitterInner);

            AddFullRow(jitterPanel);

            _idleAware.CheckedChanged += (_, __) => _idleThresholdSeconds.Enabled = _idleAware.Checked;
            _safeMode.CheckedChanged += (_, __) => _jitterPercent.Enabled = _safeMode.Checked;

            _idleThresholdSeconds.Enabled = _idleAware.Checked;
            _jitterPercent.Enabled = _safeMode.Checked;

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(0),
            };

            _startBtn = new Button { Text = "Start", Width = 90, Height = 30 };
            _stopBtn  = new Button { Text = "Stop", Width = 90, Height = 30 };
            var spacer = new Panel { Width = 18, Height = 1 };

            var okBtn = new Button { Text = "OK", Width = 90, Height = 30, DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", Width = 90, Height = 30, DialogResult = DialogResult.Cancel };

            _startBtn.Click += (_, __) => { _start(); RefreshRunButtons(); };
            _stopBtn.Click  += (_, __) => { _stop();  RefreshRunButtons(); };

            okBtn.Click += (_, __) =>
            {
                _startBtn.Enabled = false;
                _stopBtn.Enabled = false;
                okBtn.Enabled = false;
            };

            buttons.Controls.Add(_startBtn);
            buttons.Controls.Add(_stopBtn);
            buttons.Controls.Add(spacer);
            buttons.Controls.Add(okBtn);
            buttons.Controls.Add(cancelBtn);

            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            root.Controls.Add(grid, 0, 0);

            var sep = new Label
            {
                BorderStyle = BorderStyle.Fixed3D,
                Height = 2,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 10, 0, 10)
            };
            root.Controls.Add(sep, 0, 1);

            root.Controls.Add(buttons, 0, 2);

            Controls.Add(root);
        }

        private void RefreshRunButtons()
        {
            var running = _isRunning();
            _startBtn.Enabled = !running;
            _stopBtn.Enabled = running;
        }
    }
}
