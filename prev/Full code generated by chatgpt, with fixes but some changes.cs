using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

public class ImageViewer : Form
{
    private Bitmap image;
    private Bitmap originalImage; // for live adjustments
    private float zoom = 1f;
    private Point pan = Point.Empty;
    private Point mouseDown;
    private bool panning;
    private long jpegQuality = 90;
    private ToolStripMenuItem exitMenuItem;

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new ImageViewer());
    }

    public ImageViewer()
    {
        Text = "Image Viewer";
        DoubleBuffered = true;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Normal;
        Size = new Size(800, 600);

        // Menus
        var menu = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("File");
        var imageMenu = new ToolStripMenuItem("Image");

        var openMenuItem = new ToolStripMenuItem("Open", null, (s, e) => OpenImage());
        var saveAsMenuItem = new ToolStripMenuItem("Save As", null, (s, e) => SaveImageAs());
        var qualityMenuItem = new ToolStripMenuItem("Set Quality", null, (s, e) => SetQuality());

        exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        //exitMenuItem.ShortcutKeys = Keys.Escape;

        fileMenu.DropDownItems.Add(openMenuItem);
        fileMenu.DropDownItems.Add(saveAsMenuItem);
        fileMenu.DropDownItems.Add(qualityMenuItem);
        fileMenu.DropDownItems.Add(exitMenuItem);

        var resizeItem = new ToolStripMenuItem("Resize", null, (s, e) => ResizeImage());
        var mirrorItem = new ToolStripMenuItem("Mirror", null, (s, e) => MirrorImage());
        var brightItem = new ToolStripMenuItem("Adjust Brightness", null, (s, e) => AdjustBrightness());
        var contrastItem = new ToolStripMenuItem("Adjust Contrast", null, (s, e) => AdjustContrast());
        var satItem = new ToolStripMenuItem("Adjust Saturation", null, (s, e) => AdjustSaturation());
        var adjustItem = new ToolStripMenuItem("Adjust Image...", null, (s, e) => OpenAdjustDialog());

        imageMenu.DropDownItems.AddRange(new ToolStripItem[] { resizeItem, mirrorItem, brightItem, contrastItem, satItem, adjustItem });

        menu.Items.Add(fileMenu);
        menu.Items.Add(imageMenu);
        MainMenuStrip = menu;
        Controls.Add(menu);

        // Events
        MouseDown += Viewer_MouseDown;
        MouseUp += Viewer_MouseUp;
        MouseMove += Viewer_MouseMove;
        MouseWheel += Viewer_MouseWheel;
        FormClosing += (s, e) => SaveSettings();

        LoadSettings();
    }

    // --- File Operations ---
    private void OpenImage()
    {
        using (var dlg = new OpenFileDialog())
        {
            dlg.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.tif;*.tiff;*.webp|All files|*.*";
            dlg.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                image?.Dispose();
                originalImage?.Dispose();
                image = new Bitmap(dlg.FileName);
                originalImage = new Bitmap(image);
                zoom = 1f;
                pan = Point.Empty;
                Invalidate();
            }
        }
    }

    private void SaveImageAs()
    {
        if (image == null) return;
        using (var dlg = new SaveFileDialog())
        {
            dlg.Filter = "JPEG|*.jpg;*.jpeg|PNG|*.png|Bitmap|*.bmp|TIFF|*.tif";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    var encoder = GetEncoder(ImageFormat.Jpeg);
                    var ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);
                    image.Save(dlg.FileName, encoder, ep);
                }
                else if (ext == ".png") image.Save(dlg.FileName, ImageFormat.Png);
                else if (ext == ".bmp") image.Save(dlg.FileName, ImageFormat.Bmp);
                else if (ext == ".tif" || ext == ".tiff") image.Save(dlg.FileName, ImageFormat.Tiff);
                else image.Save(dlg.FileName);
            }
        }
    }

    private void SetQuality()
    {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter JPEG quality (1-100):", "Set Quality", jpegQuality.ToString());
        if (int.TryParse(input, out int q) && q >= 1 && q <= 100)
            jpegQuality = q;
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (var c in ImageCodecInfo.GetImageDecoders())
            if (c.FormatID == format.Guid) return c;
        return null;
    }

    // --- Panning & Zooming ---
    private void Viewer_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            panning = true;
            mouseDown = e.Location;
        }
    }

    private void Viewer_MouseUp(object sender, MouseEventArgs e) => panning = false;

    private void Viewer_MouseMove(object sender, MouseEventArgs e)
    {
        if (panning && image != null)
        {
            pan.X += e.X - mouseDown.X;
            pan.Y += e.Y - mouseDown.Y;
            mouseDown = e.Location;

            if (pan.X > 0) pan.X = 0;
            if (pan.Y > 0) pan.Y = 0;
            if (pan.X < Width - image.Width * zoom) pan.X = (int)(Width - image.Width * zoom);
            if (pan.Y < Height - image.Height * zoom - MainMenuStrip.Height) pan.Y = (int)(Height - image.Height * zoom - MainMenuStrip.Height);

            Invalidate();
        }
    }

    private void Viewer_MouseWheel(object sender, MouseEventArgs e)
    {
        if (image == null) return;
        float oldZoom = zoom;
        if (e.Delta > 0) zoom *= 1.1f;
        else zoom /= 1.1f;
        if (zoom < 0.1f) zoom = 0.1f;
        if (zoom > 10f) zoom = 10f;

        float mx = (e.X - pan.X) / oldZoom;
        float my = (e.Y - pan.Y) / oldZoom;
        pan.X = (int)(e.X - mx * zoom);
        pan.Y = (int)(e.Y - my * zoom);

        if (pan.X > 0) pan.X = 0;
        if (pan.Y > 0) pan.Y = 0;

        Invalidate();
    }

    // --- Drawing ---
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (image != null)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(image, new Rectangle(pan.X, pan.Y, (int)(image.Width * zoom), (int)(image.Height * zoom)));
            Text = $"Image Viewer - {image.Width}x{image.Height} @ {zoom * 100:0}%";
        }
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            exitMenuItem.PerformClick();
            return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    // --- Image Ops ---
    private void ResizeImage()
    {
        if (image == null) return;
        string input = Microsoft.VisualBasic.Interaction.InputBox("Enter scale factor:", "Resize", "1.0");
        if (!float.TryParse(input, out float scale)) return;

        int newW = (int)(image.Width * scale);
        int newH = (int)(image.Height * scale);

        Bitmap resized = new Bitmap(newW, newH);
        using (Graphics g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, newW, newH);
        }
        image.Dispose();
        originalImage.Dispose();
        image = resized;
        originalImage = new Bitmap(image);
        Invalidate();
    }

    private void MirrorImage()
    {
        if (image == null) return;
        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
        originalImage.Dispose();
        originalImage = new Bitmap(image);
        Invalidate();
    }

    private void AdjustBrightness()
    {
        if (image == null) return;
        string input = Microsoft.VisualBasic.Interaction.InputBox("Brightness factor (1=no change):", "Brightness", "1.0");
        if (!float.TryParse(input, out float factor)) return;
        ApplyAdjustments(factor, 1f, 1f);
    }

    private void AdjustContrast()
    {
        if (image == null) return;
        string input = Microsoft.VisualBasic.Interaction.InputBox("Contrast factor (1=no change):", "Contrast", "1.0");
        if (!float.TryParse(input, out float factor)) return;
        ApplyAdjustments(1f, factor, 1f);
    }

    private void AdjustSaturation()
    {
        if (image == null) return;
        string input = Microsoft.VisualBasic.Interaction.InputBox("Saturation factor (1=no change):", "Saturation", "1.0");
        if (!float.TryParse(input, out float factor)) return;
        ApplyAdjustments(1f, 1f, factor);
    }

    private void OpenAdjustDialog()
    {
        if (image == null) return;
        using (var dlg = new ImageAdjustDialog(1f, 1f, 1f))
        {
            Bitmap backup = new Bitmap(originalImage);

            dlg.ValuesChanged += (b, c, s) => ApplyAdjustmentsLive(b, c, s);
            var result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                originalImage.Dispose();
                originalImage = new Bitmap(image);
            }
            else
            {
                image.Dispose();
                image = new Bitmap(backup);
                Invalidate();
            }
            backup.Dispose();
        }
    }

    private void ApplyAdjustments(float brightness, float contrast, float saturation)
    {
        if (image == null) return;
        image.Dispose();
        image = ApplyColorMatrix(originalImage, brightness, contrast, saturation);
        Invalidate();
    }

    private void ApplyAdjustmentsLive(float brightness, float contrast, float saturation)
    {
        if (originalImage == null) return;
        image?.Dispose();
        image = ApplyColorMatrix(originalImage, brightness, contrast, saturation);
        Invalidate();
    }


