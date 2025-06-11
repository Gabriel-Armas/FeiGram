using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]))
        };

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
    });

builder.Services.AddAuthorization();

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
        app.Logger.LogError(ex, "Error al aplicar migraciones.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

string roleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
string subClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

app.MapGet("/profiles", [Authorize] async (HttpContext httpContext, ProfileDbContext db) =>
{
    var role = httpContext.User.FindFirst(roleClaimType)?.Value;
    if (role == "Banned") return Results.Forbid();

    try
    {
        return Results.Ok(await db.Profiles.ToListAsync());
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener los perfiles: " + ex.Message);
    }
});

app.MapGet("/profiles/{id}", [Authorize] async (HttpContext httpContext, string id, ProfileDbContext db) =>
{
    var role = httpContext.User.FindFirst(roleClaimType)?.Value;
    if (role == "Banned") return Results.Forbid();

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

app.MapPost("/profiles", [Authorize] async (HttpContext httpContext, Profile profile, ProfileDbContext db) =>
{
    var user = httpContext.User;

    foreach (var claim in user.Claims)
    {
        Console.WriteLine($"[PROFILE-API] Claim: {claim.Type} = {claim.Value}");
    }

    var role = user.FindFirst(roleClaimType)?.Value;
    Console.WriteLine($"[PROFILE-API] Rol recibido en token JWT: {role}");

    if (role != "Admin") return Results.Forbid();

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

app.MapPut("/profiles/{id}", [Authorize] async (HttpContext httpContext, string id, Profile updatedProfile, ProfileDbContext db) =>
{
    var user = httpContext.User;
    var role = user.FindFirst(roleClaimType)?.Value;
    var sub = user.FindFirst(subClaimType)?.Value;

    if (role is null || sub is null)
        return Results.Forbid();

    try
    {
        var profile = await db.Profiles.FindAsync(id);
        if (profile is null) return Results.NotFound();

        if (role == "Admin" || (role == "User" && id == sub))
        {
            profile.Name = updatedProfile.Name;
            profile.Photo = updatedProfile.Photo;
            profile.Sex = updatedProfile.Sex;

            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al actualizar el perfil: " + ex.Message);
    }
});

app.MapDelete("/profiles/{id}", [Authorize] async (HttpContext httpContext, string id, ProfileDbContext db) =>
{
    var user = httpContext.User;
    var role = user.FindFirst(roleClaimType)?.Value;
    var sub = user.FindFirst(subClaimType)?.Value;

    if (role is null || sub is null)
        return Results.Forbid();

    try
    {
        var profile = await db.Profiles.FindAsync(id);
        if (profile is null) return Results.NotFound();

        if (role == "Admin" || (role == "User" && id == sub))
        {
            db.Profiles.Remove(profile);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }

        return Results.Forbid();
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al eliminar el perfil: " + ex.Message);
    }
});

app.Run();
