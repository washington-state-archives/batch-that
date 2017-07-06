using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Enhance : Filter
    {
        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.Enhance();
            return image;
        }
    }
}
