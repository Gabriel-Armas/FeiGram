using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Services;

public class FollowService
{
    public async Task<FollowerCountResponse?> GetFollowerCountAsync(string profileId)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var replyQueueName = channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(channel);

        var tcs = new TaskCompletionSource<FollowerCountResponse>();
        var correlationId = Guid.NewGuid().ToString();

        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var body = ea.Body.ToArray();
                var responseJson = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[x] Received follower count response: {responseJson}");

                var response = JsonSerializer.Deserialize<FollowerCountResponse>(responseJson);
                tcs.SetResult(response!);
            }
        };

        channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

        var request = new FollowerCountRequest { ProfileId = profileId };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var requestJson = JsonSerializer.Serialize(request, options);
        var messageBytes = Encoding.UTF8.GetBytes(requestJson);

        var props = channel.CreateBasicProperties();
        props.ReplyTo = replyQueueName;
        props.CorrelationId = correlationId;

        Console.WriteLine($"[>] Sending follower count request: {requestJson}");

        channel.BasicPublish(
            exchange: "",
            routingKey: "get-followers-count",
            basicProperties: props,
            body: messageBytes
        );

        return await tcs.Task;
    }

    public async Task<FollowingListResponse?> GetFollowingListAsync(string profileId)
    {
        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var replyQueueName = channel.QueueDeclare().QueueName;
        var consumer = new EventingBasicConsumer(channel);

        var tcs = new TaskCompletionSource<FollowingListResponse>();
        var correlationId = Guid.NewGuid().ToString();

        consumer.Received += (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                var body = ea.Body.ToArray();
                var responseJson = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[x] Received following list response: {responseJson}");

                var response = JsonSerializer.Deserialize<FollowingListResponse>(responseJson);
                tcs.SetResult(response!);
            }
        };

        channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);

        var request = new FollowingListRequest { ProfileId = profileId };
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var requestJson = JsonSerializer.Serialize(request, options);
        var messageBytes = Encoding.UTF8.GetBytes(requestJson);

        var props = channel.CreateBasicProperties();
        props.ReplyTo = replyQueueName;
        props.CorrelationId = correlationId;

        Console.WriteLine($"[>] Sending following list request: {requestJson}");

        channel.BasicPublish(
            exchange: "",
            routingKey: "get-following-profiles",
            basicProperties: props,
            body: messageBytes
        );

        return await tcs.Task;
    }
}
