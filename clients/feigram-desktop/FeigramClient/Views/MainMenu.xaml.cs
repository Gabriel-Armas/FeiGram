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
        public ObservableCollection<Friend> Friends { get; set; } = new();

        private List<PostDto> PostsRecommendations { get; set; }
        private LikesService likesService;

        private ProfileSingleton _me;

        public MainMenu(ProfileSingleton profile)
        {
            InitializeComponent();
            _me = profile;
            likesService = App.Services.GetRequiredService<LikesService>();
            LoadRecommendations();
            LoadFriends();
            DataContext = this;
        }

        private async void LoadFriends()
        {
            try
            {
                var followService = App.Services.GetRequiredService<FollowService>();
                var profileService = App.Services.GetRequiredService<ProfileService>();
                profileService.SetToken(_me.Token);

                var followingIds = await followService.GetFollowingAsync(_me.Id);
                Friends.Clear();
                MessageBox.Show(""+ _me.Id);
                MessageBox.Show("followers" + followingIds.Count);

                foreach (var id in followingIds)
                {
                    var profile = await profileService.GetProfileAsync(id);
                    Friends.Add(new Friend
                    {
                        Name = profile.Name,
                        Id = profile.Id,
                        Photo = profile.Photo,
                        Sex = profile.Sex,
                        Tuition = profile.Tuition,
                        FollowerCount = profile.FollowerCount
                    });
                }
                MessageBox.Show($"Se cargaron {Friends.Count} amigos.");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando amigos: " + ex.Message);
            }
        }

        private void FriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsListBox.SelectedItem is Friend selectedFriend)
            {
                GridMenu.Visibility = Visibility.Collapsed;
                var profilePage = new Profile(_me, false, selectedFriend);
                ModalFrame.Navigate(profilePage);
                ModalOverlay.Visibility = Visibility.Visible;

            }

            // Deseleccionar para que no quede resaltado
            FriendsListBox.SelectedItem = null;
        }

        private async void LoadRecommendations()
        {
            try
            {
                var feedService = App.Services.GetRequiredService<FeedService>();
                var recomendaciones = await feedService.GetRecommendations(_me.Id);
                PostsRecommendations = recomendaciones;
                PostsCompletos.Clear();
                var profileService = App.Services.GetRequiredService<ProfileService>();
                profileService.SetToken(_me.Token);
                foreach (var p in recomendaciones)
                {
                    var profile = await profileService.GetProfileAsync(p.IdUsuario);
                    PostsCompletos.Add(new Post
                    {
                        Id = p.PostId,
                        Username = profile.Name,
                        Description = p.Descripcion,
                        UserProfileImage = profile.Photo,
                        PostImage = p.UrlMedia,
                        TimeAgo = GetTimeAgo(p.FechaPublicacion),
                        Likes = p.Likes,
                        Comentarios = p.Comentarios
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
            var profilePage = new Profile(_me, true);
            ModalFrame.Navigate(profilePage);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_me);
            ModalFrame.Navigate(messagesPage);
            ModalOverlay.Visibility = Visibility.Visible;
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
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_me);
            ModalFrame.Navigate(consultAccounts);
            ModalOverlay.Visibility = Visibility.Visible;
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

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Post post)
            {
                Like like = new Like
                {
                    PostId = post.Id.ToString(),
                    UserId = _me.Id
                };

                likesService = App.Services.GetRequiredService<LikesService>();
                var result = await likesService.CreateLikeAsync(like);

                if (result != null)
                {
                    var img = FindVisualChild<Image>(button);
                    if (img != null)
                    {
                        img.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/megustaActivo.png"));
                    }
                }
                else
                {
                    Console.WriteLine("No se pudo crear el like.");
                }
            }
        }

        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }



        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchBox.Text.Trim();

                if (!string.IsNullOrEmpty(query))
                {
                    var profileService = App.Services.GetRequiredService<ProfileService>();
                    profileService.SetToken(_me.Token);
                    var profile = await profileService.GetByEnrollmentAsync(query);
                    Friend friend = new Friend();
                    friend.Id = profile.Id;
                    friend.Name = profile.Name;
                    friend.Tuition = profile.Enrollment;
                    friend.Photo = profile.Photo;
                    friend.Sex = profile.Sex;
                    friend.FollowerCount = profile.FollowerCount;
                    if (profile != null)
                    {
                        GridMenu.Visibility = Visibility.Collapsed;
                        var profilePage = new Profile(_me, false, friend);
                        ModalFrame.Navigate(profilePage);
                        ModalOverlay.Visibility = Visibility.Visible;
                    }
                }
            }
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
        public string Comentarios { get; set; }
        public string Likes { get; set; }
    }
}
