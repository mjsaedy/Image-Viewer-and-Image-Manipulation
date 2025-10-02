public static Bitmap ApplyGammaCorrectly(Bitmap sourceImage, float gamma)
{
    Bitmap result = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb);
    
    using (ImageAttributes imageAttributes = new ImageAttributes())
    using (Graphics g = Graphics.FromImage(result))
    {
        // For proper gamma correction, we need to use a ColorMap or different approach
        // Since ColorMatrix can't do power functions directly, we use SetGamma method
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        
        Rectangle destRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
        g.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, 
                   GraphicsUnit.Pixel, imageAttributes);
    }
    
    return result;
}