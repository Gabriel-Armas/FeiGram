using FeigramClient.Models;
using FeigramClient.Resources;
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
        private RulesValidator _rulesValidator;

        public RegisterAccount(Border Overlay)
        {
            InitializeComponent();
            _rulesValidator = new RulesValidator();
            _rulesValidator.AddLimitToTextBox(FullNameBox, 80);
            _rulesValidator.AddLimitToTextBox(EmailBox, 80);
            _rulesValidator.AddLimitToTextBox(TuitionBox, 9);
            _rulesValidator.AddLimitToPasswordBox(PasswordBox, 50);
            _rulesValidator.EviteDangerLettersInTextbox(FullNameBox);
            _rulesValidator.EviteDangerLettersInTextbox(EmailBox);
            _rulesValidator.EviteDangerLettersInTextbox(TuitionBox);
            _rulesValidator.EviteDangerLettersInPasswordBox(PasswordBox);

            FullNameBox.TextChanged += (_, __) => UpdateRegisterButtonState();
            EmailBox.TextChanged += (_, __) => UpdateRegisterButtonState();
            TuitionBox.TextChanged += (_, __) => UpdateRegisterButtonState();
            PasswordBox.PasswordChanged += (_, __) => UpdateRegisterButtonState();

            RegisterButton.IsEnabled = false;
            _authenticationService = App.Services.GetRequiredService<AuthenticationService>();
            _overlay = Overlay;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            // Validar formatos con RulesValidator
            if (!_rulesValidator.EmailValidator(EmailBox.Text))
            {
                MessageBox.Show("El correo electrónico no es válido. Debe tener formato usuario@dominio.ext, por ejemplo usuario@example.com.",
                    "Correo inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_rulesValidator.PasswordValidator(PasswordBox.Password))
            {
                MessageBox.Show("La contraseña no cumple los requisitos:\n" +
                    "- Entre 8 y 64 caracteres\n" +
                    "- Al menos una letra mayúscula\n" +
                    "- Al menos un número\n" +
                    "- Al menos un carácter especial (por ejemplo !@#$%)",
                    "Contraseña débil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedPhotoPath))
            {
                MessageBox.Show("Debes seleccionar una foto de perfil antes de registrar la cuenta.",
                    "Foto faltante", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("¡Oh no! Ocurrió un error al registrar la cuenta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (success)
            {
                MessageBox.Show("¡Cuenta registrada exitosamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                _overlay.Visibility = Visibility.Collapsed;
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

            UpdateRegisterButtonState();
        }

        private void UpdateRegisterButtonState()
        {
            bool allFieldsFilled =
                !string.IsNullOrWhiteSpace(FullNameBox.Text) &&
                !string.IsNullOrWhiteSpace(EmailBox.Text) &&
                !string.IsNullOrWhiteSpace(TuitionBox.Text) &&
                !string.IsNullOrWhiteSpace(PasswordBox.Password) &&
                !string.IsNullOrWhiteSpace(selectedPhotoPath);

            RegisterButton.IsEnabled = allFieldsFilled;
        }
    }
}
