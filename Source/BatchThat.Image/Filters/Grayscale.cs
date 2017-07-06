using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Grayscale : Filter
    {
        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.ColorType = ColorType.Grayscale;
            return image;
        }
    }
}
