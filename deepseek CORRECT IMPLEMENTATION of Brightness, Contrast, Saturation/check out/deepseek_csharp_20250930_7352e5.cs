public static Bitmap ApplySepiaTone(Image original)
{
    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {0.393f, 0.349f, 0.272f, 0, 0},
            new float[] {0.769f, 0.686f, 0.534f, 0, 0},
            new float[] {0.189f, 0.168f, 0.131f, 0, 0},
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