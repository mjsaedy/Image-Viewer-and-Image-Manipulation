// ImageViewer.cs
// Compile: csc /t:winexe /r:System.Drawing.dll,System.Windows.Forms.dll ImageViewer.cs
using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Drawing;
using System.Drawing.Drawing2D;

static class Program {

    [STAThread]
    static void Main(string[] args) {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form {

    public Viewer viewer;
    MenuStrip MainMenu;
    ToolStripMenuItem mnuFile, mnuFileOpen, mnuImageFit, mnuFileExit, mnuFileSaveAs, mnuFileQuality, mnuFileRevert,
                      mnuImageActualSize, mnuImageResize, mnuImageSaturation, mnuImageMirror,
                      mnuImageContrast, mnuImageBbrightness, mnuImageAdjust;
    
    private long _jpegQuality = 90; // default JPEG quality
    private string _fileName = null;
    
    public Bitmap originalImage; // store original for live updates


    public MainForm() {
        Text = "Image Viewer Pan & Zoom";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;

        mnuFile = new ToolStripMenuItem("File");
        //---------------------------------------
        mnuFileOpen = new ToolStripMenuItem("Open...", null, OnOpenClicked);
        mnuFileRevert = new ToolStripMenuItem("Revert", null, OnRevertClicked);
        mnuFileSaveAs = new ToolStripMenuItem("Save As...", null, (s, e) => SaveAsImage());
        mnuFileQuality = new ToolStripMenuItem("Set JPEG Quality", null, (s, e) => SetJpegQuality());
        mnuFileExit = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        mnuFile.DropDownItems.Add(mnuFileOpen);
        mnuFile.DropDownItems.Add(mnuFileSaveAs);
        mnuFile.DropDownItems.Add(mnuFileRevert);
        mnuFile.DropDownItems.Add(new ToolStripSeparator());
        mnuFile.DropDownItems.Add(mnuFileQuality);
        mnuFile.DropDownItems.Add(new ToolStripSeparator());
        mnuFile.DropDownItems.Add(mnuFileExit);

        
        var mnuImage = new ToolStripMenuItem("Image");
        //---------------------------------------
        mnuImageFit = new ToolStripMenuItem("Fit to window (F)", null, (s, e) => viewer.FitImage());
        mnuImageActualSize = new ToolStripMenuItem("Actual size (R)", null, (s, e) => viewer.ResetView());
        mnuImageResize = new ToolStripMenuItem("Resize", null, (s, e) => viewer.ScaleImage());
        mnuImageMirror = new ToolStripMenuItem("Mirror", null, (s, e) => viewer.MirrorImage());
        mnuImageBbrightness = new ToolStripMenuItem("Adjust Brightness", null,
                                                (s, e) => viewer.AdjustBrightness());
        mnuImageContrast = new ToolStripMenuItem("Adjust Contrast", null,
                                                (s, e) => viewer.AdjustContrast());
        mnuImageSaturation = new ToolStripMenuItem("Adjust Saturation", null,
                                                  (s, e) => viewer.AdjustSaturation());
        mnuImageAdjust = new ToolStripMenuItem("Adjust Image...", null, (s, e) => OpenAdjustDialog());

        mnuImage.DropDownItems.Add(mnuImageFit);
        mnuImage.DropDownItems.Add(mnuImageActualSize);
        mnuImage.DropDownItems.Add(new ToolStripSeparator());
        mnuImage.DropDownItems.Add(mnuImageResize);
        mnuImage.DropDownItems.Add(mnuImageMirror);
        mnuImage.DropDownItems.Add(mnuImageBbrightness);
        mnuImage.DropDownItems.Add(mnuImageContrast);
        mnuImage.DropDownItems.Add(mnuImageSaturation);
        mnuImage.DropDownItems.Add(new ToolStripSeparator());
        mnuImage.DropDownItems.Add(mnuImageAdjust);

        MainMenu = new MenuStrip();
        MainMenu.Items.Add(mnuFile);
        MainMenu.Items.Add(mnuImage);

        this.MainMenuStrip = MainMenu;
        this.Controls.Add(MainMenu);


        viewer = new Viewer(this);
        viewer.Dock = DockStyle.Fill;
        Controls.Add(viewer);

        MainMenu.BringToFront();

        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
        
        LoadSettings();
        this.FormClosing += (s, e) => SaveSettings();
        
    }

    void OnOpenClicked(object sender, EventArgs e) {
        using (var dlg = new OpenFileDialog()) {
            dlg.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.tif;*.tiff|All files|*.*";
            dlg.Title = "Open";
            if (dlg.ShowDialog(this) == DialogResult.OK) {
                viewer.LoadImage(dlg.FileName);
                originalImage = new Bitmap(viewer.image);
                _fileName = dlg.FileName;
            }
        }
    }

    void OnRevertClicked(object sender, EventArgs e) {
        if (originalImage == null)
            return;
        // Restore original image
        viewer.image.Dispose();
        viewer.image = new Bitmap(originalImage);
        viewer.Invalidate();
    }

    void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && IsImageFile(files[0]))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }
    }

    void MainForm_DragDrop(object sender, DragEventArgs e) {
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files != null && files.Length > 0 && IsImageFile(files[0])) {
            viewer.LoadImage(files[0]);
            originalImage = new Bitmap(viewer.image);
            _fileName = files[0];
        }
    }

    bool IsImageFile(string path) {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" ||
               ext == ".gif" || ext == ".tif" || ext == ".tiff";
    }

    void MainForm_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Escape)
            Close();

        if (e.KeyCode == Keys.R)
            viewer.ResetView();

        else if (e.KeyCode == Keys.F)
            viewer.FitImage();

        else if (e.KeyCode == Keys.O && e.Control)
            OnOpenClicked(null, EventArgs.Empty);
    }

    /*
    private static string InputBox(string prompt, string title, string defaultValue = "")
    {
        Form form = new Form();
        Label label = new Label();
        TextBox textBox = new TextBox();
        Button buttonOk = new Button();
        Button buttonCancel = new Button();

        form.Text = title;
        label.Text = prompt;
        textBox.Text = defaultValue;

        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        buttonOk.Text = "OK";
        buttonCancel.Text = "Cancel";
        buttonOk.DialogResult = DialogResult.OK;
        buttonCancel.DialogResult = DialogResult.Cancel;
        buttonOk.SetBounds(228, 72, 75, 23);
        buttonCancel.SetBounds(309, 72, 75, 23);

        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = buttonOk;
        form.CancelButton = buttonCancel;

        return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
    }
    */

    private void SetJpegQuality() {
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter JPEG quality (1–100):",
            "Set JPEG Quality",
            _jpegQuality.ToString()
        );
        if (int.TryParse(input, out int q) && q >= 1 && q <= 100)
        {
            _jpegQuality = q;
        }
        else if (!string.IsNullOrEmpty(input))
        {
            MessageBox.Show("Invalid quality value. Please enter a number between 1 and 100.");
        }
    }


    private void SaveAsImage() {
        if (viewer.image == null)
            return;

        using (SaveFileDialog dlg = new SaveFileDialog()) {
            dlg.Filter = "JPEG Image|*.jpg;*.jpeg|PNG Image|*.png|Bitmap Image|*.bmp|GIF Image|*.gif";
            dlg.Title = "Save Image As";
            if (string.IsNullOrEmpty(_fileName)) {
                dlg.FileName = "";
            } else {
                dlg.InitialDirectory = Path.GetDirectoryName(_fileName);
                dlg.FileName = AppendSuffixToFileName(Path.GetFileName(_fileName), " - copy");
            }
            if (dlg.ShowDialog() == DialogResult.OK) {
                var ext = System.IO.Path.GetExtension(dlg.FileName).ToLowerInvariant();
                System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Jpeg;
                if (ext == ".jpg" || ext == ".jpeg") {
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                }
                else if (ext == ".png")
                    format = System.Drawing.Imaging.ImageFormat.Png;
                else if (ext == ".bmp")
                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                else if (ext == ".gif")
                    format = System.Drawing.Imaging.ImageFormat.Gif;

                //viewer.image.Save(dlg.FileName, format);
                if (format == System.Drawing.Imaging.ImageFormat.Jpeg) {
                    var encoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                        .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
                    if (encoder != null) {
                        var encParams = new System.Drawing.Imaging.EncoderParameters(1);
                        encParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                            System.Drawing.Imaging.Encoder.Quality, _jpegQuality);
                        viewer.image.Save(dlg.FileName, encoder, encParams);
                    } else {
                        viewer.image.Save(dlg.FileName, format); // fallback
                    }
                }
                else {
                    viewer.image.Save(dlg.FileName, format);
                }
            }
        }
    }

    private string AppendSuffixToFileName(string filePath, string suffix) {
        string dir = Path.GetDirectoryName(filePath);
        string name = Path.GetFileNameWithoutExtension(filePath);
        string ext = Path.GetExtension(filePath);
        return Path.Combine(dir, name + suffix + ext);
    }


    #region Setting
    /*
    private string GetSettingsFile() {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyImageViewer");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "settings.xml");
    }
    */

    private string GetSettingsFile() {
        string folder = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(folder, "settings.xml");
    }

    private void SaveSettings() {
        var s = new WindowSettings
        {
            X = this.Location.X,
            Y = this.Location.Y,
            Width = this.Size.Width,
            Height = this.Size.Height,
            WindowState = this.WindowState,
            JpegQuality = _jpegQuality
        };

        try
        {
            var xs = new XmlSerializer(typeof(WindowSettings));
            using (var fs = new FileStream(GetSettingsFile(), FileMode.Create))
                xs.Serialize(fs, s);
        }
        catch { /* ignore errors */ }
    }

    private void LoadSettings() {
        string file = GetSettingsFile();
        if (!File.Exists(file)) return;

        try
        {
            var xs = new XmlSerializer(typeof(WindowSettings));
            using (var fs = new FileStream(file, FileMode.Open))
            {
                var s = (WindowSettings)xs.Deserialize(fs);
                this.StartPosition = FormStartPosition.Manual;
                this.Size = new Size(s.Width, s.Height);
                this.Location = new Point(s.X, s.Y);
                this.WindowState = s.WindowState;
                _jpegQuality = s.JpegQuality;
            }
        }
        catch { /* ignore errors */ }
    }
    #endregion


    /*
    private void OpenAdjustDialog() {
        using (var dlg = new ImageAdjustDialog(1f, 1f, 1f)) // pass current values if you want
        {
            if (dlg.ShowDialog() == DialogResult.OK) {
                viewer.ApplyAdjustments(dlg.Brightness, dlg.Contrast, dlg.Saturation);
            }
        }
    }
    */
    /*
    private void OpenAdjustDialog() {
        if (viewer.image == null)
            return;

        using var dlg = new ImageAdjustDialog(1f, 1f, 1f);
        // Hook live updates
        dlg.ValuesChanged += (b, c, s) => {
            viewer.ApplyAdjustmentsLive(originalImage, b, c, s);
        };

        var result = dlg.ShowDialog();

        // Commit adjustments
        originalImage.Dispose();
        originalImage = new Bitmap(viewer.image);
    }
    */
    private void OpenAdjustDialog() {
        if (viewer.image == null)
            return;

        using (var dlg = new ImageAdjustDialog(1f, 1f, 1f)) {
            // Store original image for live updates
            Bitmap backup = new Bitmap(originalImage);

            dlg.ValuesChanged += (b, c, s) => {
                viewer.ApplyAdjustmentsLive(backup, b, c, s);
            };

            var result = dlg.ShowDialog();

            if (result == DialogResult.OK) {
                // Commit changes
                //originalImage.Dispose();
                //originalImage = new Bitmap(viewer.image);
            } else {
                // Restore original image if canceled
                viewer.image.Dispose();
                viewer.image = new Bitmap(originalImage);
                viewer.Invalidate();
            }

            backup.Dispose();
        }
    }



}


