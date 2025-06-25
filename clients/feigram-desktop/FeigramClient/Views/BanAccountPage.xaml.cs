using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace FeigramClient.Views
{
    public partial class BanAccountPage : Page
    {
        private FullUser _cuenta;
        private readonly Action cerrarModalCallback;
        private ProfileSingleton _me;

        public BanAccountPage(FullUser cuenta, Action cerrarModal, ProfileSingleton me)
        {
            InitializeComponent();
            _cuenta = cuenta;
            _me = me;
            DataContext = cuenta;
            cerrarModalCallback = cerrarModal;
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user = DataContext as FullUser;

                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    MessageBox.Show("No se pudo obtener la cuenta a banear~", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var authService = App.Services.GetRequiredService<AuthenticationService>();
                authService.SetToken(_me.Token);
                bool success = await authService.BanAsync(user.Email);

                if (success)
                {
                    MessageBox.Show("¡Cuenta baneada con éxito!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("¡No se pudo banear al usuario!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException uaEx)
            {
                MessageBox.Show(uaEx.Message, "Acceso no autorizado", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var login = new MainWindow();
                    login.Show();
                    Window.GetWindow(this)?.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al banear~: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            cerrarModalCallback?.Invoke();
        }
    }
}
