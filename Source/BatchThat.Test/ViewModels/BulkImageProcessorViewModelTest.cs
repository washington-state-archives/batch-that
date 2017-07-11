using BatchThat.Image.Enums;
using BatchThat.Image.EventArguments;
using BatchThat.ViewModels;
using Xunit;

namespace BatchThat.Test.ViewModels
{
    public class BulkImageProcessorViewModelTest
    {
        [Fact]
        public void FilterMessages_WhenParameterEnumMessageTypeAll()
        {
            BulkImageProcessorViewModel viewModel = new BulkImageProcessorViewModel();
            viewModel.Log.Add(new ChangedEventMessage("all", EnumMessageType.All));
            Assert.Equal(1, viewModel.Log.Count);

            viewModel.FilterAllMessages.Execute(EnumMessageType.All);

            Assert.Equal(0, viewModel.Log.Count);
        }

        [Fact]
        public void FilterMessages_WhenParameterEnumMessageTypeError()
        {
            BulkImageProcessorViewModel viewModel = new BulkImageProcessorViewModel();
            viewModel.Log.Add(new ChangedEventMessage("all", EnumMessageType.All));
            Assert.Equal(1, viewModel.Log.Count);

            viewModel.FilterAllMessages.Execute(EnumMessageType.Error);

            Assert.Equal(0, viewModel.Log.Count);
        }

        [Fact]
        public void FilterMessages_WhenParameterEnumMessageTypeInformational()
        {
            BulkImageProcessorViewModel viewModel = new BulkImageProcessorViewModel();
            viewModel.Log.Add(new ChangedEventMessage("all", EnumMessageType.All));
            Assert.Equal(1, viewModel.Log.Count);

            viewModel.FilterAllMessages.Execute(EnumMessageType.Informational);

            Assert.Equal(0, viewModel.Log.Count);
        }
    }
}