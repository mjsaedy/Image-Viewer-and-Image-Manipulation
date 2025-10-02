/*
Brightness: -1.0 (darkest) to 1.0 (brightest), where 0 is normal
Contrast: -1.0 (minimum contrast) to 1.0 (maximum contrast), where 0 is normal
Saturation: 0.0 (grayscale) to 2.0 (high saturation), where 1.0 is normal
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;

public static class ImageAdjustment
{
    public static void ApplyAdjustment(Bitmap bitmap, float brightness, float contrast, float saturation)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        // Validate input ranges
        brightness = Math.Max(-1.0f, Math.Min(1.0f, brightness));    // -1 to 1
        contrast = Math.Max(-1.0f, Math.Min(1.0f, contrast));        // -1 to 1
        saturation = Math.Max(0.0f, Math.Min(2.0f, saturation));     // 0 to 2

        // Create color matrix for brightness, contrast, and saturation
        ColorMatrix colorMatrix = CreateAdjustmentMatrix(brightness, contrast, saturation);

        // Apply the color matrix
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            
            try
            {
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                
                Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                graphics.DrawImage(bitmap, rect, 0, 0, bitmap.Width, bitmap.Height, 
                                 GraphicsUnit.Pixel, imageAttributes);
            }
            finally
            {
                imageAttributes?.Dispose();
            }
        }
    }

    private static ColorMatrix CreateAdjustmentMatrix(float brightness, float contrast, float saturation)
    {
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
}