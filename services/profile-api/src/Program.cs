using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using src.Rabbit;
using Services;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProfileDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<RabbitMqConsumer>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<FollowService>();
builder.Services.AddSingleton(provider =>
{
    var config = builder.Configuration;
    return new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
        config["Cloudinary:CloudName"],
        config["Cloudinary:ApiKey"],
        config["Cloudinary:ApiSecret"]
    ));
});

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

app.MapGet("/profiles", [Authorize] async (
    HttpContext httpContext,
    ProfileDbContext db,
    FollowService followService) =>
{
    var role = httpContext.User.FindFirst(roleClaimType)?.Value;
    if (role == "Banned") return Results.Forbid();

    try
    {
        var profiles = await db.Profiles.ToListAsync();

        var enrichedProfiles = new List<object>();
        foreach (var profile in profiles)
        {
            var followerData = await followService.GetFollowerCountAsync(profile.Id);
            var count = followerData?.FollowerCount ?? 0;

            enrichedProfiles.Add(new
            {
                profile.Id,
                profile.Name,
                profile.Photo,
                profile.Sex,
                FollowerCount = count
            });
        }

        return Results.Ok(enrichedProfiles);
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener los perfiles: " + ex.Message);
    }
});

app.MapGet("/profiles/{id}", [Authorize] async (
    HttpContext httpContext,
    string id,
    ProfileDbContext db,
    FollowService followService) =>
{
    var role = httpContext.User.FindFirst(roleClaimType)?.Value;
    if (role == "Banned") return Results.Forbid();

    try
    {
        var profile = await db.Profiles.FindAsync(id);
        if (profile is null) return Results.NotFound();

        var followerData = await followService.GetFollowerCountAsync(profile.Id);
        var count = followerData?.FollowerCount ?? 0;

        var result = new
        {
            profile.Id,
            profile.Name,
            profile.Photo,
            profile.Sex,
            FollowerCount = count
        };

        return Results.Ok(result);
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

app.MapPut("/profiles/{id}", [Authorize] async (
    HttpContext httpContext,
    string id,
    ProfileDbContext db,
    CloudinaryDotNet.Cloudinary cloudinary) =>
{
    var user = httpContext.User;
    var role = user.FindFirst(roleClaimType)?.Value;
    var sub = user.FindFirst(subClaimType)?.Value;

    if (role is null || sub is null)
        return Results.Forbid();

    var form = await httpContext.Request.ReadFormAsync();

    var name = form["Name"].ToString();
    var sex = form["Sex"].ToString();
    var photoFile = form.Files.GetFile("Photo");

    try
    {
        var profile = await db.Profiles.FindAsync(id);
        if (profile is null) return Results.NotFound();

        if (role == "Admin" || (role == "User" && id == sub))
        {
            if (!string.IsNullOrEmpty(name))
                profile.Name = name;

            if (!string.IsNullOrEmpty(sex))
                profile.Sex = sex;

            if (photoFile != null)
            {
                using var stream = photoFile.OpenReadStream();
                var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams
                {
                    File = new CloudinaryDotNet.FileDescription(photoFile.FileName, stream)
                };
                var uploadResult = await cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    profile.Photo = uploadResult.SecureUrl.ToString();
                }
                else
                {
                    return Results.Problem("Error al subir la imagen a Cloudinary", statusCode: 500);
                }
            }

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

app.MapGet("/profiles/{id}/following", [Authorize] async (
    HttpContext httpContext,
    string id,
    ProfileDbContext db,
    FollowService followService) =>
{
    var role = httpContext.User.FindFirst(roleClaimType)?.Value;
    if (role == "Banned") return Results.Forbid();

    try
    {
        var followingResponse = await followService.GetFollowingListAsync(id);
        if (followingResponse is null || followingResponse.FollowingIds.Length == 0)
            return Results.Ok(new List<object>());

        var followedProfiles = await db.Profiles
            .Where(p => followingResponse.FollowingIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Photo
            })
            .ToListAsync();

        return Results.Ok(followedProfiles);
    }
    catch (Exception ex)
    {
        return Results.Problem("Error al obtener los seguidos: " + ex.Message);
    }
});

app.Run();