private Bitmap ApplyColorMatrix(Bitmap src, float brightness, float contrast, float saturation)
{
    Bitmap adjusted = new Bitmap(src.Width, src.Height);
    using (Graphics g = Graphics.FromImage(adjusted))
    {
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

        float lumR = 0.3086f, lumG = 0.6094f, lumB = 0.0820f;

        float[][] cmArray = {
            new float[] { brightness * (contrast*(lumR*(1-saturation)+saturation)), brightness * (contrast*(lumR*(1-saturation))), brightness * (contrast*(lumR*(1-saturation))), 0, 0 },
            new float[] { brightness * (contrast*(lumG*(1-saturation))), brightness * (contrast*(lumG*(1-saturation)+saturation)), brightness * (contrast*(lumG*(1-saturation))), 0, 0 },
            new float[] { brightness * (contrast*(lumB*(1-saturation))), brightness * (contrast*(lumB*(1-saturation))), brightness * (contrast*(lumB*(1-saturation)+saturation)), 0, 0 },
            new float[] { 0, 0, 0, 1, 0 },
            new float[] { 0, 0, 0, 0, 1 }
        };

        var cm = new System.Drawing.Imaging.ColorMatrix(cmArray);
        var ia = new System.Drawing.Imaging.ImageAttributes();
        ia.SetColorMatrix(cm);

        g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height),
            0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
    }
    return adjusted;
}




    // --- Settings ---
    private string GetSettingsFile() => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");

    private void SaveSettings()
    {
        var s = new WindowSettings
        {
            X = Location.X,
            Y = Location.Y,
            Width = Size.Width,
            Height = Size.Height,
            WindowState = WindowState,
            JpegQuality = jpegQuality
        };
        try
        {
            var xs = new XmlSerializer(typeof(WindowSettings));
            using (var fs = new FileStream(GetSettingsFile(), FileMode.Create))
                xs.Serialize(fs, s);
        }
        catch { }
    }

    private void LoadSettings()
    {
        string file = GetSettingsFile();
        if (!File.Exists(file)) return;
        try
        {
            var xs = new XmlSerializer(typeof(WindowSettings));
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var s = (WindowSettings)xs.Deserialize(fs);
                StartPosition = FormStartPosition.Manual;
                Size = new Size(s.Width, s.Height);
                Location = new Point(s.X, s.Y);
                WindowState = s.WindowState;
                jpegQuality = s.JpegQuality;
            }
        }
        catch { }
    }
}

