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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml.Linq;
using FeigramClient.Resources;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para ConsultPost.xaml
    /// </summary>
    public partial class ConsultPost : Page
    {
        private Post _post;
        private Grid _overlay;
        private ProfileSingleton _me;
        private PostsService postService;
        private CommentsService commentsService;
        private LikesService likesService;
        private RulesValidator _rulesValidator;

        public ConsultPost(Post post, Grid overlay, ProfileSingleton profile, Friend? friend = null)
        {
            InitializeComponent();
            _me = profile;
            _rulesValidator = new RulesValidator();
            _rulesValidator.AddLimitToTextBox(MessageInput,150);
            this._overlay = overlay;
            _post = post;
            likesService = App.Services.GetRequiredService<LikesService>();
            this.DataContext = _post;
            postService = App.Services.GetRequiredService<PostsService>();
            postService.SetToken(_me.Token);

            Loaded += async (s, e) =>
            {
                ChatMessages.Height = PostContainer.ActualHeight;

                _post.IsLiked = await likesService.CheckIfUserLikedPostAsync(_me.Id, _post.Id.ToString());

                ImgLike.Source = new BitmapImage(new Uri(_post.IsLiked
                    ? "pack://application:,,,/Resources/megustaActivo.png"
                    : "pack://application:,,,/Resources/megusta.png"));

                await LoadComments();
            };
        }

        private async Task LoadComments()
        {
            try
            {
                ChatMessages.Items.Clear();
                var comments = await postService.GetCommentsAsync(_post.Id);

                commentsService = App.Services.GetRequiredService<CommentsService>();

                foreach (var comment in comments)
                {
                    ProfileDto profile = await commentsService.GetProfileByIdAsync(comment.user_id);
                    var panel = new StackPanel();
                    panel.Margin = new Thickness(0, 0, 0, 10);

                    var user = new TextBlock
                    {
                        Text = profile.Name,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.DodgerBlue
                    };

                    var message = new TextBlock
                    {
                        Text = comment.text_comment,
                        TextWrapping = TextWrapping.Wrap
                    };

                    var time = new TextBlock
                    {
                        Text = comment.created_at.ToString("g"),
                        FontSize = 10,
                        Foreground = Brushes.Gray
                    };

                    panel.Children.Add(user);
                    panel.Children.Add(message);
                    panel.Children.Add(time);

                    ChatMessages.Items.Add(panel);
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException unauthEx)
            {
                MessageBox.Show(unauthEx.Message, "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var login = new MainWindow();
                    login.Show();
                    Window.GetWindow(this)?.Close();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al consultar post:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_post.IsLiked)
                {

                    bool success = await likesService.DeleteLikeAsync(_me.Id, _post.Id.ToString());
                    if (success)
                    {
                        _post.Likes--;
                        _post.IsLiked = false;

                        ImgLike.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/megusta.png"));
                    }
                }
                else
                {
                    Like like = new Like
                    {
                        PostId = _post.Id.ToString(),
                        UserId = _me.Id
                    };

                    var result = await likesService.CreateLikeAsync(like);

                    if (result != null)
                    {
                        _post.Likes++;
                        _post.IsLiked = true;

                        ImgLike.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/megustaActivo.png"));
                    }
                    else
                    {
                        Console.WriteLine("No se pudo crear el like.");
                    }
                }

                LikesCount.Text = _post.Likes.ToString();
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


        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var input = MessageInput.Text;
                Comment comment = new Comment();
                comment.TextComment = input;
                comment.PostId = _post.Id.ToString();
                comment.UserId = _me.Id;

                commentsService = App.Services.GetRequiredService<CommentsService>();
                await commentsService.AddCommentAsync(comment);
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var panel = new StackPanel();
                    panel.Margin = new Thickness(0, 0, 0, 10);

                    var user = new TextBlock
                    {
                        Text = _me.Name,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.DodgerBlue
                    };

                    var message = new TextBlock
                    {
                        Text = input,
                        TextWrapping = TextWrapping.Wrap
                    };

                    var time = new TextBlock
                    {
                        Text = comment.CreatedAt.ToString("g"),
                        FontSize = 10,
                        Foreground = Brushes.Gray
                    };

                    panel.Children.Add(user);
                    panel.Children.Add(message);
                    panel.Children.Add(time);

                    ChatMessages.Items.Add(panel);
                    _post.Comentarios++;
                    MessageInput.Clear();
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Error de HTTP: {httpEx.Message}",
                                "Error de comunicación", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al comentar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _overlay.Visibility = Visibility.Collapsed;
            var frame = _overlay.Children.OfType<Frame>().FirstOrDefault();
            if (frame != null)
            {
                frame.Content = null;
            }
        }
    }
}
