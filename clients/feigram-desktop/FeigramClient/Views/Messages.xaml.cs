using ChartApi.Grpc;
using FeigramClient.Models;
using FeigramClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FeigramClient.Resources;

namespace FeigramClient.Views
{
    /// <summary>
    /// Lógica de interacción para Messages.xaml
    /// </summary>
    public partial class Messages : Page
    {
        public ObservableCollection<Friend> Friends { get; set; } = new();
        public ObservableCollection<MessageDto> ChatMessages { get; set; } = new();
        private ProfileSingleton _me;
        private readonly ChatWebSocketService _chatService;
        private string _selectedFriendId;
        private Frame _ModalFrame;
        private Grid _ModalOverlay;
        private MainWindow _mainWindow;
        public string CurrentUserId => _me.Id;

        private readonly Dictionary<string, List<MessageDto>> _messageHistory = new();

        public Messages(ProfileSingleton profile, MainWindow mainWindow, Frame modalFrame, Grid modalOverlay)
        {
            InitializeComponent();
            this.DataContext = this;

            _me = profile;
            _mainWindow = mainWindow;
            _ModalFrame = modalFrame;
            _ModalOverlay = modalOverlay;
            _chatService = App.Services.GetRequiredService<ChatWebSocketService>();

            ((MessageBubbleColorConverter)Resources["MessageBubbleColorConverter"]).CurrentUserId = _me.Id;

            _chatService.OnMessageReceived += OnIncomingMessage;
            _chatService.OnError += ChatService_OnError;  // Aquí te suscribes al evento de error

            this.Loaded += Messages_Loaded;

            ChatMessagesList.ItemsSource = ChatMessages;
        }

