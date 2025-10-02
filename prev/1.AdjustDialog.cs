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

    //public int DialogTop => this.Top;
    //public int DialogLeft => this.Left;

    public ImageAdjustDialog(float initialBrightness, float initialContrast, float initialSaturation)
    {
        this.Text = "Adjust Image";
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.Manual;
        this.ClientSize = new Size(300, 250);
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        Brightness = initialBrightness;
        Contrast = initialContrast;
        Saturation = initialSaturation;

        // Brightness
        lbBrightness = new Label() { Text = $"Brightness: {Brightness:0.00}", Left = 10, Top = 10, Width = 280 };
        tbBrightness = new TrackBar() { Minimum = 0,
                                        Maximum = 200,
                                        Value = (int)((initialBrightness + 1.0f) * 100),
                                        Left = 10, Top = 30, Width = 280 };
        //tbBrightness.Scroll += (s, e) => { Brightness = tbBrightness.Value / 100f; lbBrightness.Text = $"Brightness: {Brightness:0.00}"; };
        tbBrightness.Scroll += (s, e) => { 
            Brightness = tbBrightness.Value / 100f - 1.0f; 
            lbBrightness.Text = $"Brightness: {Brightness:0.00}"; 
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        // Contrast
        lbContrast = new Label() { Text = $"Contrast: {Contrast:0.00}", Left = 10, Top = 70, Width = 280 };
        tbContrast = new TrackBar() { Minimum = 0,
                                      Maximum = 200,
                                      Value = (int)((initialContrast + 1.0f) * 100),
                                      Left = 10, Top = 90, Width = 280 };
        //tbContrast.Scroll += (s, e) => { Contrast = tbContrast.Value / 100f; lbContrast.Text = $"Contrast: {Contrast:0.00}"; };
        tbContrast.Scroll += (s, e) => { 
            Contrast = tbContrast.Value / 100f - 1.0f; 
            lbContrast.Text = $"Contrast: {Contrast:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        // Saturation
        lbSaturation = new Label() { Text = $"Saturation: {Saturation:0.00}", Left = 10, Top = 120, Width = 280 };
        tbSaturation = new TrackBar() { Minimum = 0,
                                        Maximum = 200,
                                        Value = (int)(initialSaturation * 100),
                                        Left = 10, Top = 140, Width = 280 };
        //tbSaturation.Scroll += (s, e) => { Saturation = tbSaturation.Value / 100f; lbSaturation.Text = $"Saturation: {Saturation:0.00}"; };
        tbSaturation.Scroll += (s, e) => { 
            Saturation = tbSaturation.Value / 100f; 
            lbSaturation.Text = $"Saturation: {Saturation:0.00}";
            ValuesChanged?.Invoke(Brightness, Contrast, Saturation);
        };

        var okButton = new Button() { Text = "OK", DialogResult = DialogResult.OK, Left = 130, Top = 200, Width = 60 };
        this.AcceptButton = okButton;

        var cancelButton = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 160, Top = 200, Width = 60 };
        this.CancelButton = cancelButton; // pressing ESC will trigger Cancel

        this.Controls.AddRange(new Control[] { lbBrightness, tbBrightness, lbContrast, tbContrast, lbSaturation, tbSaturation, okButton, cancelButton });
        //this.Controls.Add(cancelButton);
    }
}
