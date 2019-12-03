using BatchThat.Image;
using BatchThat;
using Xunit;
namespace BatchThat.Image.Test
{
    public class ImageManagerTest
    {
        [Fact]
        public void TranslateHumanReadableFileSizes()
        {
            ImageManager testImageManager = new ImageManager();

            string sizeResult = testImageManager.GetHumanReadableFileSize(600);
            Assert.Equal("600 bytes", sizeResult);
            sizeResult = testImageManager.GetHumanReadableFileSize(2048);
            Assert.Equal("2 KB", sizeResult);
            sizeResult = testImageManager.GetHumanReadableFileSize(2560);
            Assert.Equal("2.5 KB", sizeResult);
            sizeResult = testImageManager.GetHumanReadableFileSize(31575374);
            Assert.Equal("30.1 MB", sizeResult);
            sizeResult = testImageManager.GetHumanReadableFileSize(23232294);
            Assert.Equal("22.2 MB", sizeResult);
        }
    }
}