[Serializable]
public class WindowSettings
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public FormWindowState WindowState { get; set; }
    public long JpegQuality { get; set; }
}

public class ImageAdjustDialog : Form
{
    public float Brightness { get; private set; } = 1f;
    public float Contrast { get; private set; } = 1f;
    public float Saturation { get; private set; } = 1f;

    private TrackBar tbBrightness, tbContrast, tbSaturation;
    private Label lbBrightness, lbContrast, lbSaturation;

    public event Action<float, float, float> ValuesChanged;

    public ImageAdjustDialog(float initialBrightness, float initialContrast, float initialSaturation)
    {
        Text = "Adjust Image";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(300, 230);
        MaximizeBox = false;
        MinimizeBox = false;

        Brightness = initialBrightness;
        Contrast = initialContrast;
        Saturation = initialSaturation;

        lbBrightness = new Label() { Text = $"Brightness: {Brightness:0.00}", Left = 10, Top = 10, Width = 280 };
        tbBrightness = new TrackBar() { Minimum = 0, Maximum = 200, Value = (int)(Brightness * 100), Left = 10, Top = 30, Width = 280 };
        tbBrightness.Scroll += (s, e) => { Brightness = tbBrightness.Value / 100f; lbBrightness.Text = $"Brightness: {Brightness:0.00}"; ValuesChanged?.Invoke(Brightness, Contrast, Saturation); };

        lbContrast = new Label() { Text = $"Contrast: {Contrast:0.00}", Left = 10, Top = 70, Width = 280 };
        tbContrast = new TrackBar() { Minimum = 0, Maximum = 200, Value = (int)(Contrast * 100), Left = 10, Top = 90, Width = 280 };
        tbContrast.Scroll += (s, e) => { Contrast = tbContrast.Value / 100f; lbContrast.Text = $"Contrast: {Contrast:0.00}"; ValuesChanged?.Invoke(Brightness, Contrast, Saturation); };

        lbSaturation = new Label() { Text = $"Saturation: {Saturation:0.00}", Left = 10, Top = 130, Width = 280 };
        tbSaturation = new TrackBar() { Minimum = 0, Maximum = 200, Value = (int)(Saturation * 100), Left = 10, Top = 150, Width = 280 };
        tbSaturation.Scroll += (s, e) => { Saturation = tbSaturation.Value / 100f; lbSaturation.Text = $"Saturation: {Saturation:0.00}"; ValuesChanged?.Invoke(Brightness, Contrast, Saturation); };

        var okButton = new Button() { Text = "OK", DialogResult = DialogResult.OK, Left = 130, Top = 190, Width = 60 };
        var cancelButton = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 200, Top = 190, Width = 60 };

        AcceptButton = okButton;
        CancelButton = cancelButton;

        Controls.AddRange(new Control[] { lbBrightness, tbBrightness, lbContrast, tbContrast, lbSaturation, tbSaturation, okButton, cancelButton });
    }
}
