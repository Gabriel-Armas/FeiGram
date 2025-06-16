using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
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


        public Profile(ProfileSingleton profile, bool isOwnProfile = false, Friend? friend = null)
        {
            InitializeComponent();
            _friend = friend;
            this.isOwnProfile = isOwnProfile;
            _postsService = App.Services.GetRequiredService<PostsService>();
            _viewModel = new ProfileViewModel(profile, friend);
            this.DataContext = _viewModel;
            if (isOwnProfile)
            {
                btnFollow.Visibility = isOwnProfile ? Visibility.Collapsed : Visibility.Visible;
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
            if (!this.isOwnProfile)
            {
                var followService = App.Services.GetRequiredService<FollowService>();
                await followService.FollowUserAsync(_viewModel.Me.Id, "usuarioAseguirId");
            }
        }
    }
}
