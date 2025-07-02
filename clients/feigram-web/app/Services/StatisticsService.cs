using ChartApi.Grpc;
using app.ViewModel;
using Grpc.Net.Client;
using System.Net.Http;

public class StatisticsService
{
    private readonly ChartService.ChartServiceClient _chartClient;
    private string? _bearerToken;

    public StatisticsService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel = GrpcChannel.ForAddress("https://feigram-nginx", new GrpcChannelOptions
        {
            HttpHandler = handler
        });

        _chartClient = new ChartService.ChartServiceClient(channel);
    }

    public void SetBearerToken(string token)
    {
        _bearerToken = token;
    }

    public async Task<StatsViewModel> GetStatsAsync()
    {
        var request = new ChartRequest
        {
            Week = DateTime.Now.ToString("yyyy-MM-dd")
        };

        var metadata = new Grpc.Core.Metadata();
        if (!string.IsNullOrEmpty(_bearerToken))
        {
            metadata.Add("Authorization", $"Bearer {_bearerToken}");
        }

        var response = await _chartClient.GetWeeklyPostCountsAsync(request, metadata);

        var stats = new StatsViewModel
        {
            PostsPerDay = response.Counts
                .GroupBy(d => DateTime.Parse(d.Day).ToString("dddd"))
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Count)
                ),
            TotalPosts = response.Counts.Sum(x => x.Count),
            WeekRange = GetWeekRange(DateTime.Now)
        };

        return stats;
    }


    private string GetWeekRange(DateTime date)
    {
        int diff = date.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - date.DayOfWeek;
        DateTime startOfWeek = date.AddDays(diff);
        DateTime endOfWeek = startOfWeek.AddDays(6);
        return $"{startOfWeek:yyyy-MM-dd} ~ {endOfWeek:yyyy-MM-dd}";
    }
}