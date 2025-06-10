using LikesApi.Data;
using LikesApi.Models;
using MongoDB.Driver;
using LikesApi.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/likes", async (MongoDbContext db) =>
{
    var likes = await db.Likes.Find(_ => true).ToListAsync();
    return Results.Ok(likes);
});

app.MapGet("/likes/{id}", async (int id, MongoDbContext db) =>
{
    var like = await db.Likes.Find(l => l.Id == id).FirstOrDefaultAsync();
    return like is not null ? Results.Ok(like) : Results.NotFound();
});

app.MapPost("/likes", async (Like like, MongoDbContext db, RabbitMqService rabbit) =>
{
    like.Id = await db.GetNextSequenceValue("likes");
    await db.Likes.InsertOneAsync(like);

    rabbit.Publish(like, "feigram.likes.created");

    return Results.Created($"/likes/{like.Id}", like);
});

app.MapDelete("/likes/{id}", async (int id, MongoDbContext db) =>
{
    var result = await db.Likes.DeleteOneAsync(l => l.Id == id);
    return result.DeletedCount > 0 ? Results.NoContent() : Results.NotFound();
});
app.Urls.Add("http://0.0.0.0:8082");
app.Run();
