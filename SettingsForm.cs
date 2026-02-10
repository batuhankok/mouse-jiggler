using System;
using System.Windows.Forms;

namespace MouseJiggler
{
    public sealed class SettingsForm : Form
    {
        private readonly NumericUpDown _secondsInput;
        private readonly NumericUpDown _pixelsInput;
        private readonly CheckBox _startOnLaunch;

        private readonly Button _startBtn;
        private readonly Button _stopBtn;

        private readonly Func<bool> _isRunning;
        private readonly Action _start;
        private readonly Action _stop;

        public int Seconds => (int)_secondsInput.Value;
        public int Pixels  => (int)_pixelsInput.Value;
        public bool StartOnLaunch => _startOnLaunch.Checked;

        public SettingsForm(
            int seconds,
            int pixels,
            bool startOnLaunch,
            Func<bool> isRunning,
            Action start,
            Action stop
        )
        {
            _isRunning = isRunning;
            _start = start;
            _stop = stop;

            Text = "MouseJiggler Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 380;
            Height = 240;

            var secondsLabel = new Label { Left = 15, Top = 20, Width = 190, Text = "Move every (seconds):" };
            _secondsInput = new NumericUpDown
            {
                Left = 210, Top = 18, Width = 140,
                Minimum = 1, Maximum = 3600,
                Value = Math.Clamp(seconds, 1, 3600)
            };

            var pixelsLabel = new Label { Left = 15, Top = 55, Width = 190, Text = "Move distance (pixels):" };
            _pixelsInput = new NumericUpDown
            {
                Left = 210, Top = 53, Width = 140,
                Minimum = 1, Maximum = 200,
                Value = Math.Clamp(pixels, 1, 200)
            };

            _startOnLaunch = new CheckBox
            {
                Left = 15,
                Top = 90,
                Width = 260,
                Text = "Start automatically on launch",
                Checked = startOnLaunch
            };

            _startBtn = new Button { Text = "Start", Left = 15, Top = 125, Width = 80 };
            _stopBtn  = new Button { Text = "Stop", Left = 105, Top = 125, Width = 80 };

            _startBtn.Click += (_, __) => { _start(); RefreshRunButtons(); };
            _stopBtn.Click  += (_, __) => { _stop();  RefreshRunButtons(); };

            var okBtn = new Button { Text = "OK", Left = 190, Width = 75, Top = 165, DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", Left = 275, Width = 75, Top = 165, DialogResult = DialogResult.Cancel };

            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            Controls.AddRange(new Control[]
            {
                secondsLabel, _secondsInput,
                pixelsLabel, _pixelsInput,
                _startOnLaunch,
                _startBtn, _stopBtn,
                okBtn, cancelBtn
            });

            Shown += (_, __) => RefreshRunButtons();
        }

        private void RefreshRunButtons()
        {
            var running = _isRunning();
            _startBtn.Enabled = !running;
            _stopBtn.Enabled = running;
        }
    }
}
