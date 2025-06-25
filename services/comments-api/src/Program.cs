using CommentsApi.Data;
using CommentsApi.Models;
using CommentsApi.Rabbit;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

var mongoConnectionString = builder.Configuration["MongoDb:ConnectionString"]
    ?? throw new Exception("MongoDb:ConnectionString is not configured");
builder.Services.AddSingleton(new MongoDbContext(mongoConnectionString));

builder.Services.AddSingleton<RabbitMqClient>();
builder.Services.AddSingleton<UserProfileRpcClient>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/comments", async (MongoDbContext db) =>
{
    var comments = await db.Comments.Find(_ => true).ToListAsync();
    return Results.Ok(comments);
});

app.MapGet("/comments/{id}", async (int id, MongoDbContext db) =>
{
    var comment = await db.Comments.Find(c => c.CommentId == id).FirstOrDefaultAsync();
    return comment is not null ? Results.Ok(comment) : Results.NotFound();
});

app.MapPost("/comments", async (Comment comment, MongoDbContext db) =>
{
    comment.CommentId = await db.GetNextSequenceValue("comments");
    comment.CreatedAt = DateTime.UtcNow;
    await db.Comments.InsertOneAsync(comment);
    return Results.Created($"/comments/{comment.CommentId}", comment);
});

app.MapDelete("/comments/{id}", async (int id, MongoDbContext db) =>
{
    var result = await db.Comments.DeleteOneAsync(c => c.CommentId == id);
    return result.DeletedCount > 0 ? Results.NoContent() : Results.NotFound();
});

app.MapGet("/profiles/{userId}", async (string userId, UserProfileRpcClient rpcClient) =>
{
    try
    {
        var profile = await rpcClient.CallAsync(userId);

        if (string.IsNullOrEmpty(profile.Name) && string.IsNullOrEmpty(profile.Photo))
            return Results.NotFound();

        return Results.Ok(profile);
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener el perfil vía RPC: " + ex.Message);
    }
});


// Obtiene instancia de RabbitMqClient del contenedor
var rabbitClient = app.Services.GetRequiredService<RabbitMqClient>();

// Inicia el listener
rabbitClient.ListenForPostCountRequests();
rabbitClient.ListenForCommentListRequests();

app.Urls.Add("http://0.0.0.0:8083");
app.Run();
