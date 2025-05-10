using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/profiles", async (ProfileDbContext db) =>
{
    return await db.Profiles.ToListAsync();
});

app.MapGet("/profiles/{id}", async (int id, ProfileDbContext db) =>
{
    return await db.Profiles.FindAsync(id) is Profile profile
        ? Results.Ok(profile)
        : Results.NotFound();
});

app.MapPost("/profiles", async (Profile profile, ProfileDbContext db) =>
{
    db.Profiles.Add(profile);
    await db.SaveChangesAsync();
    return Results.Created($"/profiles/{profile.Id}", profile);
});

app.MapPut("/profiles/{id}", async (int id, Profile updatedProfile, ProfileDbContext db) =>
{
    var profile = await db.Profiles.FindAsync(id);
    if (profile is null) return Results.NotFound();

    profile.Name = updatedProfile.Name;
    profile.Photo = updatedProfile.Photo;
    profile.Email = updatedProfile.Email;
    profile.PasswordHash = updatedProfile.PasswordHash;
    profile.Sex = updatedProfile.Sex;
    profile.Nickname = updatedProfile.Nickname;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/profiles/{id}", async (int id, ProfileDbContext db) =>
{
    if (await db.Profiles.FindAsync(id) is Profile profile)
    {
        db.Profiles.Remove(profile);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
