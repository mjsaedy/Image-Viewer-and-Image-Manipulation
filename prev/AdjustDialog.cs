using System;
using System.Drawing;
using System.Windows.Forms;

public class ImageAdjustDialog : Form {

    public float Brightness { get; private set; } = 0f;  // -1 to +1
    public float Contrast { get; private set; } = 0f;    // -1 to +1
    public float Saturation { get; private set; } = 1f;  // 0 to 2

    private TrackBar tbBrightness, tbContrast, tbSaturation;
    private Label lbBrightness, lbContrast, lbSaturation;

    public event Action<float, float, float> ValuesChanged;

    public ImageAdjustDialog(float initialBrightness, float initialContrast, float initialSaturation)
    {
        this.Text = "Adjust Image";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.Manual;
        this.ClientSize = new Size(320, 280);
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        Brightness = initialBrightness;
        Contrast = initialContrast;
        Saturation = initialSaturation;

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
        currentTop += 25;
        
        tbBrightness = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)((initialBrightness + 1.0f) * 100),
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbBrightness.Scroll += (s, e) => { 
            Brightness = tbBrightness.Value / 100f - 1.0f; 
            lbBrightness.Text = $"Brightness: {Brightness:0.00}"; 
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        currentTop += verticalSpacing;

        // Contrast
        lbContrast = new Label() { 
            Text = $"Contrast: {Contrast:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };
        currentTop += 25;
        
        tbContrast = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)((initialContrast + 1.0f) * 100),
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbContrast.Scroll += (s, e) => { 
            Contrast = tbContrast.Value / 100f - 1.0f; 
            lbContrast.Text = $"Contrast: {Contrast:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        currentTop += verticalSpacing;

        // Saturation
        lbSaturation = new Label() { 
            Text = $"Saturation: {Saturation:0.00}", 
            Left = leftMargin, 
            Top = currentTop, 
            Width = labelWidth 
        };
        currentTop += 25;
        
        tbSaturation = new TrackBar() { 
            Minimum = 0,
            Maximum = 200,
            Value = (int)(initialSaturation * 100),
            Left = leftMargin, 
            Top = currentTop, 
            Width = trackbarWidth 
        };
        tbSaturation.Scroll += (s, e) => { 
            Saturation = tbSaturation.Value / 100f; 
            lbSaturation.Text = $"Saturation: {Saturation:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        currentTop += 50;

        // Buttons
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
            okButton, cancelButton 
        });
    }
}