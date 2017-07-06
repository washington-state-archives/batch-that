using System.Windows;
using BatchThat.ViewModels;

namespace BatchThat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ImageProcessorViewModel ViewModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
