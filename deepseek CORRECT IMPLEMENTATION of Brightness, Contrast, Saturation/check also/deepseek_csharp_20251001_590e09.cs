using System.Drawing;
using System.Drawing.Imaging;

public static class ImageFormatConverter
{
    public static Bitmap ConvertTo32bppArgb(Image image)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));
        
        // Check if already in desired format
        if (image is Bitmap bmp && bmp.PixelFormat == PixelFormat.Format32bppArgb)
        {
            return new Bitmap(bmp); // Return a copy
        }
        
        // Create new 32bpp bitmap
        Bitmap result = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
        
        // Set resolution to match original
        result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        
        try
        {
            using (Graphics g = Graphics.FromImage(result))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                
                // Draw the image
                g.DrawImage(image, 0, 0, image.Width, image.Height);
            }
            
            return result;
        }
        catch
        {
            result.Dispose();
            throw;
        }
    }
    
    public static bool Is32bppArgb(Image image)
    {
        return image is Bitmap bmp && bmp.PixelFormat == PixelFormat.Format32bppArgb;
    }
}