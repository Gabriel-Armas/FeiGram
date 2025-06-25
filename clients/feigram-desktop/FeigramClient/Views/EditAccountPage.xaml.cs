using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using System.IO;
using System.IO.Pipes;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para EditAccountPage.xaml
    /// </summary>
    public partial class EditAccountPage : Page
    {
        private FullUser _cuenta;
        private readonly Action cerrarModalCallback;
        private string? selectedPhotoPath = null;
        private ProfileService _profileService;
        private ProfileSingleton _me;

        public EditAccountPage(ProfileSingleton me, FullUser cuenta, Action cerrarModal)
        {
            InitializeComponent();
            _me = me;
            _cuenta = cuenta;
            DataContext = cuenta;
            LoadData();
            cerrarModalCallback = cerrarModal;
        }

        private void LoadData()
        {
            FullNameBox.Text = _cuenta.Name;
            EmailBox.Text = _cuenta.Email;
            TuitionBox.Text = _cuenta.Tuition;

            if (!string.IsNullOrWhiteSpace(_cuenta.Photo))
            {
                try
                {
                    if (Uri.TryCreate(_cuenta.Photo, UriKind.Absolute, out var uri))
                    {
                        BitmapImage imagen = new BitmapImage(uri);
                        SelectedImage.Source = imagen;
                    }
                    else
                    {
                        MessageBox.Show("La URL de la imagen es inválida: " + _cuenta.Photo);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la imagen del perfil: " + ex.Message);
                }
            }
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

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            var _profileService = App.Services.GetRequiredService<ProfileService>();
            _profileService.SetToken(_me.Token);

            string userId = _cuenta.Id ?? "";

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("No se encontró el ID del usuario");
                return;
            }

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(FullNameBox.Text), "Name");
            form.Add(new StringContent(TuitionBox.Text), "Enrollment");
            form.Add(new StringContent("Male"), "Sex");

            FileStream? fileStream = null;

            if (!string.IsNullOrEmpty(selectedPhotoPath) && File.Exists(selectedPhotoPath))
            {
                fileStream = File.OpenRead(selectedPhotoPath);
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                form.Add(fileContent, "Photo", Path.GetFileName(selectedPhotoPath));
            }

            bool success = false;
            try
            {
                var response = await _profileService.EditAsync(userId, form);

                success = response;
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                MessageBox.Show(unauthEx.Message, "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var login = new MainWindow();
                    login.Show();
                    Window.GetWindow(this)?.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"¡Error al editar el perfil!\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                    fileStream = null;
                }
            }

            if (success)
            {
                MessageBox.Show("¡Perfil actualizado con éxito", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                cerrarModalCallback?.Invoke();
            }
            else
            {
                MessageBox.Show("¡No se pudo actualizar el perfil", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            cerrarModalCallback?.Invoke();
        }

    }
}
