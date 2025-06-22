using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private Frame _ModalFrame;
        private Grid _ModalOverlay;
        private MainWindow _mainWindow;


        public ConsultAccount(ProfileSingleton me, MainWindow mainWindow, Frame modalFrame, Grid modalOverlay)
        {
            InitializeComponent();
            _me = me;
            _mainWindow = mainWindow;
            _ModalFrame = modalFrame;
            _ModalOverlay = modalOverlay;
            ListaDeCuentas = new ObservableCollection<FullUser>();
            DataContext = this;
            Loaded += ConsultAccount_Loaded;
        }

        private async void ConsultAccount_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _profileService = App.Services.GetRequiredService<ProfileService>();
                _profileService.SetToken(_me.Token);

                _authService = App.Services.GetRequiredService<AuthenticationService>();

                await LoadProfilesAsync();
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar cuentas:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            } 
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
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error cargando los perfiles: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var home = new MainMenu(_me, _mainWindow);
            _ModalFrame.Navigate(home);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_me, _mainWindow, _ModalFrame, _ModalOverlay, true);
            _ModalFrame.Navigate(profilePage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(messagesPage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseSession_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Content = null;
            _mainWindow.GridLogin.Visibility = Visibility.Visible;
            _mainWindow.GridMainMenu.Visibility = Visibility.Hidden;
            _mainWindow.EmailTextBox.Text = "";
            _mainWindow.PasswordBox.Password = "";
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
