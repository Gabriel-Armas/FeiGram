using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("jwt_token"))
            {
                context.Token = context.Request.Cookies["jwt_token"];
            }
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = "authentication-api",
        ValidAudience = "FeigramClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("V7hX!3tZq@wL9rM8k#1P&2oH8nD2u7Vb9G5wI8sL5kYwJ0tR0bB4cX9Q3fTg5")),
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwt_token"];
    
    if (!string.IsNullOrEmpty(token))
    {
        logger.LogInformation("JWT encontrado en cookie: {Token}", token);
        context.Request.Headers["Authorization"] = $"Bearer {token}";
    }
    else
    {
        logger.LogWarning("No se encontró la cookie jwt_token.");
    }

    await next();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/Logout", async (HttpContext context) =>
{
    context.Response.Redirect("/Login");
});

app.Urls.Add("http://0.0.0.0:8089");

app.Run();
