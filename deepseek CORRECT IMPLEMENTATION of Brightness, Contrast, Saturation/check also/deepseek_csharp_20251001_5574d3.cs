public static Bitmap ApplyMultipleAdjustmentsSimple(Bitmap sourceImage, float gamma, float brightness, float contrast, float saturation)
{
    Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
    
    using (ImageAttributes imageAttributes = new ImageAttributes())
    using (Graphics g = Graphics.FromImage(result))
    {
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        // Apply gamma correction
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        
        // Create saturation matrix
        float rWeight = 0.299f;
        float gWeight = 0.587f;
        float bWeight = 0.114f;
        
        float rLuminance = rWeight * (1 - saturation);
        float gLuminance = gWeight * (1 - saturation);
        float bLuminance = bWeight * (1 - saturation);
        
        ColorMatrix saturationMatrix = new ColorMatrix(new float[][]
        {
            new float[] { rLuminance + saturation, rLuminance, rLuminance, 0, 0 },
            new float[] { gLuminance, gLuminance + saturation, gLuminance, 0, 0 },
            new float[] { bLuminance, bLuminance, bLuminance + saturation, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 },
            new float[] { 0, 0, 0, 0, 1 }
        });
        
        // Create brightness/contrast matrix
        float scale = contrast;
        float translation = (1 - scale) / 2 + brightness;
        
        ColorMatrix brightnessContrastMatrix = new ColorMatrix(new float[][]
        {
            new float[] { scale, 0, 0, 0, 0 },
            new float[] { 0, scale, 0, 0, 0 },
            new float[] { 0, 0, scale, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 },
            new float[] { translation, translation, translation, 0, 1 }
        });
        
        // Apply both matrices (they'll be combined automatically by GDI+)
        imageAttributes.SetColorMatrix(saturationMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        imageAttributes.SetColorMatrix(brightnessContrastMatrix, ColorMatrixFlag.Multiply, ColorAdjustType.Bitmap);
        
        Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        g.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, 
                   GraphicsUnit.Pixel, imageAttributes);
    }
    
    return result;
}