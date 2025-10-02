using System;
using System.Drawing;
using System.Drawing.Imaging;

public static class GammaCorrector
{
    /// <summary>
    /// Applies gamma correction to a Bitmap efficiently using a Look-Up Table (LUT).
    /// </summary>
    /// <param name="sourceImage">The original bitmap.</param>
    /// <param name="gamma">The gamma value. >1.0 darkens, <1.0 brightens.</param>
    /// <returns>A new gamma-corrected Bitmap.</returns>
    public static Bitmap ApplyGamma(Bitmap sourceImage, double gamma)
    {
        // 1. Precompute the Look-Up Table (LUT) for each color component
        byte[] lut = new byte[256];
        double inverseGamma = 1.0 / gamma; // We use the inverse in the formula

        for (int i = 0; i < 256; i++)
        {
            // Normalize the input value to [0, 1]
            double normalized = i / 255.0;
            // Apply the gamma correction formula
            double corrected = Math.Pow(normalized, inverseGamma);
            // Convert back to [0, 255] and store in the LUT
            lut[i] = (byte)(corrected * 255.0);
        }

        // 2. Create a new bitmap to hold the corrected image
        Bitmap correctedImage = new Bitmap(sourceImage.Width, sourceImage.Height);

        // 3. Lock bits for fast, direct memory access
        BitmapData sourceData = sourceImage.LockBits(
            new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb // Assuming 32-bit ARGB
        );

        BitmapData destData = correctedImage.LockBits(
            new Rectangle(0, 0, correctedImage.Width, correctedImage.Height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb
        );

        unsafe
        {
            // Get pointers to the start of the bitmaps
            byte* srcPtr = (byte*)sourceData.Scan0;
            byte* destPtr = (byte*)destData.Scan0;

            // The stride might have padding at the end of each row, so we use the provided value.
            int srcStride = sourceData.Stride;
            int destStride = destData.Stride;

            // Loop through every pixel
            for (int y = 0; y < sourceImage.Height; y++)
            {
                for (int x = 0; x < sourceImage.Width; x++)
                {
                    // Calculate the position of the current pixel
                    int srcPos = (y * srcStride) + (x * 4); // 4 bytes per pixel (BGRA)
                    int destPos = (y * destStride) + (x * 4);

                    // Apply the LUT to the Blue, Green, and Red channels.
                    // Ignore the Alpha channel (index 3) to preserve transparency.
                    destPtr[destPos + 0] = lut[srcPtr[srcPos + 0]]; // Blue
                    destPtr[destPos + 1] = lut[srcPtr[srcPos + 1]]; // Green
                    destPtr[destPos + 2] = lut[srcPtr[srcPos + 2]]; // Red
                    destPtr[destPos + 3] = srcPtr[srcPos + 3];      // Alpha
                }
            }
        }

        // 4. Unlock the bits
        sourceImage.UnlockBits(sourceData);
        correctedImage.UnlockBits(destData);

        return correctedImage;
    }
}

// Usage Example:
class Program
{
    static void Main()
    {
        // Load an image
        using (Bitmap originalBitmap = new Bitmap("path_to_your_image.jpg"))
        {
            // Apply a gamma of 2.2 to brighten it (common correction for dark images)
            // Or apply 0.45 (~1/2.2) to darken an image that looks washed out.
            Bitmap correctedBitmap = GammaCorrector.ApplyGamma(originalBitmap, 2.2);

            // Save the result
            correctedBitmap.Save("path_for_corrected_image.jpg", ImageFormat.Jpeg);

            // Clean up
            correctedBitmap.Dispose();
        }
    }
}