using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para RegisterAccount.xaml
    /// </summary>
    public partial class RegisterAccount : Page
    {
        private Border _overlay;
        private AuthenticationService _authenticationService;
        private string? selectedPhotoPath = null;
        public RegisterAccount(Border Overlay)
        {
            InitializeComponent();
            _authenticationService = App.Services.GetRequiredService<AuthenticationService>();
            _overlay = Overlay;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPhotoPath))
            {
                return;
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(FullNameBox.Text), "Username");
            form.Add(new StringContent(EmailBox.Text), "Email");
            form.Add(new StringContent(PasswordBox.Password), "Password");
            form.Add(new StringContent(TuitionBox.Text), "Enrollment");

            using var fileStream = File.OpenRead(selectedPhotoPath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            form.Add(fileContent, "Photo", Path.GetFileName(selectedPhotoPath));

            bool success = false;
            try
            {
                success = await _authenticationService.RegisterAsync(form);
            }
            catch (Exception ex)
            {
                MessageBox.Show("¡Oh no! Ocurrió un error al registrar la cuenta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (success)
            {
                MessageBox.Show("¡Cuenta registrada exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("¡Oh no! Ocurrió un error al registrar la cuenta. Por favor, inténtalo de nuevo.");
            }
        }



        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _overlay.Visibility = Visibility.Collapsed;
        }

        private void SelectPhoto_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Imágenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                selectedPhotoPath = dialog.FileName;

                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.UriSource = new Uri(selectedPhotoPath);
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.EndInit();

                SelectedImage.Source = imagen;
            }
        }
    }
}