        private void ChatService_OnError(string errorMessage)
        {
            // Ejecutar en el hilo UI porque es un callback desde otro hilo
            Dispatcher.Invoke(() =>
            {
                if (errorMessage.Contains("token expirado", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tu sesión ha expirado. Por favor inicia sesión de nuevo.", "Sesión expirada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    RedirectToLogin();
                }
                else if (errorMessage.Contains("usuario baneado", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tu cuenta ha sido baneada. Contacta con soporte.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Error);
                    RedirectToLogin();
                }
                else
                {
                    // Otros errores
                    MessageBox.Show($"Error: {errorMessage}", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void RedirectToLogin()
        {
            // Cierra esta ventana / página y vuelve a mostrar la ventana login principal

            // Limpias datos si es necesario
            _mainWindow.MainFrame.Content = null;
            _mainWindow.GridLogin.Visibility = Visibility.Visible;
            _mainWindow.GridMainMenu.Visibility = Visibility.Hidden;
            _mainWindow.EmailTextBox.Text = "";
            _mainWindow.PasswordBox.Password = "";

            // Opcional: cerrar la ventana actual si es independiente
            Window.GetWindow(this)?.Close();
        }



        private void Home_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var home = new MainMenu(_me, _mainWindow);
            _ModalFrame.Navigate(home);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var profilePage = new Profile(_me, _mainWindow, _ModalFrame, _ModalOverlay, true);
            _ModalFrame.Navigate(profilePage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Messages_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var messagesPage = new Messages(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(messagesPage);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new ConsultAccount(_me, _mainWindow, _ModalFrame, _ModalOverlay);
            _ModalFrame.Navigate(consultAccounts);
            _ModalOverlay.Visibility = Visibility.Visible;
        }

        private void Stadistic_Click(object sender, RoutedEventArgs e)
        {
            GridMenu.Visibility = Visibility.Collapsed;
            var consultAccounts = new Statistics(_me, _mainWindow, _ModalFrame, _ModalOverlay);
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

        private async Task LoadChatHistoryAsync(string userId, string friendId)
        {
            try
            {
                ChatMessages.Clear();

                var historyJson = await _chatService.GetChatHistoryAsync(userId, friendId);

                var historyArray = JArray.Parse(historyJson);

                ChatMessages.Clear();
                _messageHistory[friendId] = new List<MessageDto>();

                foreach (var item in historyArray)
                {
                    var msg = new MessageDto
                    {
                        FromUserId = item["from"]?.ToString(),
                        ToUserId = item["to"]?.ToString(),
                        Content = item["content"]?.ToString(),
                        SentAt = DateTime.Parse(item["timestamp"]?.ToString() ?? "").ToLocalTime()
                    };

                    _messageHistory[friendId].Add(msg);

                    if (msg.FromUserId == _me.Id)
                        AgregarMensajePropio(msg);
                    else
                        AgregarMensajeAmigo(msg);
                }


                ScrollToBottom();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar historial kawaii: " + ex.Message);
            }
        }


        private async void Messages_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _chatService.ConnectWithTokenAsync(_me.Token);
                await LoadFriends();
                await LoadMessageContacts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al iniciar chat: " + ex.Message);
            }
        }

        private async Task LoadFriends()
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
                MessageBox.Show("Error cargando amigos: " + ex.Message);
            }
        }

        private async Task LoadMessageContacts()
        {
            try
            {
                var contactsJson = await _chatService.GetContactsAsync();
                var contactIds = JArray.Parse(contactsJson);

                var profileService = App.Services.GetRequiredService<ProfileService>();
                profileService.SetToken(_me.Token);

                foreach (var idToken in contactIds)
                {
                    var id = idToken.ToString();

                    if (Friends.Any(f => f.Id == id)) continue;

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
            catch (Exception ex)
            {
                MessageBox.Show("No pude cargar los contactos uwu: " + ex.Message);
            }
        }


        private void OnIncomingMessage(string rawJson)
        {
            var msgObj = JObject.Parse(rawJson);
            Dispatcher.Invoke(() =>
            {
                if (msgObj["from_user"] != null && msgObj["to"] != null && msgObj["content"] != null)
                {
                    var from = msgObj["from_user"]?.ToString();
                    var to = msgObj["to"]?.ToString();
                    var content = msgObj["content"]?.ToString();

                    var otherId = from == _me.Id ? to : from;

                    if (!_messageHistory.ContainsKey(otherId))
                        _messageHistory[otherId] = new List<MessageDto>();

                    _messageHistory[otherId].Add(new MessageDto
                    {
                        FromUserId = from,
                        ToUserId = to,
                        Content = content,
                        SentAt = DateTime.Now 
                    });

                    if (_selectedFriendId == otherId)
                    {
                        var newMsg = new MessageDto
                        {
                            FromUserId = from,
                            ToUserId = to,
                            Content = content,
                            SentAt = DateTime.Now
                        };

                        if (from == _me.Id)
                            AgregarMensajePropio(newMsg);
                        else
                            AgregarMensajeAmigo(newMsg);
                    }

                }
            });
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var content = ChatInput.Text;
            if (string.IsNullOrWhiteSpace(content)) return;

            var json = JsonConvert.SerializeObject(new
            {
                to = _selectedFriendId,
                content,
            });

            await _chatService.SendMessageAsync(json);

            if (!_messageHistory.ContainsKey(_selectedFriendId))
                _messageHistory[_selectedFriendId] = new List<MessageDto>();

            _messageHistory[_selectedFriendId].Add(new MessageDto
            {
                FromUserId = _me.Id,
                ToUserId = _selectedFriendId,
                Content = content,
                SentAt = DateTime.Now
            });

            var newMsg = new MessageDto
            {
                FromUserId = _me.Id,
                ToUserId = _selectedFriendId,
                Content = content,
                SentAt = DateTime.Now
            };

            AgregarMensajePropio(newMsg);
            ChatInput.Text = "";
        }

        private async void FriendsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FriendsList.SelectedItem is Friend selectedFriend)
            {
                _selectedFriendId = selectedFriend.Id;
                ChatTitle.Text = selectedFriend.Name;
                
                await LoadChatHistoryAsync(_me.Id, _selectedFriendId);
            }
        }

        private void AgregarMensajePropio(MessageDto message)
        {
            ChatMessages.Add(message);
            ScrollToBottom();
        }

        private void AgregarMensajeAmigo(MessageDto message)
        {
            ChatMessages.Add(message);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (ChatMessagesList.Items.Count > 0)
            {
                ChatMessagesList.ScrollIntoView(ChatMessagesList.Items[ChatMessagesList.Items.Count - 1]);
            }
        }
    }
}
