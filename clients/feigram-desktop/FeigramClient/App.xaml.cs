using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;
using FeigramClient.Services;

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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
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
                client.BaseAddress = new Uri("https://localhost");
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            });

            Services = services.BuildServiceProvider();

            base.OnStartup(e);
        }
    }
}
