using System.Reflection;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using user_service.Data;
using user_service.Interfaces;
using user_service.Options;
using user_service.Repositories;
using user_service.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set it in User Secrets.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Key) || jwtOptions.Key.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("JWT key is not configured. Set 'Jwt:Key' in User Secrets.");
}

if (string.IsNullOrWhiteSpace(sendGridOptions.ApiKey) || sendGridOptions.ApiKey.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("SendGrid API key is not configured. Set 'SendGrid:ApiKey' in User Secrets.");
}

if (string.IsNullOrWhiteSpace(sendGridOptions.FromEmail) || sendGridOptions.FromEmail.Contains("__SET_IN_USER_SECRETS__", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("SendGrid sender email is not configured. Set 'SendGrid:FromEmail' in User Secrets.");
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("SwaggerAndLocal");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
