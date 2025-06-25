using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

public class UserProfileRpcClient
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly IBasicProperties props;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();

    public UserProfileRpcClient()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        consumer = new EventingBasicConsumer(channel);

        props = channel.CreateBasicProperties();
        props.ReplyTo = replyQueueName;
        props.CorrelationId = Guid.NewGuid().ToString();

        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                return;

            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.SetResult(response);
        };

        channel.BasicConsume(replyQueueName, true, consumer);
    }

    public async Task<UserProfileResponse> CallAsync(string userId)
    {
        var message = JsonSerializer.Serialize(new UserProfileRequest { UserId = userId });
        var body = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();

        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;

        callbackMapper[correlationId] = tcs;

        channel.BasicPublish("", "user-profile-request", props, body);

        var result = await tcs.Task;
        return JsonSerializer.Deserialize<UserProfileResponse>(result)!;
    }
    public class UserProfileRequest
    {
        public string UserId { get; set; } = string.Empty;
    }
    public class UserProfileResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Photo { get; set; } = string.Empty;
    }
}
