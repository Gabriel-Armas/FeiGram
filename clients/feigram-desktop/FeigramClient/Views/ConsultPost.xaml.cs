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

        public ConsultPost(Post post, Grid overlay, ProfileSingleton profile, Friend? friend = null)
        {
            InitializeComponent();
            _me = profile;
            this._overlay = overlay;
            _post = post;
            likesService = App.Services.GetRequiredService<LikesService>();
            this.DataContext = _post;
            postService = App.Services.GetRequiredService<PostsService>();
            postService.SetToken(_me.Token);

            Loaded += async (s, e) =>
            {
                ChatMessages.Height = PostContainer.ActualHeight;
                await LoadComments();
            };
        }

        private async Task LoadComments()
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

        private async void LikeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_post.IsLiked)
            {
                // Descomenta esto si deseas permitir quitar el like
                /*
                bool success = await likesService.UnlikePostAsync(_me.Id, _post.Id.ToString());
                if (success)
                {
                    _post.Likes--;
                    _post.IsLiked = false;

                    ImgLike.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/megusta.png"));
                }
                */
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


        private async void Send_Click(object sender, RoutedEventArgs e)
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
                MessageInput.Clear();
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
