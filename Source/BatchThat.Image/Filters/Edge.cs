using ImageMagick;

namespace BatchThat.Image.Filters
{
    public class Edge : Filter
    {
        public double Radius { get; }

        public Edge(double radius)
        {
            Radius = radius;
        }

        public override MagickImage ApplyFilter(MagickImage image)
        {
            image.Edge(Radius);
            return image;
        }
    }
}
