using System.Windows;

namespace Telhai.DotNet.PlayerProject
{
    public partial class EditSongWindow : Window
    {
        private EditSongViewModel vm;

        public EditSongWindow(MusicTrack track)
        {
            InitializeComponent();
            vm = new EditSongViewModel(track);
            DataContext = vm;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            vm.Save();
            DialogResult = true;
            Close();
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Image Files|*.png;*.jpg;*.jpeg";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == true)
            {
                foreach (var file in ofd.FileNames)
                {
                    vm.Images.Add(file);
                }
            }
        }

        private void BtnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (lstImages.SelectedItem is string imgPath)
            {
                vm.Images.Remove(imgPath);
            }
        }


    }
}
