using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Shapes;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para UnbanPage.xaml
    /// </summary>
    public partial class UnbanAccountPage : Page
    {
        private readonly FullUser _cuenta;
        private readonly Action _cerrar;

        public UnbanAccountPage(FullUser cuenta, Action cerrar)
        {
            InitializeComponent();
            _cuenta = cuenta;
            _cerrar = cerrar;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cerrar();
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authService = App.Services.GetRequiredService<AuthenticationService>();
                bool success = await authService.UnbanUserAsync(_cuenta.Email ?? "");

                if (success)
                {
                    MessageBox.Show("Usuario desbaneado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    _cerrar();
                }
                else
                {
                    MessageBox.Show("No se pudo desbanear al usuario", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error HTTP: {httpEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al desbanear:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
