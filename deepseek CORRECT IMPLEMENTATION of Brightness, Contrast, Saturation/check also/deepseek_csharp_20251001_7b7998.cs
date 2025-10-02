class Program
{
    static void Main()
    {
        // Load an image (might be 24bpp JPEG, 8bpp PNG, etc.)
        using (Image originalImage = Image.FromFile("input.jpg"))
        {
            Console.WriteLine($"Original format: {originalImage.PixelFormat}");
            
            // Convert to 32bpp first
            using (Bitmap thirtyTwoBitImage = ImageFormatConverter.ConvertTo32bppArgb(originalImage))
            {
                Console.WriteLine($"Converted format: {thirtyTwoBitImage.PixelFormat}");
                
                // Now apply gamma correction safely
                Bitmap gammaCorrected = GammaCorrector.ApplyGamma(thirtyTwoBitImage, 2.2);
                
                // Save the result (use PNG to preserve quality and alpha channel)
                gammaCorrected.Save("final_output.png", ImageFormat.Png);
                
                gammaCorrected.Dispose();
            }
        }
    }
}