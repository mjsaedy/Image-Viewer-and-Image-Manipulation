/*
Brightness: -1.0 (darkest) to 1.0 (brightest), where 0 is normal
Contrast: -1.0 (minimum contrast) to 1.0 (maximum contrast), where 0 is normal
Saturation: 0.0 (grayscale) to 2.0 (high saturation), where 1.0 is normal
*/

using System;
using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 5)
        {
            Console.WriteLine("Usage: program.exe INPUT OUTPUT BRIGHTNESS CONTRAST SATURATION");
            Console.WriteLine("Example: program.exe input.jpg output.jpg 0.2 1.2 1.5");
            return;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        if (!float.TryParse(args[2], out float brightness) ||
            !float.TryParse(args[3], out float contrast) ||
            !float.TryParse(args[4], out float saturation))
        {
            Console.WriteLine("Error: BRIGHTNESS, CONTRAST, and SATURATION must be valid float values.");
            return;
        }

        try
        {
            using (Bitmap bitmap = new Bitmap(inputPath))
            {
                ImageAdjustment.ApplyAdjustment(bitmap, brightness, contrast, saturation);
                bitmap.Save(outputPath);
            }

            Console.WriteLine("Image saved to: " + outputPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing image: " + ex.Message);
        }
    }
}
