using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using BatchThat.Image;
using BatchThat.Image.EventArguments;
using Application = System.Windows.Application;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace BatchThat.ViewModels
{
    public class FilePropertyViewModel : ViewModelBase
    {
        private string _sourceFolder;
        private bool _includeSubfolders;
        private string _outputFile;
        private int _total;
        private int _current;
        private static readonly object Mutex = new object();

        protected ImageManager ImageManager { get; }

        public string SourceFolder
        {
            get => _sourceFolder;
            set { _sourceFolder = value; OnPropertyChanged(GetPropertyInfo(this, x => x.SourceFolder).Name); }
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;
            set { _includeSubfolders = value; OnPropertyChanged(GetPropertyInfo(this, x => x.IncludeSubfolders).Name); }
        }

        public int Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Total).Name); }
        }

        public int Current
        {
            get => _current;
            set { _current = value; OnPropertyChanged(GetPropertyInfo(this, x => x.Current).Name); }
        }

        public ICommand BrowseSourceFolderCommand { get; set; }
        public ICommand ExportFilePropertiesCommand { get; set; }

        public FilePropertyViewModel()
        {
            Current = 0;
            Total = 100;
            Enabled = true;
            ImageManager = new ImageManager();
            BrowseSourceFolderCommand = new RelayCommand(BrowseSourceFolder);
            ExportFilePropertiesCommand = new RelayCommand(ExportFileProperties);
        }

        private async void ExportFileProperties(object obj)
        {
            ImageManager.ProgressChanged = ProgressChanged;

            var saveFileDialog = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            var showDialog = saveFileDialog.ShowDialog();
            if (showDialog != null && showDialog.Value)
            {
                if (File.Exists(saveFileDialog.FileName))
                    File.Delete(saveFileDialog.FileName);

                _outputFile = saveFileDialog.FileName;
                Enabled = false;
                await Task.Run(() =>
                {
                    var files = Directory.GetFiles(SourceFolder, "*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => !x.EndsWith(".db"));
                    ImageManager.GetFilesProperties(files.ToList(), saveFileDialog.FileName);
                });
                Enabled = true;
            }
        }

        private void BrowseSourceFolder(object obj)
        {
            var openFolderDialog = new FolderBrowserDialog();
            if (string.IsNullOrWhiteSpace(SourceFolder))
            {
                var showDialog = openFolderDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    SourceFolder = openFolderDialog.SelectedPath;
                }
            }
            else
            {
                if (Directory.Exists(Path.GetDirectoryName(SourceFolder)))
                {
                    openFolderDialog.SelectedPath = SourceFolder;
                }

                var showDialog = openFolderDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    SourceFolder = openFolderDialog.SelectedPath;
                }
            }
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgument e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Current = e.Current;
                Total = e.Total;
                lock (Mutex)
                {
                    File.AppendAllText(_outputFile, $@"{e.Message.Message}{Environment.NewLine}");
                }
            });
        }

    }
}
