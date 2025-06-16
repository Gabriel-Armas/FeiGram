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
        public ObservableCollection<UserPostDto> UserPosts { get; } = new();

        public ProfileViewModel(ProfileSingleton me)
        {
            Me = me;
        }
    }

    /// <summary>
    /// Lógica de interacción para Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        private readonly PostsService _postsService;
        private readonly ProfileViewModel _viewModel;


        public Profile(ProfileSingleton profile)
        {
            InitializeComponent();

            _postsService = App.Services.GetRequiredService<PostsService>();
            _viewModel = new ProfileViewModel(profile);
            this.DataContext = _viewModel;

            LoadUserPosts();
        }

        private async void LoadUserPosts()
        {
            try
            {
                _postsService.SetToken(_viewModel.Me.Token ?? "");

                var posts = await _postsService.GetUserPostsAsync(_viewModel.Me.Id);
                _viewModel.UserPosts.Clear();

                foreach (var post in posts)
                    _viewModel.UserPosts.Add(post);
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
    }
}