public class Viewer : Control {

    public Image image;
    float scale = 1.0f;
    PointF offset = new PointF(0, 0);
    bool panning = false;
    Point lastMouse;
    const float MinScale = 0.01f;
    const float MaxScale = 100.0f;
    MainForm parent;

    public Viewer(MainForm parentForm)
    {
        parent = parentForm;
        DoubleBuffered = true;
        BackColor = Color.Black;
        this.SetStyle(ControlStyles.Selectable, true);
        this.TabStop = true;

        MouseDown += Viewer_MouseDown;
        MouseMove += Viewer_MouseMove;
        MouseUp += Viewer_MouseUp;
        MouseWheel += Viewer_MouseWheel;
        Resize += (s, e) => Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(BackColor);

        if (image == null)
        {
            using (var sf = new StringFormat())
            using (var font = new Font("Segoe UI", 10))
            using (var brush = new SolidBrush(Color.FromArgb(200, Color.White)))
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.DrawString("File → Open or drag an image here\nMouse wheel: zoom — Left drag: pan — R: reset — F: fit",
                    font, brush, ClientRectangle, sf);
            }
            parent.Text = "Image Viewer — Pan & Zoom";
            return;
        }

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.SmoothingMode = SmoothingMode.HighQuality;

        var oldTransform = g.Transform;
        var mat = new Matrix();
        mat.Translate(offset.X, offset.Y);
        mat.Scale(scale, scale);
        g.Transform = mat;

