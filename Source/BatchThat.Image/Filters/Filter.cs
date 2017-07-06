using ImageMagick;

namespace BatchThat.Image.Filters
{
    public abstract class Filter
    {
        public abstract MagickImage ApplyFilter(MagickImage image);
    }
}
