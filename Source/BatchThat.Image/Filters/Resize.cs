using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Resize : Filter
    {
        public int Width { get; }
        public int Height { get; }
        public bool IgnoreAspectRatio { get; }

        public Resize(int width, int height, bool ignoreAspectRatio)
        {
            Width = width;
            Height = height;
            IgnoreAspectRatio = ignoreAspectRatio;
        }

        public override MagickImage ApplyFilter(MagickImage image)
        {
            var size = new MagickGeometry(Width, Height) { IgnoreAspectRatio = IgnoreAspectRatio };
            image.Resize(size);
            return image;
        }
    }
}
