using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace src.Rabbit;

public class RabbitMqPublisher
{
    private readonly IModel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        var connection = factory.CreateConnection();
        _channel = connection.CreateModel();

        _channel.QueueDeclare(queue: "create-profile-queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    public void PublishCreateProfile(CreateProfileMessage message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(exchange: "",
                              routingKey: "create-profile-queue",
                              basicProperties: null,
                              body: body);
    }
}
