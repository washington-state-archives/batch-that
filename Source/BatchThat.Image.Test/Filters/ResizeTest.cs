using BatchThat.Image.Filters;
using Xunit;

namespace BatchThat.Image.Test.Filters
{
    public class ResizeTest
    {
        [Fact]
        public void Constructor()
        {
            Resize resize = new Resize(10, 20, true);

            Assert.Equal(10, resize.Width);
            Assert.Equal(20, resize.Height);
            Assert.True(resize.IgnoreAspectRatio);
        }
    }
}