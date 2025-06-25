using ChartApi.DTO;
using ChartApi.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ChartApi.Rabbit;

public class RabbitMqClient
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqClient()
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public async Task<List<Post>> RequestPostsForWeekAsync(string week)
    {
        var corrId = Guid.NewGuid().ToString();
        var replyQueue = _channel.QueueDeclare().QueueName;

        var props = _channel.CreateBasicProperties();
        props.CorrelationId = corrId;
        props.ReplyTo = replyQueue;

        var messageBody = Encoding.UTF8.GetBytes(week);
        _channel.BasicPublish(
            exchange: "",
            routingKey: "get-weekly-posts",
            basicProperties: props,
            body: messageBody);

        var tcs = new TaskCompletionSource<string>();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == corrId)
            {
                var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                tcs.SetResult(response);
            }
        };

        _channel.BasicConsume(
            consumer: consumer,
            queue: replyQueue,
            autoAck: true);

        var json = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var dtos = JsonConvert.DeserializeObject<List<PostDto>>(json) ?? new();
        return dtos.Select(dto => new Post
        {
            CreatedAt = dto.FechaPublicacion,
            Content = dto.Descripcion
        }).ToList();
    }
}
