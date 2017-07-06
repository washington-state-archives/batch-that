using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Trim : Filter
    {
        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.Trim();
            return image;
        }
    }
}
