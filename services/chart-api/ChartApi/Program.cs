using System.Net;
using ChartApi.Services;
using ChartApi.Rabbit;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8087);
    /*
    options.Listen(IPAddress.Any, 8081, listenOptions =>
    {
        var certPath = Path.Combine(AppContext.BaseDirectory, "localhost.pfx");
        listenOptions.UseHttps(certPath, "miContrasenaSegura123");
    });
    */
});

builder.Services.AddGrpc();
builder.Services.AddSingleton<RabbitMqClient>();

var app = builder.Build();

app.MapGrpcService<ChartServiceImpl>();
app.MapGet("/", () => "Use a gRPC client to communicate with this service.");

app.Run();
