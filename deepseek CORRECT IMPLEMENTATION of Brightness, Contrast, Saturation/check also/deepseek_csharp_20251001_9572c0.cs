// Example usage with different saturation values:
using (Bitmap original = new Bitmap("input.jpg"))
{
    // Normal saturation (1.0)
    Bitmap normal = ApplyMultipleAdjustments(original, 1.0f, 0.0f, 1.0f, 1.0f);
    
    // Grayscale (saturation = 0)
    Bitmap grayscale = ApplyMultipleAdjustments(original, 1.0f, 0.0f, 1.0f, 0.0f);
    
    // Oversaturated (saturation = 2.0)
    Bitmap oversaturated = ApplyMultipleAdjustments(original, 1.0f, 0.1f, 1.2f, 2.0f);
    
    // Desaturated vintage look
    Bitmap vintage = ApplyMultipleAdjustments(original, 1.2f, 0.1f, 1.1f, 0.5f);
    
    normal.Save("normal.jpg");
    grayscale.Save("grayscale.jpg");
    oversaturated.Save("oversaturated.jpg");
    vintage.Save("vintage.jpg");
}