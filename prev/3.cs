// ImageViewer.cs
// Compile: csc /t:winexe /r:System.Drawing.dll,System.Windows.Forms.dll ImageViewer.cs
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

static class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}

public class MainForm : Form
{
    public Viewer viewer;
    ToolStripMenuItem openMenuItem;
    ToolStripMenuItem fitMenuItem;
    ToolStripMenuItem exitMenuItem;

    public MainForm()
    {
        Text = "Image Viewer — Pan & Zoom";
        Width = 1000;
        Height = 700;

        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("File");
        openMenuItem = new ToolStripMenuItem("Open...", null, OnOpenClicked);
        fitMenuItem = new ToolStripMenuItem("Fit to Window (F)", null, (s, e) => viewer.FitImage());
        exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());
        file.DropDownItems.Add(openMenuItem);
        file.DropDownItems.Add(fitMenuItem);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(exitMenuItem);
        menu.Items.Add(file);
        Controls.Add(menu);
        MainMenuStrip = menu;

        viewer = new Viewer(this);
        viewer.Dock = DockStyle.Fill;
        Controls.Add(viewer);

        menu.BringToFront();

        AllowDrop = true;
        DragEnter += MainForm_DragEnter;
        DragDrop += MainForm_DragDrop;

        KeyPreview = true;
        KeyDown += MainForm_KeyDown;
    }

    void OnOpenClicked(object sender, EventArgs e)
    {
        using (var dlg = new OpenFileDialog())
        {
            dlg.Filter = "Images|*.bmp;*.png;*.jpg;*.jpeg;*.gif;*.tif;*.tiff;*.webp|All files|*.*";
            dlg.Title = "Open image";
            if (dlg.ShowDialog(this) == DialogResult.OK)
                viewer.LoadImage(dlg.FileName);
        }
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

    void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files != null && files.Length > 0 && IsImageFile(files[0]))
            viewer.LoadImage(files[0]);
    }

    bool IsImageFile(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" ||
               ext == ".gif" || ext == ".tif" || ext == ".tiff" || ext == ".webp";
    }

    void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.R)
            viewer.ResetView();
        else if (e.KeyCode == Keys.F)
            viewer.FitImage();
        else if (e.KeyCode == Keys.O && e.Control)
            OnOpenClicked(null, EventArgs.Empty);
    }
}

public class Viewer : Control
{
    Image image;
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

    public void LoadImage(string path)
    {
        try
        {
            var img = Image.FromFile(path);
            if (image != null) image.Dispose();
            image = img;
            FitImage();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to load image:\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ResetView()
    {
        scale = 1.0f;
        offset = new PointF(0, 0);
        ClampOffset();
        Invalidate();
    }

    public void FitImage()
    {
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            ZoomBy(1.2f, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        else if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            ZoomBy(1.0f / 1.2f, new Point(ClientSize.Width / 2, ClientSize.Height / 2));
        else if (e.KeyCode == Keys.R) ResetView();
        else if (e.KeyCode == Keys.F) FitImage();
    }

    void ZoomBy(float factor, Point center)
    {
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

    protected override void Dispose(bool disposing)
    {
        if (disposing && image != null)
        {
            image.Dispose();
            image = null;
        }
        base.Dispose(disposing);
    }
}
