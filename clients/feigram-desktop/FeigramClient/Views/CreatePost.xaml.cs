using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para CreatePost.xaml
    /// </summary>
    public partial class CreatePost : Page
    {
        private string? selectedImagePath;

        public CreatePost()
        {
            InitializeComponent();
        }
        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                selectedImagePath = dialog.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(selectedImagePath));
                PreviewImage.Visibility = Visibility.Visible;
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            selectedImagePath = null;
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Cierra esta Page, regresa al menú o limpia campos
        }

        private void Publish_Click(object sender, RoutedEventArgs e)
        {
            var description = DescriptionBox.Text;
            var imagePath = selectedImagePath;

            // Aquí puedes subir la imagen y descripción a tu backend
        }
    }
}