        g.DrawImage(image, 0, 0, image.Width, image.Height);
        g.Transform = oldTransform;

        float percent = scale * 100f;
        parent.Text = $"Image Viewer — {image.Width}x{image.Height} @ {percent:0.#}%";
    }

    void Viewer_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            panning = true;
            lastMouse = e.Location;
            Cursor = Cursors.Hand;
            Capture = true;
        }
        Focus();
    }

    void Viewer_MouseMove(object sender, MouseEventArgs e)
    {
        if (panning)
        {
            var dx = e.X - lastMouse.X;
            var dy = e.Y - lastMouse.Y;
            offset = new PointF(offset.X + dx, offset.Y + dy);
            lastMouse = e.Location;
            ClampOffset();
            Invalidate();
        }
    }

    void Viewer_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && panning)
        {
            panning = false;
            Cursor = Cursors.Default;
            Capture = false;
        }
    }

    void Viewer_MouseWheel(object sender, MouseEventArgs e)
    {
        if (image == null) return;

        float delta = e.Delta / 120f;
        float zoomFactor = (float)Math.Pow(1.2, delta);
        float newScale = scale * zoomFactor;
        if (newScale < MinScale) newScale = MinScale;
        if (newScale > MaxScale) newScale = MaxScale;
        zoomFactor = newScale / scale;

        var mouse = e.Location;
        var imagePointX = (mouse.X - offset.X) / scale;
        var imagePointY = (mouse.Y - offset.Y) / scale;

        offset = new PointF(
            mouse.X - imagePointX * newScale,
            mouse.Y - imagePointY * newScale
        );

        scale = newScale;
        ClampOffset();
        Invalidate();
    }

    void ClampOffset()
    {
        if (image == null) return;
        float imgW = image.Width * scale;
        float imgH = image.Height * scale;

        // Horizontal clamp
        if (imgW <= ClientSize.Width)
            offset.X = (ClientSize.Width - imgW) / 2f; // center if smaller
        else
        {
            if (offset.X > 0) offset.X = 0;
            if (offset.X + imgW < ClientSize.Width)
                offset.X = ClientSize.Width - imgW;
        }

        // Vertical clamp
        if (imgH <= ClientSize.Height)
            offset.Y = (ClientSize.Height - imgH) / 2f; // center if smaller
        else
        {
            if (offset.Y > 0) offset.Y = 0;
            if (offset.Y + imgH < ClientSize.Height)
                offset.Y = ClientSize.Height - imgH;
        }
    }

    public void LoadImage(string path) {
        try {
            var img = Image.FromFile(path);
            if (image != null)
                image.Dispose();
            image = img;
            FitImage();
        } catch (Exception ex) {
            MessageBox.Show("Failed to load image:\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ResetView() {
        scale = 1.0f;
        offset = new PointF(0, 0);
        ClampOffset();
        Invalidate();
    }

    public void FitImage() {
        if (image == null) return;
        var client = ClientSize;
        if (client.Width <= 0 || client.Height <= 0) return;

        float sx = (float)client.Width / image.Width;
        float sy = (float)client.Height / image.Height;
        scale = Math.Min(sx, sy);
        float imgW = image.Width * scale;
        float imgH = image.Height * scale;
        offset = new PointF((client.Width - imgW) / 2f, (client.Height - imgH) / 2f);
        Invalidate();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        
        if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            ZoomBy(1.2f, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        
        else if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            ZoomBy(1.0f / 1.2f, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        
        else if (e.KeyCode == Keys.R)
            ResetView();
        
        else if (e.KeyCode == Keys.F)
            FitImage();
    }

    void ZoomBy(float factor, Point center) {
        if (image == null) return;
        float newScale = scale * factor;
        if (newScale < MinScale) newScale = MinScale;
        if (newScale > MaxScale) newScale = MaxScale;
        factor = newScale / scale;

        var wx = (center.X - offset.X) / scale;
        var wy = (center.Y - offset.Y) / scale;

        offset = new PointF(
            center.X - wx * newScale,
            center.Y - wy * newScale
        );
        scale = newScale;
        ClampOffset();
        Invalidate();
    }

    protected override void Dispose(bool disposing) {
        if (disposing && image != null) {
            image.Dispose();
            image = null;
        }
        base.Dispose(disposing);
    }

    #region Image Manipulation
    public void ScaleImage() {
        if (image == null) return;
        string input = Interaction.InputBox("Enter resize scale (e.g., 2 for double size):", "Resize", "1.0");
        if (double.TryParse(input, out double scale) && scale > 0)
        {
            int newW = (int)(image.Width * scale);
            int newH = (int)(image.Height * scale);
            Bitmap resized = new Bitmap(newW, newH);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.DrawImage(image, new Rectangle(0, 0, newW, newH), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            }
            image.Dispose();
            image = resized;
            Invalidate();
        }
    }

    public void MirrorImage() {
        if (image == null) return;
        Bitmap mirrored = new Bitmap(image.Width, image.Height);
        using (Graphics g = Graphics.FromImage(mirrored))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                image.Width, 0, -image.Width, image.Height, GraphicsUnit.Pixel);
        }
        image.Dispose();
        image = mirrored;
        Invalidate();
    }

    public void AdjustSaturation() {
        if (image == null) return;
        string input = Interaction.InputBox("Enter saturation factor (1 = no change, 0 = grayscale, >1 = more saturated):", "Adjust Saturation", "1.0");
        if (float.TryParse(input, out float factor))
        {
            Bitmap adjusted = new Bitmap(image.Width, image.Height);
            using (Graphics g = Graphics.FromImage(adjusted))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                float lumR = 0.3086f;
                float lumG = 0.6094f;
                float lumB = 0.0820f;
                float[][] ptsArray = {
                    new float[] {lumR*(1-factor)+factor, lumR*(1-factor), lumR*(1-factor), 0, 0},
                    new float[] {lumG*(1-factor), lumG*(1-factor)+factor, lumG*(1-factor), 0, 0},
                    new float[] {lumB*(1-factor), lumB*(1-factor), lumB*(1-factor)+factor, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                };
                var cm = new System.Drawing.Imaging.ColorMatrix(ptsArray);
                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);

                g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
            }
            image.Dispose();
            image = adjusted;
            Invalidate();
        }
    }
    
    public void AdjustBrightness() {
        if (image == null)
            return;

        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter brightness factor (1 = no change, <1 darker, >1 brighter):",
            "Adjust Brightness",
            "1.0"
        );

        if (!float.TryParse(input, out float factor))
            return;

        Bitmap adjusted = new Bitmap(image.Width, image.Height);
        using (Graphics g = Graphics.FromImage(adjusted))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            float[][] ptsArray = {
                new float[] {factor, 0, 0, 0, 0},
                new float[] {0, factor, 0, 0, 0},
                new float[] {0, 0, factor, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            };

            var cm = new System.Drawing.Imaging.ColorMatrix(ptsArray);
            var ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(cm);

            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
        }

        image.Dispose();
        image = adjusted;
        Invalidate();
    }

    public void AdjustContrast() {
        if (image == null)
            return;

        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter contrast factor (1 = no change, <1 less contrast, >1 more contrast):",
            "Adjust Contrast",
            "1.0"
        );

        if (!float.TryParse(input, out float factor))
            return;

        Bitmap adjusted = new Bitmap(image.Width, image.Height);
        using (Graphics g = Graphics.FromImage(adjusted))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            float t = 0.5f * (1 - factor); // translate so 0.5 gray stays center
            float[][] ptsArray = {
                new float[] {factor, 0, 0, 0, t},
                new float[] {0, factor, 0, 0, t},
                new float[] {0, 0, factor, 0, t},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            };

            var cm = new System.Drawing.Imaging.ColorMatrix(ptsArray);
            var ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetColorMatrix(cm);

            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
        }

        image.Dispose();
        image = adjusted;
        Invalidate();
    }
    
    public void ApplyAdjustments(float brightness, float contrast, float saturation) {
        if (image == null) return;

        Bitmap adjusted = new Bitmap(image.Width, image.Height);
        using (Graphics g = Graphics.FromImage(adjusted))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            //float t = 0.5f * (1 - contrast);
            float lumR = 0.3086f;
            float lumG = 0.6094f;
            float lumB = 0.0820f;

            /*
            float[][] cmArray = {
                new float[] { contrast * (lumR*(1-saturation)+saturation), contrast * (lumR*(1-saturation)), contrast * (lumR*(1-saturation)), 0, t + brightness-1 },
                new float[] { contrast * (lumG*(1-saturation)), contrast * (lumG*(1-saturation)+saturation), contrast * (lumG*(1-saturation)), 0, t + brightness-1 },
                new float[] { contrast * (lumB*(1-saturation)), contrast * (lumB*(1-saturation)), contrast * (lumB*(1-saturation)+saturation), 0, t + brightness-1 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };
            */

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

            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
        }

        image.Dispose();
        image = adjusted;
        Invalidate();
    }

    public void ApplyAdjustmentsLive(Bitmap originalImage, float brightness, float contrast, float saturation) {
        if (originalImage == null)
            return;

        if (image != null)
            image.Dispose();

        image = new Bitmap(originalImage.Width, originalImage.Height);
        using (Graphics g = Graphics.FromImage(image)) {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            //float t = 0.5f * (1 - contrast);
            float lumR = 0.3086f;
            float lumG = 0.6094f;
            float lumB = 0.0820f;

            /*
            float[][] cmArray = {
                new float[] { contrast * (lumR*(1-saturation)+saturation), contrast * (lumR*(1-saturation)), contrast * (lumR*(1-saturation)), 0, t + brightness-1 },
                new float[] { contrast * (lumG*(1-saturation)), contrast * (lumG*(1-saturation)+saturation), contrast * (lumG*(1-saturation)), 0, t + brightness-1 },
                new float[] { contrast * (lumB*(1-saturation)), contrast * (lumB*(1-saturation)), contrast * (lumB*(1-saturation)+saturation), 0, t + brightness-1 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            };
            */

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

            g.DrawImage(originalImage, new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, ia);
        }

        Invalidate();
    }

    
    #endregion

}


public class WindowSettings {
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public FormWindowState WindowState { get; set; }
    public long JpegQuality { get; set; }
}



