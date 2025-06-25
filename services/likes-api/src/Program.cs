using LikesApi.Data;
using LikesApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/likes/check", async (string userId, string postId, MongoDbContext db) =>
{
    var filter = Builders<Like>.Filter.Eq(l => l.UserId, userId) & Builders<Like>.Filter.Eq(l => l.PostId, postId);
    var exists = await db.Likes.Find(filter).AnyAsync();
    return Results.Ok(exists);
});

app.MapGet("/likes/{id}", async (int id, MongoDbContext db) =>
{
    var like = await db.Likes.Find(l => l.Id == id).FirstOrDefaultAsync();
    return like is not null ? Results.Ok(like) : Results.NotFound();
});

app.MapPost("/likes", async (Like like, MongoDbContext db) =>
{
    like.Id = await db.GetNextSequenceValue("likes");
    await db.Likes.InsertOneAsync(like);
    return Results.Created($"/likes/{like.Id}", like);
});

app.MapDelete("/likes", async (
    HttpRequest request,
    MongoDbContext db) =>
{
    if (!request.Query.TryGetValue("userId", out var userIdValue) ||
        !request.Query.TryGetValue("postId", out var postIdValue))
    {
        return Results.BadRequest("Faltan parámetros userId o postId.");
    }

    var userId = userIdValue.ToString();
    var postId = postIdValue.ToString();

    var result = await db.Likes.DeleteOneAsync(
        l => l.UserId == userId && l.PostId == postId);

    return result.DeletedCount > 0
        ? Results.Ok("Like eliminado exitosamente.")
        : Results.NotFound("No se encontró el like para eliminar.");
});
app.Urls.Add("http://0.0.0.0:8082");

var consumer = new RabbitMqConsumer(app.Services.GetRequiredService<MongoDbContext>());
consumer.Start();

app.Run();
