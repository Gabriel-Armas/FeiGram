using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;
using FeigramClient.Services;
using LiveCharts.Wpf.Charts.Base;
using ChartApi.Grpc;

namespace FeigramClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        public static HttpClient HttpClient { get; internal set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();


            services.AddHttpClient<AuthenticationService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<FeedService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<ProfileService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<CommentsService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<PostsService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<LikesService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            services.AddHttpClient<FollowService>(client =>
            {
                client.BaseAddress = new Uri("https://192.168.1.237");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });
            
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://192.168.1.237", new Grpc.Net.Client.GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

            var client = new ChartApi.Grpc.ChartService.ChartServiceClient(channel);

            services.AddSingleton(channel);
            services.AddSingleton(client);

            services.AddTransient<StatisticsService>(sp =>
            {
                var grpcClient = sp.GetRequiredService<ChartApi.Grpc.ChartService.ChartServiceClient>();
                return new StatisticsService(grpcClient);
            });

            services.AddSingleton<ChatWebSocketService>();
            Services = services.BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}
