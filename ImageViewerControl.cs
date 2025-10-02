using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using Microsoft.VisualBasic;


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
                g.DrawString("File → Open or drag an image here", font, brush, ClientRectangle, sf);
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
        
        /*
        else if (e.KeyCode == Keys.R)
            ResetView();
        
        else if (e.KeyCode == Keys.F)
            FitImage();
        */
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
        using (Graphics g = Graphics.FromImage(adjusted)) {
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
    
    private static ColorMatrix CreateAdjustmentMatrix(float brightness, float contrast, float saturation) {
        // Convert brightness from [-1, 1] to [0, 2] scale for matrix
        float brightnessFactor = brightness + 1.0f;
        
        // Convert contrast from [-1, 1] to scale factor
        // -1 = minimum contrast (0), 0 = normal (1), 1 = maximum contrast (2)
        float contrastFactor = contrast + 1.0f;
        
        // Calculate contrast translation (moves the midpoint)
        float contrastTranslate = (1.0f - contrastFactor) / 2.0f;

        // Calculate saturation matrix components
        // Based on luminance values for RGB: 0.299R + 0.587G + 0.114B
        float luminanceR = 0.299f;
        float luminanceG = 0.587f;
        float luminanceB = 0.114f;
        
        float saturationR = luminanceR * (1 - saturation);
        float saturationG = luminanceG * (1 - saturation);
        float saturationB = luminanceB * (1 - saturation);
        
        float saturationA = saturationR + saturation;
        float saturationB1 = saturationG + saturation;
        float saturationC = saturationB + saturation;

        // Create the combined color matrix
        // Order of operations: saturation -> contrast -> brightness
        return new ColorMatrix(new float[][]
        {
            // Red channel
            new float[] { saturationA * contrastFactor, saturationR * contrastFactor, saturationR * contrastFactor, 0, 0 },
            // Green channel  
            new float[] { saturationG * contrastFactor, saturationB1 * contrastFactor, saturationG * contrastFactor, 0, 0 },
            // Blue channel
            new float[] { saturationB * contrastFactor, saturationB * contrastFactor, saturationC * contrastFactor, 0, 0 },
            // Alpha channel (unchanged)
            new float[] { 0, 0, 0, 1, 0 },
            // Translation row (brightness and contrast offset)
            new float[] { 
                contrastTranslate * brightnessFactor + brightness, 
                contrastTranslate * brightnessFactor + brightness, 
                contrastTranslate * brightnessFactor + brightness, 
                0, 
                brightnessFactor 
            }
        });
    }

    public void ApplyAdjustmentsLive(Bitmap originalImage,
                                    float brightness,
                                    float contrast,
                                    float saturation,
                                    float gamma
                                    )
        {
        if (originalImage == null)
            return;

        if (image != null)
            image.Dispose();

        // Validate input ranges
        brightness = Math.Max(-1.0f, Math.Min(1.0f, brightness));    // -1 to 1
        contrast = Math.Max(-1.0f, Math.Min(1.0f, contrast));        // -1 to 1
        saturation = Math.Max(0.0f, Math.Min(2.0f, saturation));     // 0 to 2
        if (gamma <= 0)                                              // 1 = no change, <1 lighter, >1 darker
            gamma = 0.01f; // avoid division by zero

        // Create color matrix for brightness, contrast, and saturation
        ColorMatrix colorMatrix = CreateAdjustmentMatrix(brightness, contrast, saturation);

        /*
        // Build gamma correction lookup table
        // Math.Pow(i / 255.0, 1.0 / gamma) * 255 → the real gamma mapping.
        // + 0.5 is just for correct rounding to nearest integer before casting to int → byte.
        byte[] gammaLut = new byte[256];
        for (int i = 0; i < 256; i++) {
            gammaLut[i] = (byte)Math.Min(255,
                (int)((Math.Pow(i / 255.0, 1.0 / gamma)) * 255 + 0.5));
        }
        */

        // Apply the color matrix
        image = new Bitmap(originalImage.Width, originalImage.Height);
        using (Graphics g = Graphics.FromImage(image)) {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            ImageAttributes imageAttributes = new ImageAttributes();
            
            try
            {
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                imageAttributes.SetGamma(gamma, System.Drawing.Imaging.ColorAdjustType.Bitmap);
                
                Rectangle rect = new Rectangle(0, 0, originalImage.Width, originalImage.Height);
                g.DrawImage(originalImage, rect, 0, 0, originalImage.Width, originalImage.Height, 
                                 GraphicsUnit.Pixel, imageAttributes);
            }
            finally
            {
                imageAttributes?.Dispose();
            }
        }
        Invalidate();
    }
    
    public void ApplyGamma() {
        if (image == null)
            return;
        string input = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter gamma value (1 = no change, <1 lighter, >1 darker):",
            "Adjust Gamma",
            "1.0"
        );

        if (float.TryParse(input, out float gamma)) {
            Bitmap adjusted = Gamma((Bitmap)image, gamma);
            image.Dispose();
            image = adjusted;
            Invalidate();
        }
    }
        
    private Bitmap Gamma(Bitmap src, float gamma) {
        if (gamma <= 0)
            gamma = 0.01f; // avoid division by zero

        Bitmap adjusted = new Bitmap(src.Width, src.Height);
        using (Graphics g = Graphics.FromImage(adjusted))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            /*
            // Build gamma correction lookup table
            byte[] gammaLut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                gammaLut[i] = (byte)Math.Min(255,
                    (int)((Math.Pow(i / 255.0, 1.0 / gamma)) * 255 + 0.5));
            }
            */

            var ia = new System.Drawing.Imaging.ImageAttributes();
            ia.SetGamma(gamma, System.Drawing.Imaging.ColorAdjustType.Bitmap);

            g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height),
                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, ia);
        }
        return adjusted;
    }

    
    #endregion

}
