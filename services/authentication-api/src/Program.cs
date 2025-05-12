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
        var user = await dbContext.Users.Find(u => u.Username == request.Username).FirstOrDefaultAsync();

        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (!PasswordService.VerifyPassword(request.Password, user.Password))
        {
            return Results.Unauthorized();
        }

        var token = await authService.AuthenticateUserAsync(request.Username, request.Password);
        return Results.Ok(new { token });
    });

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
            Email = request.Email
        };

        dbContext.Users.InsertOne(user);

        return Results.Ok(new { message = "User registered successfully" });
    });


    app.Run();

    record RegisterRequest(string Username, string Password, string Email);
    record LoginRequest(string Username, string Password);