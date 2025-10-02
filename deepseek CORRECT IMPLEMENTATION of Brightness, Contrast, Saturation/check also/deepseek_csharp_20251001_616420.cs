using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public static Bitmap ApplyGammaWithImageAttributes(Bitmap sourceImage, float gamma)
{
    // Create a new 32bpp bitmap to draw onto (preserves format)
    Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
    
    // Create ImageAttributes for color adjustment
    using (ImageAttributes imageAttributes = new ImageAttributes())
    using (Graphics g = Graphics.FromImage(result))
    {
        // Configure high-quality rendering
        g.CompositingMode = CompositingMode.SourceCopy;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        
        // Create a color matrix for gamma adjustment
        // Gamma is applied using a simple power function in the color matrix
        float adjustedGamma = gamma;
        
        // Apply gamma to all color channels (R, G, B) but not alpha
        ColorMatrix colorMatrix = new ColorMatrix
        {
            Matrix00 = adjustedGamma, // Red
            Matrix11 = adjustedGamma, // Green  
            Matrix22 = adjustedGamma, // Blue
            Matrix33 = 1.0f,         // Alpha (unchanged)
            Matrix44 = 1.0f          // Overall scaling (unchanged)
        };
        
        // Set the gamma adjustment
        imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        
        // Draw the image with gamma correction applied
        Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        g.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, 
                   GraphicsUnit.Pixel, imageAttributes);
    }
    
    return result;
}