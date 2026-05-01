using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using user_service.Data;
using user_service.DTOs.Common;
using user_service.Interfaces;
using user_service.Options;
using user_service.Repositories;
using user_service.Services;

var builder = WebApplication.CreateBuilder(args);

var rawPort = Environment.GetEnvironmentVariable("PORT")
    ?? Environment.GetEnvironmentVariable("WEBSITES_PORT");

if (int.TryParse(rawPort, out var port) && port > 0)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Hide internal API from Swagger
    options.DocInclusionPredicate((name, api) =>
    {
        if (api.RelativePath?.StartsWith("api/internal") == true)
            return false;
        return true;
    });
});

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("SwaggerAndLocal", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection(SendGridOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var sendGridOptions = builder.Configuration.GetSection(SendGridOptions.SectionName).Get<SendGridOptions>() ?? new SendGridOptions();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set it in User Secrets or environment variables.");
}

jwtOptions.Key = string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase)
    ? Environment.GetEnvironmentVariable("JWT_KEY") ?? jwtOptions.Key
    : jwtOptions.Key;

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT key is not configured. Set 'Jwt:Key' in User Secrets or JWT_KEY in environment variables.");
}

sendGridOptions.ApiKey = string.IsNullOrWhiteSpace(sendGridOptions.ApiKey) || sendGridOptions.ApiKey.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase)
    ? Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? sendGridOptions.ApiKey
    : sendGridOptions.ApiKey;

sendGridOptions.FromEmail = string.IsNullOrWhiteSpace(sendGridOptions.FromEmail) || sendGridOptions.FromEmail.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase)
    ? Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL") ?? sendGridOptions.FromEmail
    : sendGridOptions.FromEmail;

if (string.IsNullOrWhiteSpace(sendGridOptions.ApiKey) || sendGridOptions.ApiKey.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("SendGrid API key is not configured. Set 'SendGrid:ApiKey' or SENDGRID_API_KEY.");
}

if (string.IsNullOrWhiteSpace(sendGridOptions.FromEmail) || sendGridOptions.FromEmail.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("SendGrid sender email is not configured. Set 'SendGrid:FromEmail' or SENDGRID_FROM_EMAIL.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<UserServiceDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IVerificationEmailSender, SendGridVerificationEmailSender>();
builder.Services.AddScoped<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IInternalUserService, InternalUserService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    db.Database.Migrate();
}

var enableSwagger = app.Environment.IsDevelopment() ||
                    string.Equals(Environment.GetEnvironmentVariable("ENABLE_SWAGGER"), "true", StringComparison.OrdinalIgnoreCase);

if (enableSwagger)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
        options.RoutePrefix = "swagger";
    });
}

var runningInContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseCors("SwaggerAndLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { status = "ok", service = "user-service" }))
    .WithName("Health")
    .WithOpenApi()
    .Produces(200)
    .ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .Produces(200)
    .ExcludeFromDescription();

app.MapControllers();

app.Run();
