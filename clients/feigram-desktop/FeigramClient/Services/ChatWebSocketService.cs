using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FeigramClient.Services
{
    public class ChatWebSocketService
    {
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;
        public event Action<string> OnMessageReceived;

        public event Action? OnConnected;
        public event Action<string>? OnError;
        public event Action? OnDisconnected;

        private TaskCompletionSource<string>? _historyResponseTcs;

        public async Task ConnectWithTokenAsync(string token)
        {
            _socket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            _socket.Options.RemoteCertificateValidationCallback = (sender, cert, chain, errors) => true;

            try
            {
                MessageBox.Show(token);
                Uri uri = new Uri($"wss://192.168.1.237/messages/ws/?token={token}");
                Console.WriteLine($"Connecting to {uri}");
                await _socket.ConnectAsync(uri, _cts.Token);
                OnConnected?.Invoke();

                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }


        public async Task SendMessageAsync(string json)
        {
            if (_socket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var buffer = new ArraySegment<byte>(bytes);
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, _cts.Token);
            }
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[4096];

            try
            {
                while (_socket.State == WebSocketState.Open)
                {
                    var ms = new MemoryStream();

                    WebSocketReceiveResult result;

                    do
                    {
                        result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnDisconnected?.Invoke();
                            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
                            return;
                        }
                        ms.Write(buffer, 0, result.Count);

                    } while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    using var reader = new StreamReader(ms, Encoding.UTF8);
                    string message = await reader.ReadToEndAsync();

                    var json = JObject.Parse(message);
                    if (json["type"]?.ToString() == "history")
                    {
                        var historyArray = json["messages"]?.ToString() ?? "[]";
                        _historyResponseTcs?.TrySetResult(historyArray);
                        continue;
                    }

                    if (json["type"]?.ToString() == "contacts")
                    {
                        var contactsArray = json["contacts"]?.ToString() ?? "[]";
                        _contactsResponseTcs?.TrySetResult(contactsArray);
                        continue;
                    }

                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex.Message);
            }
        }


        public async Task DisconnectAsync()
        {
            _cts.Cancel();
            if (_socket.State == WebSocketState.Open)
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
        }

        public async Task<string> GetChatHistoryAsync(string id, string friendId)
        {
            if (_socket.State == WebSocketState.Open)
            {
                _historyResponseTcs = new TaskCompletionSource<string>();

                var request = new
                {
                    type = "start_chat",
                    from = id,
                    with = friendId
                };

                var json = System.Text.Json.JsonSerializer.Serialize(request);
                await SendMessageAsync(json);

                return await _historyResponseTcs.Task;
            }

            throw new Exception("WebSocket no está conectado :c");
        }

        private TaskCompletionSource<string>? _contactsResponseTcs;

        public async Task<string> GetContactsAsync()
        {
            if (_socket.State == WebSocketState.Open)
            {
                _contactsResponseTcs = new TaskCompletionSource<string>();

                var request = new
                {
                    type = "get_contacts"
                };

                var json = System.Text.Json.JsonSerializer.Serialize(request);
                await SendMessageAsync(json);

                return await _contactsResponseTcs.Task;
            }

            throw new Exception("WebSocket no está conectado owo");
        }


    }
}
