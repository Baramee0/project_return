using System.Text;
using Backend.Data;
using Backend.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtService>();

// Get JWT settings from environment variables with fallback to configuration
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
               builder.Configuration["JwtSettings:SecretKey"];
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
            builder.Configuration["JwtSettings:Issuer"] ?? "ProjectReturn";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
              builder.Configuration["JwtSettings:Audience"] ?? "ProjectReturnUsers";

// Validate secret key
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret Key is not configured. Please set JWT_SECRET_KEY in .env file or JwtSettings:SecretKey in appsettings.json");
}

if (secretKey.Length < 32)
{
    throw new InvalidOperationException($"JWT Secret Key must be at least 32 characters long. Current length: {secretKey.Length}");
}

// Debug output (remove in production)
Console.WriteLine($"JWT Secret Key Length: {secretKey.Length} characters");
Console.WriteLine($"JWT Issuer: {issuer}");
Console.WriteLine($"JWT Audience: {audience}");

var key = Encoding.UTF8.GetBytes(secretKey);

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // Optional: reduce token expiry tolerance
        };
    });

// Add CORS for Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJS", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Optional: if you need credentials
    });
});

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowNextJS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();