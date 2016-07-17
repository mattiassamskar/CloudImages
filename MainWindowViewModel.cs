using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CloudImages
{
    public class MainWindowViewModel : INotifyPropertyChanged, IDropTarget
    {
        public RelayCommand PublishCommand { get; set; }

        public RelayCommand ClearCommand { get; set; }

        public ObservableCollection<string> Files { get; set; }

        public ObservableCollection<string> Albums { get; set; }

        public string SelectedAlbum { get; set; }

        private string _progress;
        public string Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        public MainWindowViewModel()
        {
            Progress = "Publish";
            Files = new ObservableCollection<string>();
            Albums = new ObservableCollection<string>();
            PublishCommand = new RelayCommand(CanExecutePublishCommand, ExecutePublishCommand);
            ClearCommand = new RelayCommand(CanExecuteClearCommand, ExecuteClearCommand);

            UpdateAlbums();

            IsEnabled = true;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var files = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>().ToList();
            files.ForEach(file => Files.Add(file));
        }

        private async void ExecutePublishCommand()
        {
            if (!Files.Any() || string.IsNullOrEmpty(SelectedAlbum))
                return;

            try
            {
                IsEnabled = false;

                await CloudHandler.ResizeAndPublishImagesAsync(SelectedAlbum, Files, new Progress<PublishingStatus>(ReportProgress));
                UpdateAlbums();
                Files.Clear();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Cloud Images", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Progress = "Publish";
                IsEnabled = true;
            }
        }

        private void ReportProgress(PublishingStatus status)
        {
            switch (status)
            {
                case PublishingStatus.Resizing:
                    Progress = "Resizing..";
                    break;
                case PublishingStatus.Publishing:
                    Progress = "Sending..";
                    break;
                case PublishingStatus.Cleaning:
                    Progress = "Cleaning up..";
                    break;
                default:
                    Progress = "Publish";
                    break;
            }
        }

        private void UpdateAlbums()
        {
            IsEnabled = false;

            Albums.Clear();
            CloudHandler.GetAlbumNames().ToList().ForEach(x => Albums.Add(x));

            IsEnabled = true;
        }

        private bool CanExecutePublishCommand()
        {
            return IsEnabled;
        }

        private void ExecuteClearCommand()
        {
            Files.Clear();
        }

        private bool CanExecuteClearCommand()
        {
            return IsEnabled;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
