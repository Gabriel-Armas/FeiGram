using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace FeigramClient.Views
{
    public class ProfileViewModel
    {
        public ProfileSingleton Me { get; }
        public Friend? Friend { get; }

        public ObservableCollection<UserPostDto> UserPosts { get; } = new();
        public string Name => Friend?.Name ?? Me.Name;
        public string Photo => Friend?.Photo ?? Me.Photo;
        public ProfileViewModel(ProfileSingleton me, Friend? friend = null)
        {
            Me = me;
            Friend = friend;
        }
    }

    /// <summary>
    /// Lógica de interacción para Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        private readonly PostsService _postsService;
        private readonly ProfileViewModel _viewModel;
        public bool isOwnProfile;
        private Friend _friend;
        private Frame _ModalFrame;
        private Grid _ModalOverlay;
        private MainWindow _mainWindow;
        private bool _following;

        public Profile(ProfileSingleton profile, MainWindow mainWindow, Frame modalFrame, Grid modalOverlay, bool isOwnProfile = false, Friend? friend = null, bool following = false)
        {
            InitializeComponent();
            _friend = friend;
            _following = following;
            this.isOwnProfile = isOwnProfile;
            _postsService = App.Services.GetRequiredService<PostsService>();
            _viewModel = new ProfileViewModel(profile, friend);
            _mainWindow = mainWindow;
            _ModalFrame = modalFrame;
            _ModalOverlay = modalOverlay;
            this.DataContext = _viewModel;
            if (_friend != null && following == true)
            {
                btnFollow.Content = "Siguiendo";
                btnFollow.IsEnabled = false;
            }
            else if (_friend != null)
            {

            }
            if (isOwnProfile)
            {
                btnFollow.Visibility = isOwnProfile ? Visibility.Collapsed : Visibility.Visible;
                btnFollow.IsEnabled = false;
            }
            LoadUserPosts();
        }

        private async void LoadUserPosts()
        {
            try
            {
                _postsService.SetToken(_viewModel.Me.Token ?? "");

                if (_friend != null)
                {
                    var posts = await _postsService.GetUserPostsAsync(_friend.Id);
                    _viewModel.UserPosts.Clear();

                    foreach (var post in posts)
                        _viewModel.UserPosts.Add(post);
                }
                else
                {
                    var posts = await _postsService.GetUserPostsAsync(_viewModel.Me.Id);
                    _viewModel.UserPosts.Clear();

                    foreach (var post in posts)
                        _viewModel.UserPosts.Add(post);
                }
                
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar publicaciones: {ex.Message}");
            }
        }

        private void PostImage_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UserPostDto post)
            {
                if (_friend != null)
                {
                    Post postButton = new Post();
                    postButton.Id = post.PostId;
                    postButton.Username = _friend.Name;
                    postButton.UserProfileImage = _friend.Photo;
                    postButton.Description = post.Descripcion;
                    postButton.PostImage = post.UrlMedia;
                    postButton.TimeAgo = GetTimeAgo(post.FechaPublicacion);

                    var consultPost = new ConsultPost(postButton, ModalOverlay, _viewModel.Me, _friend);
                    ModalFrame.Navigate(consultPost);
                    ModalOverlay.Visibility = Visibility.Visible;
                }
                else
                {
                    Post postButton = new Post();
                    postButton.Id = post.PostId;
                    postButton.Username = _viewModel.Me.Name;
                    postButton.UserProfileImage = _viewModel.Me.Photo;
                    postButton.Description = post.Descripcion;
                    postButton.PostImage = post.UrlMedia;
                    postButton.TimeAgo = GetTimeAgo(post.FechaPublicacion);

                    var consultPost = new ConsultPost(postButton, ModalOverlay, _viewModel.Me);
                    ModalFrame.Navigate(consultPost);
                    ModalOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var home = new MainMenu(_viewModel.Me, _mainWindow);
            _ModalFrame.Navigate(home);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_viewModel.Me, _mainWindow, _ModalFrame, _ModalOverlay, true);
            _ModalFrame.Navigate(profilePage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_viewModel.Me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(messagesPage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_viewModel.Me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_viewModel.Me, _mainWindow, _ModalFrame, _ModalOverlay);
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
        private string GetTimeAgo(DateTime fecha)
        {
            var diff = DateTime.Now - fecha;

            if (diff.TotalMinutes < 60)
                return $"Hace {Math.Floor(diff.TotalMinutes)} minutos";
            else if (diff.TotalHours < 24)
                return $"Hace {Math.Floor(diff.TotalHours)} horas";
            else
                return $"Hace {Math.Floor(diff.TotalDays)} días";
        }

        private async void Follow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.isOwnProfile)
                {
                    var followService = App.Services.GetRequiredService<FollowService>();
                    await followService.FollowUserAsync(_viewModel.Me.Id, _friend.Id);
                    btnFollow.Content = "Siguiendo";
                    btnFollow.Visibility = Visibility.Collapsed;
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al seguir:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
