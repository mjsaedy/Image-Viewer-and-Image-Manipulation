public static Bitmap ApplyMultipleEffects(Image original, float brightness, float contrast, float saturation)
{
    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        // Create individual matrices
        ColorMatrix brightnessMatrix = CreateBrightnessMatrix(brightness);
        ColorMatrix contrastMatrix = CreateContrastMatrix(contrast);
        ColorMatrix saturationMatrix = CreateSaturationMatrix(saturation);
        
        // Combine matrices (order matters!)
        ColorMatrix finalMatrix = MultiplyColorMatrices(
            MultiplyColorMatrices(brightnessMatrix, contrastMatrix), saturationMatrix);

        using (ImageAttributes attributes = new ImageAttributes())
        {
            attributes.SetColorMatrix(finalMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
    }
    return newBitmap;
}

private static ColorMatrix MultiplyColorMatrices(ColorMatrix a, ColorMatrix b)
{
    // Implementation of matrix multiplication for ColorMatrix
    // This is a simplified version - in practice, you'd implement proper 5x5 matrix multiplication
    return a; // Placeholder
}