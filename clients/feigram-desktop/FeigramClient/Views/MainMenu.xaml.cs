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
    /// Lógica de interacción para MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public ObservableCollection<Post> PostsCompletos { get; set; } = new();
        private List<PostDto> PostsRecommendations { get; set; }
        private ProfileSingleton _me;

        public MainMenu(ProfileSingleton profile)
        {
            InitializeComponent();
            _me = profile;
            LoadRecommendations();
            DataContext = this;
        }

        private async void LoadRecommendations()
        {
            try
            {
                var feedService = App.Services.GetRequiredService<FeedService>();
                var recomendaciones = await feedService.GetRecommendations(_me.Id);
                PostsRecommendations = recomendaciones;
                PostsCompletos.Clear();
                foreach (var p in recomendaciones)
                {
                    PostsCompletos.Add(new Post
                    {
                        Id = p.PostId,
                        Username = _me.Name,
                        Description = p.Descripcion,
                        UserProfileImage = _me.Photo,
                        PostImage = p.UrlMedia,
                        TimeAgo = GetTimeAgo(p.FechaPublicacion)
                    });

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando recomendaciones: " + ex.Message);
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

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            //home
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_me);
            ModalFrame.Navigate(profilePage);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            //messages
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_me);
            ModalFrame.Navigate(consultAccounts);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            //stadistics
        }

        private void CloseSession_Click(object sender, RoutedEventArgs e)
        {
            /*var login = new MainWindow();
            login.Show();
            this.Close();*/
        }

        public void CloseModal()
        {
            ModalOverlay.Visibility = Visibility.Collapsed;
            ModalFrame.Content = null;
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                //GridMenu.Visibility = Visibility.Collapsed;
                var consultPost = new ConsultPost(post, ModalOverlay, _me);
                ModalFrame.Navigate(consultPost);
                ModalOverlay.Visibility = Visibility.Visible;
            }
        }

        private void AddPost_Click(object sender, RoutedEventArgs e)
        {
            var addpost = new CreatePost(ModalOverlay, _me);
            ModalFrame.Navigate(addpost);
            ModalOverlay.Visibility = Visibility.Visible;
        }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string TimeAgo { get; set; }
        public string Description { get; set; }
        public string UserProfileImage { get; set; }
        public string PostImage { get; set; }
    }
}
