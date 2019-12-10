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

        [Fact]
        public void TestPassthroughValueOnNoDescription()
        {
            ImageManager testImageManager = new ImageManager();

            byte[] badProfileData = new byte[]
            {
                0, 0, 2, 48, 65, 68, 66
            };

            string profileDescription = "Initial value";

            testImageManager.AssignDescriptionFromICCProfile(badProfileData, ref profileDescription);

            Assert.Equal("Initial value", profileDescription);
        }
        [Fact]
        public void TestFindICCDescription()
        {
            ImageManager testImageManager = new ImageManager();

            string profileDescription = "Initial value";

            byte[] iccProfile = new byte[]
            {
                0, 0, 2, 48, 65, 68, 66, 69, 2, 16, 0, 0, 109, 110, 116, 114, 82, 71, 66, 32, 88, 89, 90, 32, 7, 207, 0,
                6, 0, 3, 0, 0, 0, 0, 0, 0, 97, 99, 115, 112, 65, 80, 80, 76, 0, 0, 0, 0, 110, 111, 110, 101, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 246, 214, 0, 1, 0, 0, 0, 0, 211, 45, 65, 68, 66, 69, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 99, 112, 114, 116, 0, 0, 0, 252, 0, 0, 0, 50, 100, 101, 115, 99, 0, 0, 1,
                48, 0, 0, 0, 107, 119, 116, 112, 116, 0, 0, 1, 156, 0, 0, 0, 20, 98, 107, 112, 116, 0, 0, 1, 176, 0, 0,
                0, 20, 114, 84, 82, 67, 0, 0, 1, 196, 0, 0, 0, 14, 103, 84, 82, 67, 0, 0, 1, 212, 0, 0, 0, 14, 98, 84,
                82, 67, 0, 0, 1, 228, 0, 0, 0, 14, 114, 88, 89, 90, 0, 0, 1, 244, 0, 0, 0, 20, 103, 88, 89, 90, 0, 0, 2,
                8, 0, 0, 0, 20, 98, 88, 89, 90, 0, 0, 2, 28, 0, 0, 0, 20, 116, 101, 120, 116, 0, 0, 0, 0, 67, 111, 112,
                121, 114, 105, 103, 104, 116, 32, 49, 57, 57, 57, 32, 65, 100, 111, 98, 101, 32, 83, 121, 115, 116, 101,
                109, 115, 32, 73, 110, 99, 111, 114, 112, 111, 114, 97, 116, 101, 100, 0, 0, 0, 100, 101, 115, 99, 0, 0,
                0, 0, 0, 0, 0, 17, 65, 100, 111, 98, 101, 32, 82, 71, 66, 32, 40, 49, 57, 57, 56, 41, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 88, 89, 90, 32, 0, 0, 0, 0, 0, 0, 243, 81, 0, 1, 0, 0, 0, 1, 22, 204, 88, 89, 90, 32, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 99, 117, 114, 118, 0, 0, 0, 0, 0, 0, 0, 1, 2, 51, 0, 0, 99,
                117, 114, 118, 0, 0, 0, 0, 0, 0, 0, 1, 2, 51, 0, 0, 99, 117, 114, 118, 0, 0, 0, 0, 0, 0, 0, 1, 2, 51, 0,
                0, 88, 89, 90, 32, 0, 0, 0, 0, 0, 0, 156, 24, 0, 0, 79, 165, 0, 0, 4, 252, 88, 89, 90, 32, 0, 0, 0, 0,
                0, 0, 52, 141, 0, 0, 160, 44, 0, 0, 15, 149, 88, 89, 90, 32, 0, 0, 0, 0, 0, 0, 38, 49, 0, 0, 16, 47, 0,
                0, 190, 156
            };

            testImageManager.AssignDescriptionFromICCProfile(iccProfile, ref profileDescription);

            Assert.Equal("Adobe RGB (1998)", profileDescription);
        }
    }
}