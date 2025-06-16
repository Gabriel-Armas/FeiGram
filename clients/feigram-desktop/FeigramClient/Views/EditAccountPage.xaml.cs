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
using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

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

        public EditAccountPage(FullUser cuenta, Action cerrarModal)
        {
            InitializeComponent();
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
            MessageBox.Show(_cuenta.Photo ?? "No hay foto de perfil");

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
            var profileService = App.Services.GetRequiredService<ProfileService>();

            string userId = _cuenta.Id ?? "";

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("No se encontró el ID del usuario");
                return;
            }

            bool success;
            try
            {
                success = await profileService.EditAsync(
                    userId,
                    _cuenta.Name,
                    _cuenta.Tuition,
                    selectedPhotoPath
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show($"¡Error al editar el perfil!\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (success)
            {
                MessageBox.Show("¡Perfil actualizado con éxito!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                cerrarModalCallback?.Invoke();
            }
            else
            {
                MessageBox.Show("¡No se pudo actualizar el perfil!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            cerrarModalCallback?.Invoke();
        }

    }
}
