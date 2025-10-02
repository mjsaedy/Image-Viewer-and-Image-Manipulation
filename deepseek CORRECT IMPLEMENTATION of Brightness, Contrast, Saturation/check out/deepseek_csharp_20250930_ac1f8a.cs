public static Bitmap AdjustContrast(Image original, float contrast)
{
    // contrast: 0.0 to 2.0 (0% to 200%)
    float scale = (1.0f + contrast) / 2.0f;
    float translate = (1.0f - scale) / 2.0f;

    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {scale, 0, 0, 0, 0},
            new float[] {0, scale, 0, 0, 0},
            new float[] {0, 0, scale, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {translate, translate, translate, 0, 1}
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