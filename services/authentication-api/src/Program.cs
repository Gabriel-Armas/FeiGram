using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AuthenticationApi.Models;
using AuthenticationApi.Services;
using AuthenticationApi.Repositories;
using AuthenticationApi.Data;
using MongoDB.Driver;
using System.Text;

    var builder = WebApplication.CreateBuilder(args);

    var configuration = builder.Configuration;

    builder.Services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<AuthService>();
    builder.Services.AddSingleton<EmailService>();

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
    //builder.Services.AddHostedService<RabbitMqConsumer>();

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
        return Results.Ok(new { token });
    })
    .Produces<LoginRequest>()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

    app.MapPost("/register", async (HttpContext context, IConfiguration config, AuthenticationDbContext dbContext) =>
    {
        var request = await context.Request.ReadFromJsonAsync<RegisterRequest>();

        if (request is null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Email))
        {
            return Results.BadRequest("Invalid registration request");
        }

        var existingUser = await dbContext.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (existingUser != null)
        {
            return Results.BadRequest("Email already exists");
        }

        var hashedPassword = PasswordService.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Password = hashedPassword,
            Email = request.Email,
            Role = "User",
            CreationDate = DateTime.UtcNow
        };

        try
        {
            dbContext.Users.InsertOne(user);
            return Results.Ok(new { message = "User registered successfully" });
        }
        catch (Exception ex)
        {
            return Results.StatusCode(500);
        }
    })
    .Produces<RegisterRequest>()
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

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

        // Opcional: borrar o invalidar el token
        await dbContext.PasswordResetRequests.DeleteOneAsync(r => r.Id == resetRequest.Id);

        return Results.Ok("Password reset successfully");
    });

app.Run();

    record RegisterRequest(string Username, string Password, string Email);
    record LoginRequest(string Email, string Password);
    record ForgotPasswordRequest(string Email);
    record ResetPasswordRequest(string Token, string NewPassword);