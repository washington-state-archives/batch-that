using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BatchThat.Image;
using BatchThat.Image.EventArguments;
using Microsoft.Win32;

namespace BatchThat.ViewModels
{
    public class SingleImageProcessorViewModel : ImageProcessorViewModel
    {
        private string _sourceFile;
        private ImageSource _previewFile;

        protected ImageManager ImageManager { get; }

        public string SourceFile
        {
            get => _sourceFile;
            set { _sourceFile = value; OnPropertyChanged(GetPropertyInfo(this, x => x.SourceFile).Name); }
        }

        public ICommand BrowseImageFileCommand { get; set; }
        public ICommand ApplyFiltersCommand { get; set; }
        public ICommand SaveImageCommand { get; set; }
        public ImageSource PreviewFile
        {
            get => _previewFile;
            set { _previewFile = value; OnPropertyChanged(GetPropertyInfo(this, x => x.PreviewFile).Name); }
        }

        public SingleImageProcessorViewModel()
        {
            Enabled = true;
            ImageManager = new ImageManager();
            BrowseImageFileCommand = new RelayCommand(BrowseImageFile);
            SaveImageCommand = new RelayCommand(SaveImage);
        }

        private void BrowseImageFile(object obj)
        {
            var imageSourceConverter = new ConvertBitmapToBitmapImage();
            var openFileDialog = new OpenFileDialog { Filter = "TIFF Files (*.tif)|*.tif|All Files (*.*)|*.*" };
            if (string.IsNullOrWhiteSpace(SourceFile))
            {
                var showDialog = openFileDialog.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    SourceFile = openFileDialog.FileName;
                    PreviewFile = imageSourceConverter.Convert(new Bitmap(System.Drawing.Image.FromFile(SourceFile)));
                }
            }
            else
            {
                if (Directory.Exists(Path.GetDirectoryName(SourceFile)))
                {
                    if (File.Exists(SourceFile))
                    {
                        openFileDialog.FileName = SourceFile;
                    }
                }

                var showDialog = openFileDialog.ShowDialog();
                if (showDialog != null && showDialog.Value)
                {
                    SourceFile = openFileDialog.FileName;
                    PreviewFile = imageSourceConverter.Convert(new Bitmap(System.Drawing.Image.FromFile(SourceFile)));
                }
            }
        }

        private async void SaveImage(object obj)
        {
            ImageManager.ProgressChanged += ProgressChanged;

            var saveFileDialog = new SaveFileDialog { Filter = "TIFF Files (*.tif)|*.tif" };
            var showDialog = saveFileDialog.ShowDialog();
            if (showDialog != null && showDialog.Value)
            {
                Enabled = false;
                await Task.Run(() =>
                {
                    ImageManager.ApplyFilters(GetFilters(), SourceFile, saveFileDialog.FileName);
                });
                Enabled = true;
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgument e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                PreviewFile.Freeze();
                PreviewFile = new ConvertBitmapToBitmapImage().Convert(e.Image);
                Current = e.Current;
                Total = e.Total;
            });
        }
    }
}
