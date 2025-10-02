// Example usage
using (Image originalImage = Image.FromFile("input.jpg"))
{
    // Apply grayscale
    Bitmap grayscale = ConvertToGrayscale(originalImage);
    grayscale.Save("grayscale.jpg");
    
    // Apply brightness adjustment
    Bitmap brighter = AdjustBrightness(originalImage, 0.3f);
    brighter.Save("brighter.jpg");
    
    // Apply sepia tone
    Bitmap sepia = ApplySepiaTone(originalImage);
    sepia.Save("sepia.jpg");
    
    // Don't forget to dispose bitmaps when done
    grayscale.Dispose();
    brighter.Dispose();
    sepia.Dispose();
}