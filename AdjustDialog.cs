using System;
using System.Drawing;
using System.Windows.Forms;

public class ImageAdjustDialog : Form {

    public float Brightness { get; private set; } = 0.0f;  // -1 to +1
    public float Contrast { get; private set; } = 0.0f;    // -1 to +1
    public float Saturation { get; private set; } = 1.0f;  // 0 to 2
    public float Gamma { get; private set; } = 1.0f; // (1 = no change, <1 lighter, >1 darker)


    private TrackBar tbBrightness, tbContrast, tbSaturation, tbGamma;
    private Label lbBrightness, lbContrast, lbSaturation, lbGamma;

    public event Action<float, float, float, float> ValuesChanged;

    public ImageAdjustDialog(float initialBrightness,
                             float initialContrast,
                             float initialSaturation,
                             float initialGamma)
    {
        this.Text = "Adjust Image";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.Manual;
        this.ClientSize = new Size(320, 380);
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        Brightness = initialBrightness;
        Contrast = initialContrast;
        Saturation = initialSaturation;
        Gamma = initialGamma;

        int labelWidth = 280;
        int trackbarWidth = 280;
        int leftMargin = 20;
        int verticalSpacing = 50;
        int currentTop = 20;

        // Brightness
        lbBrightness = new Label() { 
            Text = $"Brightness: {Brightness:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };
        currentTop += verticalSpacing / 2;
        
        tbBrightness = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)((initialBrightness + 1.0f) * 100),
            TickFrequency = 10,
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbBrightness.Scroll += (s, e) => { 
            Brightness = tbBrightness.Value / 100f - 1.0f; 
            lbBrightness.Text = $"Brightness: {Brightness:0.00}"; 
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation, Gamma);
        };

        currentTop += verticalSpacing;

        // Contrast
        lbContrast = new Label() { 
            Text = $"Contrast: {Contrast:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };
        currentTop += verticalSpacing / 2;
        
        tbContrast = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)((initialContrast + 1.0f) * 100),
            TickFrequency = 10,
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbContrast.Scroll += (s, e) => { 
            Contrast = tbContrast.Value / 100f - 1.0f; 
            lbContrast.Text = $"Contrast: {Contrast:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation, Gamma);
        };

        currentTop += verticalSpacing;

        // Saturation
        lbSaturation = new Label() { 
            Text = $"Saturation: {Saturation:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };
        currentTop += verticalSpacing / 2;
        
        tbSaturation = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)(initialSaturation * 100),
            TickFrequency = 10,
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbSaturation.Scroll += (s, e) => { 
            Saturation = tbSaturation.Value / 100f; 
            lbSaturation.Text = $"Saturation: {Saturation:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation, Gamma);
        };

        currentTop += verticalSpacing;
        
        // Gamma
        lbGamma = new Label() { 
            Text = $"Gamma: {Gamma:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };

        currentTop += verticalSpacing / 2;

        tbGamma = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)(initialGamma * 100),
            TickFrequency = 10,
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbGamma.Scroll += (s, e) => { 
            Gamma = tbGamma.Value / 100f; 
            lbGamma.Text = $"Gamma: {Gamma:0.00}"; 
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation, Gamma);
        };

        currentTop += (int)(verticalSpacing * 1.5);

        // Buttons - Reset, OK, Cancel
        var resetButton = new Button() { 
            Text = "Reset", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = 75 
        };
        resetButton.Click += ResetToDefaults;

        var okButton = new Button() { 
            Text = "OK", 
            DialogResult = DialogResult.OK, 
            Left = 150, 
            Top = currentTop, 
            Width = 75 
        };
        
        var cancelButton = new Button() { 
            Text = "Cancel", 
            DialogResult = DialogResult.Cancel, 
            Left = 235, 
            Top = currentTop, 
            Width = 75 
        };

        this.AcceptButton = okButton;
        this.CancelButton = cancelButton;

        this.Controls.AddRange(new Control[] { 
            lbBrightness, tbBrightness, 
            lbContrast, tbContrast, 
            lbSaturation, tbSaturation, 
            lbGamma, tbGamma, 
            resetButton, okButton, cancelButton 
        });
    }

    private void ResetToDefaults(object sender, EventArgs e)
    {
        // Reset to default values: Brightness=0, Contrast=0, Saturation=1
        Brightness = 0f;
        Contrast = 0f;
        Saturation = 1f;
        Gamma = 1f;

        // Update trackbar values
        tbBrightness.Value = (int)((Brightness + 1.0f) * 100);
        tbContrast.Value = (int)((Contrast + 1.0f) * 100);
        tbSaturation.Value = (int)(Saturation * 100);
        tbGamma.Value = (int)(Gamma * 100);

        // Update labels
        lbBrightness.Text = $"Brightness: {Brightness:0.00}";
        lbContrast.Text = $"Contrast: {Contrast:0.00}";
        lbSaturation.Text = $"Saturation: {Saturation:0.00}";
        lbGamma.Text = $"Gamma: {Gamma:0.00}";

        // Trigger the values changed event
        ValuesChanged?.Invoke(Brightness, Contrast, Saturation, Gamma);
    }
}