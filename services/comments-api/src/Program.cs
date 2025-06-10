using CommentsApi.Data;
using CommentsApi.Models;
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

app.Urls.Add("http://0.0.0.0:8088");
app.Run();
