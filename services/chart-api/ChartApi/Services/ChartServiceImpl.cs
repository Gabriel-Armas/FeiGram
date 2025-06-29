using ChartApi.Models;
using ChartApi.Rabbit;
using ChartApi.Grpc;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;

namespace ChartApi.Services;

public class ChartServiceImpl : ChartService.ChartServiceBase
{
    private readonly RabbitMqClient _mqClient;

    public ChartServiceImpl(RabbitMqClient mqClient)
    {
        _mqClient = mqClient;
    }

    public override async Task<ChartResponse> GetWeeklyPostCounts(ChartRequest request, ServerCallContext context)
    {
        var posts = await _mqClient.RequestPostsForWeekAsync(request.Week);

        var dayCounts = posts
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new DailyCount
            {
                Day = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            });

        var response = new ChartResponse();
        response.Counts.AddRange(dayCounts);

        return await Task.FromResult(response);
    }
}
