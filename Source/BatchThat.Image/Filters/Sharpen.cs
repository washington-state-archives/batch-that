using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Sharpen : Filter
    {
        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.Sharpen();
            return image;
        }
    }
}
