using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Deskew : Filter
    {
        public int Threshold { get; }

        public Deskew(int threshold)
        {
            Threshold = threshold;
        }

        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.BackgroundColor = MagickColor.FromRgb(0, 0, 0);
            image.Deskew(new Percentage(Threshold));
            image.RePage();
            return image;
        }
    }
}
