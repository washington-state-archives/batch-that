using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using BatchThat.Image;
using BatchThat.Image.Enums;
using BatchThat.Image.EventArguments;
using Application = System.Windows.Application;

namespace BatchThat.ViewModels
{
    public class BulkImageProcessorViewModel : ImageProcessorViewModel
    {
        private string _sourceFolder;
        private string _destinationFolder;
        private bool _includeSubfolders;
        private EnumMessageType _currentFilter;

        private List<ChangedEventMessage> ChangedEvents { get; set; }
        public BindingList<ChangedEventMessage> Log { get; set; }
        protected ImageManager ImageManager { get; }
        public string SourceFolder
        {
            get => _sourceFolder;
            set { _sourceFolder = value; OnPropertyChanged(GetPropertyInfo(this, x => x.SourceFolder).Name); }
        }

        public string DestinationFolder
        {
            get => _destinationFolder;
            set { _destinationFolder = value; OnPropertyChanged(GetPropertyInfo(this, x => x.DestinationFolder).Name); }
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;
            set { _includeSubfolders = value; OnPropertyChanged(GetPropertyInfo(this, x => x.IncludeSubfolders).Name); }
        }

        public ICommand BrowseSourceFolderCommand { get; set; }
        public ICommand BrowseDestinationFolderCommand { get; set; }
        public ICommand ApplyFiltersCommand { get; set; }
        public ICommand SaveImageCommand { get; set; }
        public ICommand FilterAllMessages { get; set; }

        public BulkImageProcessorViewModel()
        {
            _currentFilter = EnumMessageType.All;
            ChangedEvents = new List<ChangedEventMessage>();
            Log = new BindingList<ChangedEventMessage>();
            Enabled = true;
            ImageManager = new ImageManager();
            BrowseSourceFolderCommand = new RelayCommand(BrowseSourceFolder);
            BrowseDestinationFolderCommand = new RelayCommand(BrowseDestinationFolder);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            FilterAllMessages = new RelayCommand(FilterMessages);
        }

        private void FilterMessages(object parameter)
        {
            var param = parameter.ToString();
            _currentFilter = (EnumMessageType) Enum.Parse(typeof(EnumMessageType), param);
            switch (_currentFilter)
            {
                case EnumMessageType.All:
                    Log.Clear();
                    foreach (var message in ChangedEvents)
                    {
                        Log.Add(message);
                    }
                    break;
                case EnumMessageType.Error:
                    Log.Clear();
                    foreach (var message in ChangedEvents.Where(x => x.MessageType == EnumMessageType.Error))
                    {
                        Log.Add(message);
                    }
                    break;
                case EnumMessageType.Informational:
                    Log.Clear();
                    foreach (var message in ChangedEvents.Where(x => x.MessageType == EnumMessageType.Informational))
                    {
                        Log.Add(message);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        private void BrowseDestinationFolder(object obj)
        {
            var openFolderDialog = new FolderBrowserDialog();
            if (string.IsNullOrWhiteSpace(DestinationFolder))
            {
                var showDialog = openFolderDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    DestinationFolder = openFolderDialog.SelectedPath;
                }
            }
            else
            {
                if (Directory.Exists(Path.GetDirectoryName(DestinationFolder)))
                {
                    openFolderDialog.SelectedPath = DestinationFolder;
                }

                var showDialog = openFolderDialog.ShowDialog();
                if (showDialog == DialogResult.OK)
                {
                    DestinationFolder = openFolderDialog.SelectedPath;
                }
            }
        }

        private async void ApplyFilters(object obj)
        {
            ChangedEvents.Clear();
            Log.Clear();
            Enabled = false;
            ImageManager.ProgressChanged += ProgressChanged;
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(SourceFolder, "*", IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => !x.EndsWith(".db"));
                ImageManager.ApplyFilters(GetFilters(), files.ToList(), DestinationFolder, SourceFolder);
            });
            Enabled = true;
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgument e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                Current = e.Current;
                Total = e.Total;
                ChangedEvents.Add(e.Message);
                if (_currentFilter == EnumMessageType.All || _currentFilter == e.Message.MessageType)
                    Log.Add(e.Message);
            });
        }
    }
}
