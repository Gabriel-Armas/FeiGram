using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace LikesApi.RabbitMQ
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "rabbitmq" };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: "feigram_events", type: ExchangeType.Topic);
            }
            catch (Exception ex)
            {
                Console.WriteLine("🐾 Error al conectar con RabbitMQ: " + ex.Message);
                throw;
            }
        }


        public void Publish<T>(T message, string routingKey)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "feigram_events",
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }
    }
}
