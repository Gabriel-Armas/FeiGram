using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ProfileDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al aplicar migraciones. Verifica la cadena de conexión y el estado de la base de datos.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/profiles", async (ProfileDbContext db) =>
{
    try
    {
        return Results.Ok(await db.Profiles.ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener los perfiles: " + ex.Message);
    }
});

app.MapGet("/profiles/{id}", async (int id, ProfileDbContext db) =>
{
    try
    {
        var profile = await db.Profiles.FindAsync(id);
        return profile is not null ? Results.Ok(profile) : Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener el perfil: " + ex.Message);
    }
});

app.MapPost("/profiles", async (Profile profile, ProfileDbContext db) =>
{
    try
    {
        db.Profiles.Add(profile);
        await db.SaveChangesAsync();
        return Results.Created($"/profiles/{profile.Id}", profile);
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al crear el perfil: " + ex.Message);
    }
});

app.MapPut("/profiles/{id}", async (int id, Profile updatedProfile, ProfileDbContext db) =>
{
    try
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
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al actualizar el perfil: " + ex.Message);
    }
});

app.MapDelete("/profiles/{id}", async (int id, ProfileDbContext db) =>
{
    try
    {
        var profile = await db.Profiles.FindAsync(id);
        if (profile is null) return Results.NotFound();

        db.Profiles.Remove(profile);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al eliminar el perfil: " + ex.Message);
    }
});

app.Run();
