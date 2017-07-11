using System.Collections.Generic;
using BatchThat.Image.Filters;
using Xunit;

namespace BatchThat.Test.ViewModels
{
    public class ImageProcessorViewModelTest
    {
        [Fact]
        public void Constructor()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel();

            Assert.Equal(0, viewModel.Current);
            Assert.Equal(100, viewModel.Total);
        }

        [Fact]
        public void GetFilters_None()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel();

            IList<Filter> filters = viewModel.GetFilters();

            Assert.Equal(0, filters.Count);
        }

        [Fact]
        public void GetFilters_All()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel
            {
                Deskew = true, AutoCrop = true, Enhance = true, Greyscale = true, Sharpen = true, Trim = true
            };

            IList<Filter> filters = viewModel.GetFilters();

            Assert.Equal(8, filters.Count);
            Assert.Equal(typeof(Deskew), filters[0].GetType());
            Assert.Equal(60, ((Deskew)filters[0]).Threshold);
            Assert.Equal(typeof(AutoCrop), filters[1].GetType());
            Assert.Equal(typeof(Enhance), filters[2].GetType());
            Assert.Equal(typeof(Grayscale), filters[3].GetType());
            Assert.Equal(typeof(Sharpen), filters[4].GetType());
            Assert.Equal(typeof(Trim), filters[5].GetType());
            Assert.Equal(typeof(Deskew), filters[6].GetType());
            Assert.Equal(40, ((Deskew)filters[6]).Threshold);
            Assert.Equal(typeof(AutoCrop), filters[7].GetType());
        }
    }
}