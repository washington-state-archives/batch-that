using Xunit;

namespace BatchThat.Test
{
    public class RelayCommandTest
    {
        private bool MethodCalled;
        private void Method(object parameter)
        {
            MethodCalled = true;
        }

        [Fact]
        public void Execute_WhenParameterIsNotNull()
        {
            RelayCommand relayCommand = new RelayCommand(Method);
            Assert.False(MethodCalled);

            relayCommand.Execute("");

            Assert.True(MethodCalled);
        }

        [Fact]
        public void Execute_WhenParameterIsNull()
        {
            RelayCommand relayCommand = new RelayCommand(Method);
            Assert.False(MethodCalled);

            relayCommand.Execute(null);

            Assert.False(MethodCalled);
        }

        [Fact]
        public void CanExecute()
        {
            RelayCommand relayCommand = new RelayCommand(Method);

            bool canExecute = relayCommand.CanExecute("");

            Assert.True(canExecute);
        }
    }
}