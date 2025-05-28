using ChartApi.Models;

namespace ChartApi.Rabbit;

public class RabbitMqClient
{
    public RabbitMqClient()
    {
    }

    public List<Post> RequestPostsForWeek(string week)
    {
        return new List<Post>
        {
            new Post { CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Post { CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };
    }
}
