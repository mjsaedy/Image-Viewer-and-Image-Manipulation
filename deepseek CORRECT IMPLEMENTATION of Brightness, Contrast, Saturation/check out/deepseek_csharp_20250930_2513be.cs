public static Bitmap AdjustSaturation(Image original, float saturation)
{
    // saturation: 0.0 (grayscale) to 2.0 (oversaturated)
    float rWeight = 0.3086f;
    float gWeight = 0.6094f;
    float bWeight = 0.0820f;

    float r = (1.0f - saturation) * rWeight;
    float g = (1.0f - saturation) * gWeight;
    float b = (1.0f - saturation) * bWeight;

    Bitmap newBitmap = new Bitmap(original.Width, original.Height);
    using (Graphics g = Graphics.FromImage(newBitmap))
    {
        ColorMatrix colorMatrix = new ColorMatrix(new float[][]
        {
            new float[] {r + saturation, r, r, 0, 0},
            new float[] {g, g + saturation, g, 0, 0},
            new float[] {b, b, b + saturation, 0, 0},
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