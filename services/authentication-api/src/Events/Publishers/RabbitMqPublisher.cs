using System.Text;
using System.Text.Json;
using AuthenticationApi.Models;
using RabbitMQ.Client;

namespace src.Events.Publishers;

public class RabbitMqPublisher
{
    private readonly string _hostname = "rabbitmq";
    private readonly string _queueName = "create-profile-queue";

    public void Publish(CreateProfileMessage message)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        channel.BasicPublish(exchange: "",
                             routingKey: _queueName,
                             basicProperties: null,
                             body: body);
    }
}
