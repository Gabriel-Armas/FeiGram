using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para ConsultAccount.xaml
    /// </summary>
    public partial class ConsultAccount : Page
    {
        public ObservableCollection<FullUser> ListaDeCuentas { get; set; }
        private ProfileService _profileService;
        private AuthenticationService _authService;
        private ProfileSingleton _me;

        public ConsultAccount(ProfileSingleton me)
        {
            InitializeComponent();
            _me = me;
            ListaDeCuentas = new ObservableCollection<FullUser>();
            DataContext = this;

            Loaded += ConsultAccount_Loaded;
        }

        private async void ConsultAccount_Loaded(object sender, RoutedEventArgs e)
        {
            _profileService = App.Services.GetRequiredService<ProfileService>();
            _profileService.SetToken(_me.Token);

            _authService = App.Services.GetRequiredService<AuthenticationService>();

            await LoadProfilesAsync();
        }

        private async Task LoadProfilesAsync()
        {
            ListaDeCuentas.Clear();

            try
            {
                var perfiles = await _profileService.GetProfilesAsync();
                var authService = App.Services.GetRequiredService<AuthenticationService>();

                var tasks = perfiles.Select(async p =>
                {
                    var response = await authService.GetAccountAsync(p.Id ?? "");

                    MessageBox.Show(p.Tuition);
                    return new FullUser
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Email = response.Email,
                        Tuition = p.Tuition,
                        Role = response.Role?.Trim() ?? "User",
                        Photo = p.Photo
                    };
                });

                var usuariosCompletos = await Task.WhenAll(tasks);

                foreach (var usuario in usuariosCompletos)
                {
                    ListaDeCuentas.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error cargando los perfiles: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddAccount_Click(object sender, RoutedEventArgs e)
        {
            ModalFrame.Navigate(new RegisterAccount(Overlay));
            Overlay.Visibility = Visibility.Visible;
        }


        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cuenta = button?.Tag as FullUser;

            if (cuenta == null)
                return;

            Action cerrarModal = () => Overlay.Visibility = Visibility.Collapsed;

            var editarPage = new EditAccountPage(cuenta, cerrarModal);

            ModalFrame.Navigate(editarPage);
            Overlay.Visibility = Visibility.Visible;
        }

        private void Ban_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var cuenta = boton?.Tag as FullUser;

            if (cuenta == null)
                return;

            Action cerrarModal = () => Overlay.Visibility = Visibility.Collapsed;
            
            var banearPage = new BanAccountPage(cuenta, cerrarModal);

            ModalFrame.Navigate(banearPage);
            Overlay.Visibility = Visibility.Visible;
        }
    }

    public class Cuenta
    {
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Matricula { get; set; }
    }
}
