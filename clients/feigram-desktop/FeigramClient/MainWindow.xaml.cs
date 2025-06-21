using FeigramClient.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FeigramClient.Services;
using FeigramClient.Models;

namespace FeigramClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ProfileSingleton _profileSingleton;
        public MainWindow()
        {
            InitializeComponent();
            _profileSingleton = new ProfileSingleton();
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;

            var authService = App.Services.GetRequiredService<AuthenticationService>();

            try
            {
                LoginResponse? loginresponse = await authService.LoginAsync(email, password);

                if (loginresponse != null && !string.IsNullOrEmpty(loginresponse.Token))
                {
                    var profileService = App.Services.GetRequiredService<ProfileService>();
                    profileService.SetToken(loginresponse.Token);
                    var profile = await profileService.GetProfileAsync(loginresponse.UserId);
                    if (profile != null)
                    {
                        _profileSingleton.Id = profile.Id;
                        _profileSingleton.Name = profile.Name;
                        _profileSingleton.Email = EmailTextBox.Text;
                        _profileSingleton.Sex = profile.Sex;
                        _profileSingleton.Photo = profile.Photo;
                        _profileSingleton.FollowerCount = profile.FollowerCount;
                        _profileSingleton.Role = loginresponse.Rol;
                        _profileSingleton.Token = loginresponse.Token;
                        MainFrame.Navigate(new MainMenu(_profileSingleton, this));
                        GridMainMenu.Visibility = Visibility.Visible;
                        GridLogin.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        MessageBox.Show("No se pudo obtener el perfil.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Correo o contraseña incorrectos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}