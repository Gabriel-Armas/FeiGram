using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class UserProfileRequest
{
    public string UserId { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public string Name { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
}

public class UserProfileRpcConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public UserProfileRpcConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var factory = new ConnectionFactory()
    {
        HostName = "rabbitmq",
        UserName = "guest",
        Password = "guest"
    };

    // Intentar conectarse con reintentos
    for (int attempt = 1; attempt <= 5 && !stoppingToken.IsCancellationRequested; attempt++)
    {
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "user-profile-request",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            Console.WriteLine("Conectado a RabbitMQ");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reintentando conexión a RabbitMQ ({attempt}/5): {ex.Message}");
            await Task.Delay(2000, stoppingToken);
        }
    }

    if (_channel == null)
    {
        Console.WriteLine("No se pudo conectar a RabbitMQ después de varios intentos.");
        return;
    }

    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += async (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var request = JsonSerializer.Deserialize<UserProfileRequest>(message)!;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
        var profile = await db.Profiles.FindAsync(request.UserId);

        var userProfile = profile != null
            ? new UserProfileResponse
            {
                Name = profile.Name ?? string.Empty,
                Photo = profile.Photo ?? string.Empty
            }
            : new UserProfileResponse();

        var response = JsonSerializer.Serialize(userProfile);
        var responseBytes = Encoding.UTF8.GetBytes(response);

        var props = ea.BasicProperties;
        var replyProps = _channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        _channel.BasicPublish(
            exchange: "",
            routingKey: props.ReplyTo,
            basicProperties: replyProps,
            body: responseBytes
        );
    };

    _channel.BasicConsume(
        queue: "user-profile-request",
        autoAck: true,
        consumer: consumer
    );

    while (!stoppingToken.IsCancellationRequested)
    {
        await Task.Delay(1000, stoppingToken);
    }
}



    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
