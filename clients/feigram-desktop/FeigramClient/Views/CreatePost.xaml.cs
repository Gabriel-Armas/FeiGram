﻿using FeigramClient.Models;
using FeigramClient.Resources;
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

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para CreatePost.xaml
    /// </summary>
    public partial class CreatePost : Page
    {
        private string? selectedImagePath;
        private Grid _overlay;
        private ProfileSingleton _me;
        private PostsService postService;
        private RulesValidator _rulesValidator;
        private MainWindow _mainWindow;

        public CreatePost(Grid overlay, ProfileSingleton profile)
        {
            InitializeComponent();
            _overlay = overlay;
            _rulesValidator = new RulesValidator();
            _rulesValidator.AddLimitToTextBox(DescriptionBox, 200);
            _rulesValidator.EviteDangerLettersInTextbox(DescriptionBox);
            _me = profile;
            postService = App.Services.GetRequiredService<PostsService>();
            postService.SetToken(_me.Token);
            PublishButton.IsEnabled = false;
        }
        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagenes (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                selectedImagePath = dialog.FileName;
                PreviewImage.Source = new BitmapImage(new Uri(selectedImagePath));
                PreviewImage.Visibility = Visibility.Visible;
                PublishButton.IsEnabled = true;
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            selectedImagePath = null;
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
            PublishButton.IsEnabled = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _overlay.Visibility = Visibility.Collapsed;
            var frame = _overlay.Children.OfType<Frame>().FirstOrDefault();
            if (frame != null)
            {
                frame.Content = null;
            }
        }

        private async void Publish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? imageUrl = null;

                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    var uploadResult = await postService.UploadImageAsync(selectedImagePath);
                    imageUrl = uploadResult.Url;
                }

                var descripcion = DescriptionBox.Text;
                await postService.CreatePostAsync(descripcion, imageUrl ?? "", _me);

                Cancel_Click(sender, e);
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
                MessageBox.Show($"Error al publicar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ImagePreview_MouseEnter(object sender, MouseEventArgs e)
        {
            HoverText.Visibility = Visibility.Visible;
        }

        private void ImagePreview_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverText.Visibility = Visibility.Collapsed;
        }
    }
}
