using System.Net;
using ChartApi.Services;
using ChartApi.Rabbit;

var builder = WebApplication.CreateBuilder(args);

/*
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 8087);
});
*/

builder.Services.AddGrpc();
builder.Services.AddSingleton<RabbitMqClient>();

var app = builder.Build();

app.MapGrpcService<ChartServiceImpl>();
app.MapGet("/", () => "Use a gRPC client to communicate with this service.");

app.Run();
