using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace Telhai.DotNet.PlayerProject
{
    public partial class MusicPlayer : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private ItunesService itunes = new ItunesService();
        private CancellationTokenSource? metadataCTS;
        private DispatcherTimer timer = new DispatcherTimer();
        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;
        private const string FILE_NAME = "library.json";
        private DispatcherTimer imageTimer = new DispatcherTimer();
        private int currentImageIndex = 0;


        public MusicPlayer()
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            this.Loaded += MusicPlayer_Loaded;
            imageTimer.Interval = TimeSpan.FromSeconds(3);
            imageTimer.Tick += ImageTimer_Tick;

        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLibrary();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null && mediaPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                sliderProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliderProgress.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        // ================= PLAY =================

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();

                txtCurrentSong.Text = track.Title;
                txtStatus.Text = "Playing";

                DisplayTrackImage(track);
                LoadMetadataAsync(track);
                imageTimer.Start();

            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
            imageTimer.Stop();

        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
            imageTimer.Stop();

        }

        // ================= SLIDER =================

        private void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaPlayer.Volume = sliderVolume.Value;
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(sliderProgress.Value);
        }

        // ================= LIBRARY =================

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    MusicTrack track = new MusicTrack
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(file),
                        FilePath = file
                    };
                    library.Add(track);
                }
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                mediaPlayer.Open(new Uri(track.FilePath));
                mediaPlayer.Play();
                timer.Start();
                imageTimer.Start();


                txtCurrentSong.Text = track.Title;
                txtStatus.Text = "Playing";

                DisplayTrackImage(track);
                LoadMetadataAsync(track);
            }
        }

        private void LstLibrary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                txtCurrentSong.Text = track.Title;
                txtStatus.Text = track.FilePath;
                DisplayTrackImage(track);
            }
        }

        // ================= EDIT =================

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                EditSongWindow win = new EditSongWindow(track);

                if (win.ShowDialog() == true)
                {
                    SaveLibrary();
                    UpdateLibraryUI();

                    txtCurrentSong.Text = $"{track.Title} - {track.Artist}";
                    DisplayTrackImage(track);
                }
            }
            else
            {
                MessageBox.Show("Select a song first");
            }
        }

        // ================= METADATA =================

        private async void LoadMetadataAsync(MusicTrack track)
        {
            if (track.MetadataLoaded)
            {
                txtCurrentSong.Text = $"{track.Title} - {track.Artist}";
                txtStatus.Text = track.Album;
                DisplayTrackImage(track);
                return;
            }

            metadataCTS?.Cancel();
            metadataCTS = new CancellationTokenSource();

            string query = track.Title.Replace("-", " ").Replace("_", " ");
            txtStatus.Text = "Searching online...";

            var data = await itunes.SearchAsync(query, metadataCTS.Token);

            if (data == null)
            {
                txtStatus.Text = track.FilePath;
                txtCurrentSong.Text = track.Title;
                DisplayTrackImage(track);
                return;
            }

            track.Title = data.TrackName;
            track.Artist = data.ArtistName;
            track.Album = data.AlbumName;
            track.ArtworkUrl = data.ArtworkUrl;

            track.MetadataLoaded = true;

            SaveLibrary();

            txtCurrentSong.Text = $"{track.Title} - {track.Artist}";
            txtStatus.Text = track.Album;
            DisplayTrackImage(track);
        }


        // ================= IMAGE LOGIC (THE IMPORTANT PART) =================

        private void DisplayTrackImage(MusicTrack track)
        {
            imageTimer.Stop();
            currentImageIndex = 0;

            if (track.Images != null && track.Images.Count > 0)
            {
                if (File.Exists(track.Images[0]))
                    imgCover.Source = new BitmapImage(new Uri(track.Images[0]));
                else
                    imgCover.Source = new BitmapImage(new Uri(track.Images[0], UriKind.Absolute));

                if (track.Images.Count > 1)
                    imageTimer.Start();

                return;
            }

            if (!string.IsNullOrEmpty(track.ArtworkUrl))
            {
                imgCover.Source = new BitmapImage(new Uri(track.ArtworkUrl));
                return;
            }

            SetDefaultImage();
        }


        private void SetDefaultImage()
        {
            imgCover.Source = new BitmapImage(
                new Uri("pack://application:,,,/Telhai.DotNet.PlayerProject;component/Assets/default.png", UriKind.Absolute));
        }

        // ================= SAVE/LOAD =================

        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        private void SaveLibrary()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(library, options);
            File.WriteAllText(FILE_NAME, json);
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json) ?? new List<MusicTrack>();
                UpdateLibraryUI();
            }
        }

        private void ImageTimer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source == null)
                return;

            if (mediaPlayer.CanPause == false)
                return;

            if (lstLibrary.SelectedItem is MusicTrack track && track.Images != null && track.Images.Count > 1)
            {
                currentImageIndex++;

                if (currentImageIndex >= track.Images.Count)
                    currentImageIndex = 0;

                imgCover.Source = new BitmapImage(new Uri(track.Images[currentImageIndex]));
            }
        }






    }
}


    //private void MusicPlayer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    //{
    //    MainWindow p = new MainWindow();
    //    p.Title = "YYYYY";
    //    p.Show();
    //}
