public static Bitmap AdjustColorChannels(Image original, float redScale, float greenScale, float blueScale)
{
    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {redScale, 0, 0, 0, 0},
            new float[] {0, greenScale, 0, 0, 0},
            new float[] {0, 0, blueScale, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1}
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