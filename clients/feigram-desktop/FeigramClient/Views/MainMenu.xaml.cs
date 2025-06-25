using FeigramClient.Models;
using FeigramClient.Resources;
using FeigramClient.Services;
using Google.Protobuf.Collections;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Lógica de interacción para MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public ObservableCollection<Post> PostsCompletos { get; set; } = new();
        public ObservableCollection<Friend> Friends { get; set; } = new();
        private MainWindow _mainWindow;
        private List<PostDto> PostsRecommendations { get; set; }
        private LikesService likesService;
        private bool _isLoadingRecommendations = false;
        private ProfileSingleton _me;
        private RulesValidator _rulesValidator;

        public MainMenu(ProfileSingleton profile, MainWindow mainWindow)
        {
            InitializeComponent();
            _me = profile;
            _mainWindow = mainWindow;
            _rulesValidator = new RulesValidator();
            _rulesValidator.EviteDangerLettersInTextbox(SearchBox);
            _rulesValidator.AddLimitToTextBox(SearchBox, 80);
            likesService = App.Services.GetRequiredService<LikesService>();

            _isLoadingRecommendations = true;
            LoadRecommendations();
            LoadFriends();
            DataContext = this;
        }

        private async void LoadFriends()
        {
            try
            {
                var followService = App.Services.GetRequiredService<FollowService>();
                followService.SetToken(_me.Token);
                var profileService = App.Services.GetRequiredService<ProfileService>();
                profileService.SetToken(_me.Token);

                var followingIds = await followService.GetFollowingAsync(_me.Id);
                Friends.Clear();

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

            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var profilePage = new Profile(_me, _mainWindow, ModalFrame, ModalOverlay, false, selectedFriend, true);
                ModalFrame.Navigate(profilePage);
                ModalOverlay.Visibility = Visibility.Visible;

            }

            FriendsListBox.SelectedItem = null;
        }

        private async void LoadRecommendations()
        {
            if (PostsCompletos.Count > 0)
            {
                _isLoadingRecommendations = false;
                return;
            }

            try
            {
                var feedService = App.Services.GetRequiredService<FeedService>();
                feedService.SetToken(_me.Token);
                var recomendaciones = await feedService.GetRecommendations(_me.Id);
                PostsRecommendations = recomendaciones;
                PostsCompletos.Clear();
                var profileService = App.Services.GetRequiredService<ProfileService>();
                profileService.SetToken(_me.Token);
                var likesService = App.Services.GetRequiredService<LikesService>();
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
                        Comentarios = p.Comentarios,
                        IsLiked = await likesService.CheckIfUserLikedPostAsync(_me.Id, p.PostId.ToString())
                    });
                }
                _isLoadingRecommendations = false; 
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando recomendaciones: " + ex.Message);
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;

            if (scrollViewer != null &&
                scrollViewer.VerticalOffset + scrollViewer.ViewportHeight >= scrollViewer.ExtentHeight &&
                !_isLoadingRecommendations)
            {
                _isLoadingRecommendations = true;
                LoadRecommendations();
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
            GridMenu.Visibility = Visibility.Collapsed;
            var home = new MainMenu(_me, _mainWindow);
            ModalFrame.Navigate(home);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_me, _mainWindow, ModalFrame, ModalOverlay, true);
            ModalFrame.Navigate(profilePage);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_me, _mainWindow, ModalFrame, ModalOverlay);
            ModalFrame.Navigate(messagesPage);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_me, _mainWindow , ModalFrame, ModalOverlay);
            ModalFrame.Navigate(consultAccounts);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_me, _mainWindow, ModalFrame, ModalOverlay);
            ModalFrame.Navigate(consultAccounts);
            ModalOverlay.Visibility = Visibility.Visible;
        }

        private void CloseSession_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Content = null;
            _mainWindow.GridLogin.Visibility = Visibility.Visible;
            _mainWindow.GridMainMenu.Visibility = Visibility.Hidden;
            _mainWindow.EmailTextBox.Text = "";
            _mainWindow.PasswordBox.Password = "";
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
            try
            {
                if (sender is Button button && button.DataContext is Post post)
                {
                    likesService = App.Services.GetRequiredService<LikesService>();

                    if (post.IsLiked)
                    {
                        bool success = await likesService.DeleteLikeAsync(_me.Id, post.Id.ToString());
                        if (success)
                        {
                            post.Likes--;
                            post.IsLiked = false;

                            var img = FindVisualChild<Image>(button);
                            if (img != null)
                            {
                                img.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/megusta.png"));
                            }
                            
                        }
                    }
                    else
                    {
                        Like like = new Like
                        {
                            PostId = post.Id.ToString(),
                            UserId = _me.Id
                        };

                        var result = await likesService.CreateLikeAsync(like);

                        if (result != null)
                        {
                            post.Likes++;
                            post.IsLiked = true;

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
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al dar like:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var profileService = App.Services.GetRequiredService<ProfileService>();
            profileService.SetToken(_me.Token);

            string searchText = SearchBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchPopup.IsOpen = false;
                return;
            }

            try
            {
                var results = await profileService.SearchProfilesByNameAsync(searchText);
                List<ProfileWithFollowerCount> filteredResults;

                if (results != null)
                {
                    filteredResults = results
                        .Where(p => p.Id != _me.Id) // Excluirme a mí mismo
                        .ToList();
                }
                else
                {
                    filteredResults = new List<ProfileWithFollowerCount>();
                }

                if (filteredResults.Any())
                {
                    SearchResultsListBox.ItemsSource = filteredResults;
                }
                else
                {
                    SearchResultsListBox.ItemsSource = new List<ProfileWithFollowerCount>
            {
                new ProfileWithFollowerCount { Name = "Sin resultados", Enrollment = "" }
            };
                }

                SearchPopup.IsOpen = true;
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en búsqueda: " + ex.Message);
                SearchPopup.IsOpen = false;
            }
        }



        private void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResultsListBox.SelectedItem is ProfileWithFollowerCount profile)
            {
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
                    var profilePage = new Profile(_me, _mainWindow, ModalFrame, ModalOverlay, false, friend);
                    ModalFrame.Navigate(profilePage);
                    ModalOverlay.Visibility = Visibility.Visible;
                }
                SearchPopup.IsOpen = false;
            }
        }

    }

    public class Post : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string TimeAgo { get; set; }
        public string Description { get; set; }
        public string UserProfileImage { get; set; }
        public string PostImage { get; set; }
        public int Comentarios { get; set; }

        private int likes;
        public int Likes
        {
            get => likes;
            set
            {
                if (likes != value)
                {
                    likes = value;
                    OnPropertyChanged(nameof(Likes));
                }
            }
        }

        private bool isLiked;
        public bool IsLiked
        {
            get => isLiked;
            set
            {
                if (isLiked != value)
                {
                    isLiked = value;
                    OnPropertyChanged(nameof(IsLiked));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
