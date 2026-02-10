using System;
using System.Windows.Forms;

namespace MouseJigglerTray
{
    public sealed class SettingsForm : Form
    {
        private readonly NumericUpDown _secondsInput;
        private readonly NumericUpDown _pixelsInput;

        public int Seconds => (int)_secondsInput.Value;
        public int Pixels  => (int)_pixelsInput.Value;

        public SettingsForm(int seconds, int pixels)
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Width = 320;
            Height = 180;

            var secondsLabel = new Label { Left = 15, Top = 20, Width = 150, Text = "Move every (seconds):" };
            _secondsInput = new NumericUpDown
            {
                Left = 175, Top = 18, Width = 110,
                Minimum = 1, Maximum = 3600,
                Value = Math.Clamp(seconds, 1, 3600)
            };

            var pixelsLabel = new Label { Left = 15, Top = 55, Width = 150, Text = "Move distance (pixels):" };
            _pixelsInput = new NumericUpDown
            {
                Left = 175, Top = 53, Width = 110,
                Minimum = 1, Maximum = 200,
                Value = Math.Clamp(pixels, 1, 200)
            };

            var okBtn = new Button { Text = "OK", Left = 120, Width = 80, Top = 95, DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", Left = 205, Width = 80, Top = 95, DialogResult = DialogResult.Cancel };

            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            Controls.AddRange(new Control[]
            {
                secondsLabel, _secondsInput,
                pixelsLabel, _pixelsInput,
                okBtn, cancelBtn
            });
        }
    }
}
