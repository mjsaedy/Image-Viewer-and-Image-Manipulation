public static Bitmap ApplyMultipleAdjustments(Bitmap sourceImage, float gamma, float brightness, float contrast)
{
    Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
    
    using (ImageAttributes imageAttributes = new ImageAttributes())
    using (Graphics g = Graphics.FromImage(result))
    {
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        // Apply gamma correction
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        
        // Apply brightness and contrast using ColorMatrix
        float scale = contrast; // >1 increase contrast, <1 decrease contrast
        float translation = (1 - scale) / 2 + brightness; // Adjust for brightness
        
        ColorMatrix brightnessContrastMatrix = new ColorMatrix(new float[][]
        {
            new float[] {scale, 0, 0, 0, 0},        // Red
            new float[] {0, scale, 0, 0, 0},        // Green
            new float[] {0, 0, scale, 0, 0},        // Blue
            new float[] {0, 0, 0, 1, 0},           // Alpha
            new float[] {translation, translation, translation, 0, 1} // Brightness
        });
        
        // Combine gamma with brightness/contrast
        imageAttributes.SetColorMatrix(brightnessContrastMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        
        Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        g.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, 
                   GraphicsUnit.Pixel, imageAttributes);
    }
    
    return result;
}