using System.Text;
using System.Text.Json;
using AuthenticationApi.Models;
using RabbitMQ.Client;

namespace src.Events.Publishers;

public class RabbitMqPublisher
{
    private readonly string _hostname = "rabbitmq";

    private readonly string _legacyQueue = "create-profile-queue";
    private readonly string _followQueue = "create-follow-user-queue";

    public void Publish(CreateProfileMessage message)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _legacyQueue,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

        channel.QueueDeclare(queue: _followQueue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: "",
                             routingKey: _legacyQueue,
                             basicProperties: null,
                             body: body);

        channel.BasicPublish(exchange: "",
                             routingKey: _followQueue,
                             basicProperties: null,
                             body: body);
    }
}
