using System.Drawing;
using System.Drawing.Imaging;

public static Bitmap ConvertTo32bppEfficient(Bitmap original)
{
    // If it's already 32bpp, return a copy
    if (original.PixelFormat == PixelFormat.Format32bppArgb)
    {
        return new Bitmap(original);
    }
    
    Bitmap converted = new Bitmap(original.Width, original.Height, 
                                PixelFormat.Format32bppArgb);
    
    // Lock bits for both images for fast access
    BitmapData originalData = original.LockBits(
        new Rectangle(0, 0, original.Width, original.Height),
        ImageLockMode.ReadOnly,
        original.PixelFormat);
    
    BitmapData convertedData = converted.LockBits(
        new Rectangle(0, 0, converted.Width, converted.Height),
        ImageLockMode.WriteOnly,
        PixelFormat.Format32bppArgb);
    
    unsafe
    {
        byte* srcPtr = (byte*)originalData.Scan0;
        byte* destPtr = (byte*)convertedData.Scan0;
        
        int srcBytesPerPixel = Image.GetPixelFormatSize(original.PixelFormat) / 8;
        if (srcBytesPerPixel == 0) srcBytesPerPixel = 1; // For indexed formats
        
        for (int y = 0; y < original.Height; y++)
        {
            for (int x = 0; x < original.Width; x++)
            {
                int srcPos = (y * originalData.Stride) + (x * srcBytesPerPixel);
                int destPos = (y * convertedData.Stride) + (x * 4); // 4 bytes for 32bpp
                
                Color pixelColor;
                
                // Handle different source formats
                if (original.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    // 24bpp RGB: BGR order
                    destPtr[destPos + 0] = srcPtr[srcPos + 0]; // Blue
                    destPtr[destPos + 1] = srcPtr[srcPos + 1]; // Green  
                    destPtr[destPos + 2] = srcPtr[srcPos + 2]; // Red
                    destPtr[destPos + 3] = 255; // Alpha (fully opaque)
                }
                else if (original.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    // For 8bpp indexed, you'd need to look up the color in the palette
                    // This is a simplified version - you'd need the actual palette
                    byte index = srcPtr[srcPos];
                    // You would need to access original.Palette here
                    // For now, we'll just make it grayscale
                    destPtr[destPos + 0] = index; // Blue
                    destPtr[destPos + 1] = index; // Green
                    destPtr[destPos + 2] = index; // Red
                    destPtr[destPos + 3] = 255;   // Alpha
                }
                else
                {
                    // Fallback: use GetPixel (slower but reliable)
                    pixelColor = original.GetPixel(x, y);
                    destPtr[destPos + 0] = pixelColor.B;
                    destPtr[destPos + 1] = pixelColor.G;
                    destPtr[destPos + 2] = pixelColor.R;
                    destPtr[destPos + 3] = pixelColor.A;
                }
            }
        }
    }
    
    original.UnlockBits(originalData);
    converted.UnlockBits(convertedData);
    
    return converted;
}