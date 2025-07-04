using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace src.Rabbit;

public class RabbitMqConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _hostname = "rabbitmq";
    private readonly string _queueName = "create-profile-queue";

    public RabbitMqConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            int maxRetries = 3;
            int attempt = 0;
            bool success = false;

            while (attempt < maxRetries && !success)
            {
                attempt++;

                try
                {
                    var message = JsonSerializer.Deserialize<CreateProfileMessage>(json);
                    if (message != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();

                        var profile = new Profile
                        {
                            Id = message.UserId,
                            Name = message.Name,
                            Photo = message.Photo,
                            Enrollment = message.Enrollment,
                            Sex = message.Sex,
                            Major = message.Major
                        };

                        Console.WriteLine($"🎯 Creando perfil: {profile.Name}, Major: {profile.Major ?? "null"}");

                        dbContext.Profiles.Add(profile);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        success = true;
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Intento {attempt} fallido: {ex.Message}");

                    if (attempt == maxRetries)
                    {
                        Console.WriteLine("Se alcanzó el máximo de reintentos. El mensaje será descartado.");
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        await Task.Delay(1000); 
                    }
                }
            }
        };

        channel.BasicConsume(queue: _queueName,
                             autoAck: false,
                             consumer: consumer);

        return Task.CompletedTask;
    }
}
