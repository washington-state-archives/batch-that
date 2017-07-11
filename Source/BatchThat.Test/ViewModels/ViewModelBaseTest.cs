using System;
using System.Reflection;
using Xunit;

namespace BatchThat.Test.ViewModels
{
    public class ViewModelBaseTest
    {
        [Fact]
        public void GetPropertyInfo_WhenParameterIsMethod()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel();

            try
            {
                viewModel.GetPropertyInfo(viewModel, model => model.GetFilters());
                Assert.False(true, "exception should have been thrown");
            }
            catch (ArgumentException exception)
            {
                Assert.Equal("Expression 'model => model.GetFilters()' refers to a method, not a property.",
                             exception.Message);
            }
        }

        [Fact]
        public void GetPropertyInfo_WhenParameterIsField()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel();

            try
            {
                viewModel.GetPropertyInfo(viewModel, model => model.Field);
                Assert.False(true, "exception should have been thrown");
            }
            catch (ArgumentException exception)
            {
                Assert.Equal("Expression 'model => model.Field' refers to a field, not a property.",
                             exception.Message);
            }
        }

        [Fact]
        public void GetPropertyInfo()
        {
            ConcreteImageProcessorViewModel viewModel = new ConcreteImageProcessorViewModel();

            PropertyInfo propertyInfo = viewModel.GetPropertyInfo(viewModel, model => model.Enabled);

            Assert.Equal(typeof(bool), propertyInfo.PropertyType);
        }
    }
}