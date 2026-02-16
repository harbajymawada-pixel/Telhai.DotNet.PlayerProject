using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace Telhai.DotNet.PlayerProject
{
    public class EditSongViewModel : INotifyPropertyChanged
    {
        private MusicTrack _track;

        public EditSongViewModel(MusicTrack track)
        {
            _track = track;
            Images = new ObservableCollection<string>(_track.Images ?? new List<string>());
        }

        public string Title
        {
            get => _track.Title;
            set
            {
                _track.Title = value;
                OnPropertyChanged();
            }
        }

        public string Artist
        {
            get => _track.Artist;
            set
            {
                _track.Artist = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Images { get; set; }

        public void Save()
        {
            _track.Images = Images.ToList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
