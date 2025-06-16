using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AuthenticationApi.Models;
using AuthenticationApi.Services;
using AuthenticationApi.Repositories;
using AuthenticationApi.Data;
using MongoDB.Driver;
using System.Text;
using src.Events.Publishers;

var builder = WebApplication.CreateBuilder(args);

    var configuration = builder.Configuration;

    builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<AuthService>();
    builder.Services.AddSingleton<EmailService>();
    builder.Services.AddSingleton<RabbitMqPublisher>();
    builder.Services.AddSingleton(provider => new CloudinaryDotNet.Cloudinary(new CloudinaryDotNet.Account(
    configuration["Cloudinary:CloudName"],
    configuration["Cloudinary:ApiKey"],
    configuration["Cloudinary:ApiSecret"]
    )));

    builder.Services.AddSingleton<AuthenticationDbContext>(serviceProvider =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new AuthenticationDbContext(configuration);
    });

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
        };
    });

    builder.Services.AddAuthorization();

    var app = builder.Build();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapPost("/login", async (LoginRequest request, AuthService authService, AuthenticationDbContext dbContext) =>
    {
        var user = await dbContext.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

        if (user is null || !PasswordService.VerifyPassword(request.Password, user.Password))
        {
            return Results.Unauthorized();
        }
            
        var token = await authService.AuthenticateUserAsync(request.Email, request.Password);
        return Results.Ok(new { token, userId= user.Id, rol = user.Role});
    })
    .Produces<LoginRequest>()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

    app.MapPost("/register", async (
    HttpContext context,
    IConfiguration config,
    AuthenticationDbContext dbContext,
    RabbitMqPublisher publisher,
    CloudinaryDotNet.Cloudinary cloudinary) =>
{
    var form = await context.Request.ReadFormAsync();

    var username = form["Username"].ToString();
    var password = form["Password"].ToString();
    var email = form["Email"].ToString();
    var sex = form["Sex"].ToString();
    var photoFile = form.Files.GetFile("Photo");

    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
    {
        return Results.BadRequest("Missing required fields");
    }

    var existingUser = await dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    if (existingUser != null)
    {
        return Results.BadRequest("Email already exists");
    }

    string photoUrl = null;

    if (photoFile != null)
    {
        using var stream = photoFile.OpenReadStream();
        var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams()
        {
            File = new CloudinaryDotNet.FileDescription(photoFile.FileName, stream)
        };
        var uploadResult = await cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
            photoUrl = uploadResult.SecureUrl.ToString();
        }
        else
        {
            return Results.Problem(detail: "Error uploading image", statusCode: 500);
        }
    }

    var hashedPassword = PasswordService.HashPassword(password);

    var user = new User
    {
        Username = username,
        Password = hashedPassword,
        Email = email,
        Role = "User",
        CreationDate = DateTime.UtcNow
    };

    try
    {
        dbContext.Users.InsertOne(user);

        var message = new CreateProfileMessage
        {
            UserId = user.Id,
            Name = user.Username,
            Photo = photoUrl,
            Sex = sex
        };

        publisher.Publish(message);

        return Results.Ok(new { message = "User registered successfully" });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
    });


    app.MapPost("/forgot-password", async (ForgotPasswordRequest request, AuthenticationDbContext dbContext, EmailService emailService) =>
    {
        var user = await dbContext.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (user is null)
            return Results.Ok("If the email exists, a reset link will be sent~");

        var token = Guid.NewGuid().ToString();
        var expiration = DateTime.UtcNow.AddMinutes(15);

        var resetRequest = new PasswordResetRequest
        {
            Email = request.Email,
            Token = token,
            Expiration = expiration
        };

        await dbContext.PasswordResetRequests.InsertOneAsync(resetRequest);

        var resetUrl = $"http://localhost:8088/ResetPassword?token={token}";

        await emailService.SendResetPasswordEmail(request.Email, resetUrl);

        return Results.Ok("if the email exists, a reset link will be sent~");
    });

    app.MapPost("/reset-password", async (ResetPasswordRequest request, AuthenticationDbContext dbContext) =>
    {
        var resetRequest = await dbContext.PasswordResetRequests
            .Find(r => r.Token == request.Token && r.Expiration > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (resetRequest == null)
            return Results.BadRequest("Invalid or expired reset token");

        var user = await dbContext.Users.Find(u => u.Email == resetRequest.Email).FirstOrDefaultAsync();
        if (user == null)
            return Results.BadRequest("User not found");

        var hashedPassword = PasswordService.HashPassword(request.NewPassword);

        var update = Builders<User>.Update.Set(u => u.Password, hashedPassword);
        await dbContext.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        await dbContext.PasswordResetRequests.DeleteOneAsync(r => r.Id == resetRequest.Id);

        return Results.Ok("Password reset successfully");
    });

    app.MapPost("/ban-user", async (BanUserRequest request, AuthenticationDbContext dbContext) =>
    {
        var user = await dbContext.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

        if (user is null)
            return Results.NotFound("User not found");

        var update = Builders<User>.Update.Set(u => u.Role, "Banned");
        await dbContext.Users.UpdateOneAsync(u => u.Id == user.Id, update);

        return Results.Ok($"User {user.Username} has been banned");
    });

    app.MapGet("/users/{id}", async (string id, AuthenticationDbContext dbContext) =>
    {
        var user = await dbContext.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

        if (user == null)
            return Results.NotFound();

        return Results.Ok(new 
        { 
            email = user.Email,
            role = user.Role
        });
    });

app.Urls.Add("http://0.0.0.0:8084");
app.Run();

    record RegisterRequest(string Username, string Password, string Email, string Photo, string Sex);
    record LoginRequest(string Email, string Password);
    record ForgotPasswordRequest(string Email);
    record ResetPasswordRequest(string Token, string NewPassword);
    record BanUserRequest(string Email);
