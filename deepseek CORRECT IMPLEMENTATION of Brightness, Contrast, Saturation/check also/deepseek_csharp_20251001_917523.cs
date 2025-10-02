public static Bitmap ApplyMultipleAdjustments(Bitmap sourceImage, float gamma, float brightness, float contrast, float saturation)
{
    Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
    
    using (ImageAttributes imageAttributes = new ImageAttributes())
    using (Graphics g = Graphics.FromImage(result))
    {
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        
        // Apply gamma correction
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        
        // Create combined matrix for brightness, contrast, and saturation
        float scale = contrast; // >1 increase contrast, <1 decrease contrast
        float translation = (1 - scale) / 2 + brightness; // Adjust for brightness
        
        // Saturation matrix components
        // When saturation = 0: grayscale (weights: R:0.299, G:0.587, B:0.114)
        // When saturation = 1: original colors
        // When saturation > 1: oversaturated
        float saturationFactor = saturation;
        float rWeight = 0.299f;
        float gWeight = 0.587f;
        float bWeight = 0.114f;
        
        float rLuminance = rWeight * (1 - saturationFactor);
        float gLuminance = gWeight * (1 - saturationFactor);
        float bLuminance = bWeight * (1 - saturationFactor);
        
        float[,] saturationMatrix = {
            { rLuminance + saturationFactor, rLuminance, rLuminance, 0, 0 },     // Red
            { gLuminance, gLuminance + saturationFactor, gLuminance, 0, 0 },     // Green
            { bLuminance, bLuminance, bLuminance + saturationFactor, 0, 0 },     // Blue
            { 0, 0, 0, 1, 0 },                                                   // Alpha
            { 0, 0, 0, 0, 1 }                                                    // Translation
        };
        
        // Brightness and contrast matrix
        float[,] brightnessContrastMatrix = {
            { scale, 0, 0, 0, 0 },                    // Red
            { 0, scale, 0, 0, 0 },                    // Green  
            { 0, 0, scale, 0, 0 },                    // Blue
            { 0, 0, 0, 1, 0 },                        // Alpha
            { translation, translation, translation, 0, 1 } // Brightness
        };
        
        // Combine saturation with brightness/contrast
        // We need to multiply the matrices: saturationMatrix × brightnessContrastMatrix
        float[,] combinedMatrix = MultiplyColorMatrices(saturationMatrix, brightnessContrastMatrix);
        
        ColorMatrix finalColorMatrix = new ColorMatrix(combinedMatrix);
        
        // Apply the combined color matrix
        imageAttributes.SetColorMatrix(finalColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        
        Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        g.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, 
                   GraphicsUnit.Pixel, imageAttributes);
    }
    
    return result;
}

// Helper method to multiply two 5x5 color matrices
private static float[,] MultiplyColorMatrices(float[,] matrix1, float[,] matrix2)
{
    float[,] result = new float[5, 5];
    
    for (int i = 0; i < 5; i++)
    {
        for (int j = 0; j < 5; j++)
        {
            result[i, j] = 0;
            for (int k = 0; k < 5; k++)
            {
                result[i, j] += matrix1[i, k] * matrix2[k, j];
            }
        }
    }
    
    return result;
}