using CommentsApi.Data;
using CommentsApi.Models;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CommentsApi.Rabbit;

public class RabbitMqClient
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly MongoDbContext _dbContext;

    public RabbitMqClient(MongoDbContext dbContext)
    {
        _dbContext = dbContext;

    var factory = new ConnectionFactory() { HostName = "rabbitmq" };

    int retries = 20;
    int delayMs = 5000;
    Exception lastException = null;

    for (int i = 0; i < retries; i++)
    {
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            Console.WriteLine("✅ Conectado a RabbitMQ");
            break;
        }
        catch (Exception ex)
        {
            lastException = ex;
            Console.WriteLine($"❌ Error al conectar a RabbitMQ: {ex.Message}. Reintentando en {delayMs}ms...");
            Thread.Sleep(delayMs);
        }
    }

    if (_connection == null || !_connection.IsOpen)
    {
        throw new Exception("No se pudo conectar a RabbitMQ después de varios intentos.", lastException);
    }
    }

    public void ListenForPostCountRequests()
    {
        _channel.QueueDeclare(queue: "comments_count_queue", durable: false, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var request = JsonSerializer.Deserialize<PostCountRequest>(message);

            if (request == null || string.IsNullOrWhiteSpace(request.post_id))
            {
                Console.WriteLine("❌ Error: post_id inválido o request nulo.");
                return;
            }

            var count = await _dbContext.Comments.CountDocumentsAsync(c => c.PostId == request.post_id);

            var responseJson = JsonSerializer.Serialize(new { count });
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            _channel.BasicPublish(
                exchange: "",
                routingKey: props.ReplyTo,
                basicProperties: replyProps,
                body: responseBytes
            );
        };

        _channel.BasicConsume(queue: "comments_count_queue", autoAck: true, consumer: consumer);
        Console.WriteLine("🔄 Esperando mensajes de post-api...");
    }
}

public class PostCountRequest
{
    public string post_id { get; set; } = string.Empty;
}
