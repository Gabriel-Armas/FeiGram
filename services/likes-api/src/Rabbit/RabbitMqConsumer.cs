using LikesApi.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Text.Json.Serialization;

public class RabbitMqConsumer
{
    private readonly MongoDbContext _dbContext;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqConsumer(MongoDbContext dbContext)
    {
        _dbContext = dbContext;

        var factory = new ConnectionFactory() { HostName = "rabbitmq" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "likes_queue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
    }

    public void Start()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var request = JsonSerializer.Deserialize<LikesRequest>(message);

            object responseObj = null;

            if (request.Action == "liked_post_ids")
            {
                Console.WriteLine($"[x] Received liked_post_ids request for user_id: {request.UserId}");
                var likedPostIds = await GetLikedPostIdsAsync(request.UserId);
                responseObj = new { liked_post_ids = likedPostIds };
            }
            else if (request.Action == "count_likes")
            {
                Console.WriteLine($"[x] Received count_likes request for post_id: {request.PostId}");
                var count = await CountLikesByPostIdAsync(request.PostId);
                responseObj = new { like_count = count };
            }
            else
            {
                Console.WriteLine($"[!] Unknown action: {request.Action}");
                responseObj = new { error = "Unknown action" }; 
            }

            var responseJson = JsonSerializer.Serialize(responseObj);
            var responseBytes = Encoding.UTF8.GetBytes(responseJson);

            var props = _channel.CreateBasicProperties();
            props.CorrelationId = ea.BasicProperties.CorrelationId;

            _channel.BasicPublish(exchange: "",
                                 routingKey: ea.BasicProperties.ReplyTo,
                                 basicProperties: props,
                                 body: responseBytes);

            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

            Console.WriteLine("[x] Sent response!");
        };

        _channel.BasicConsume(queue: "likes_queue",
                             autoAck: false,
                             consumer: consumer);

        Console.WriteLine(" [*] Waiting for RPC requests. To exit press CTRL+C");
    }

    private async Task<List<string>> GetLikedPostIdsAsync(string userId)
    {
        var filter = Builders<LikesApi.Models.Like>.Filter.Eq(l => l.UserId, userId);
        var likes = await _dbContext.Likes.Find(filter).ToListAsync();

        var postIds = new List<string>();
        foreach (var like in likes)
        {
            postIds.Add(like.PostId);
        }

        return postIds;
    }

    public async Task<int> CountLikesByPostIdAsync(string postId)
    {
        var filter = Builders<LikesApi.Models.Like>.Filter.Eq(l => l.PostId, postId);
        var count = await _dbContext.Likes.CountDocumentsAsync(filter);
        return (int)count;
    }


    public record LikesRequest(
        [property: JsonPropertyName("action")] string Action,
        [property: JsonPropertyName("user_id")] string UserId,
        [property: JsonPropertyName("post_id")] string PostId
    );
}