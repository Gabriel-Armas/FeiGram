using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

public class RabbitMqClient
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqClient()
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task<FollowerCountResponse?> RequestFollowerCountAsync(string profileId)
    {
        var tcs = new TaskCompletionSource<FollowerCountResponse>();

        var replyQueue = _channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(_channel);
        var correlationId = Guid.NewGuid().ToString();

        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var body = ea.Body.ToArray();
                var response = JsonSerializer.Deserialize<FollowerCountResponse>(Encoding.UTF8.GetString(body));
                tcs.TrySetResult(response);
            }
        };

        _channel.BasicConsume(replyQueue, true, consumer);

        var request = new FollowerCountRequest { ProfileId = profileId };
        var json = JsonSerializer.Serialize(request);
        var bodyBytes = Encoding.UTF8.GetBytes(json);

        var props = _channel.CreateBasicProperties();
        props.ReplyTo = replyQueue;
        props.CorrelationId = correlationId;

        _channel.BasicPublish("", "get-followers-count", props, bodyBytes);

        return tcs.Task;
    }
}
