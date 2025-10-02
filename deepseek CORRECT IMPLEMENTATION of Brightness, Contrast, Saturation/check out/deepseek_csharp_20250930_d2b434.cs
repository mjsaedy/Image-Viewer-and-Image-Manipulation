public static Bitmap InvertColors(Image original)
{
    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {-1, 0, 0, 0, 0},
            new float[] {0, -1, 0, 0, 0},
            new float[] {0, 0, -1, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {1, 1, 1, 0, 1}
        });

        using (ImageAttributes attributes = new ImageAttributes())
        {
            attributes.SetColorMatrix(colorMatrix);
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                        0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
        }
    }
    return newBitmap;
}