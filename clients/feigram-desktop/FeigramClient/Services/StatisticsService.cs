using System;
using System.Threading.Tasks;
using ChartApi.Grpc;
using Google.Protobuf.WellKnownTypes; 

namespace FeigramClient.Services
{
    internal class StatisticsService
    {
        private readonly ChartService.ChartServiceClient _client;

        public StatisticsService(ChartService.ChartServiceClient client)
        {
            _client = client;
        }
        public async Task<ChartResponse?> GetWeeklyStatsAsync()
        {
            var monday = GetMondayOfCurrentWeek();
            try
            {
                var response = await _client.GetWeeklyPostCountsAsync(new ChartRequest
                {
                    Week = monday.ToString("yyyy-MM-dd")
                });
                return response;
            }
            catch (Grpc.Core.RpcException ex)
            {
                Console.WriteLine($"Error gRPC: {ex.Status.Detail}");
                return null;
            }
        }


        public DateTime GetMondayOfCurrentWeek()
        {
            DateTime today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff);
        }
    }
}
