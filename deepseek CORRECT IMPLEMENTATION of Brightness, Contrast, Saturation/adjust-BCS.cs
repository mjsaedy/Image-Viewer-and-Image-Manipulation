using System;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 5)
        {
            Console.WriteLine("Usage: program.exe INPUT OUTPUT BRIGHTNESS% CONTRAST% SATURATION%");
            Console.WriteLine("Example: program.exe input.jpg output.jpg 20 -15 50");
            Console.WriteLine("Ranges: -100% to 100% for all parameters");
            return;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        if (!int.TryParse(args[2], out int brightnessPercent) ||
            !int.TryParse(args[3], out int contrastPercent) ||
            !int.TryParse(args[4], out int saturationPercent))
        {
            Console.WriteLine("Error: BRIGHTNESS, CONTRAST, and SATURATION must be valid integer percentages.");
            return;
        }

        // Validate percentage ranges
        if (brightnessPercent < -100 || brightnessPercent > 100 ||
            contrastPercent < -100 || contrastPercent > 100 ||
            saturationPercent < -100 || saturationPercent > 100)
        {
            Console.WriteLine("Error: All percentages must be between -100 and 100.");
            return;
        }

        try
        {
            // Convert percentages to float values expected by the adjustment method
            float brightness = PercentageToBrightness(brightnessPercent);
            float contrast = PercentageToContrast(contrastPercent);
            float saturation = PercentageToSaturation(saturationPercent);

            Console.WriteLine($"Adjusting image: Brightness={brightnessPercent}%, Contrast={contrastPercent}%, Saturation={saturationPercent}%");
            Console.WriteLine($"Converted to: Brightness={brightness:F2}, Contrast={contrast:F2}, Saturation={saturation:F2}");

            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                ImageAdjustment.ApplyAdjustment(bitmap, brightness, contrast, saturation);
                bitmap.Save(outputPath);
            }

            Console.WriteLine("Image successfully saved to: " + outputPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing image: " + ex.Message);
        }
    }

    /// <summary>
    /// Converts brightness percentage (-100 to 100) to float value (-1.0 to 1.0)
    /// -100% = -1.0 (darkest), 0% = 0.0 (normal), 100% = 1.0 (brightest)
    /// </summary>
    private static float PercentageToBrightness(int percent)
    {
        return percent / 100.0f;
    }

    /// <summary>
    /// Converts contrast percentage (-100 to 100) to float value (-1.0 to 1.0)
    /// -100% = -1.0 (minimum contrast), 0% = 0.0 (normal), 100% = 1.0 (maximum contrast)
    /// </summary>
    private static float PercentageToContrast(int percent)
    {
        return percent / 100.0f;
    }

    /// <summary>
    /// Converts saturation percentage (-100 to 100) to float value (0.0 to 2.0)
    /// -100% = 0.0 (grayscale), 0% = 1.0 (normal), 100% = 2.0 (high saturation)
    /// </summary>
    private static float PercentageToSaturation(int percent)
    {
        // Map -100..0..100 to 0.0..1.0..2.0
        return 1.0f + (percent / 100.0f);
    }
}