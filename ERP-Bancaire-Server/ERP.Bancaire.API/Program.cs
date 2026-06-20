using System.Text;
using ERP.Bancaire.Persistence;
using ERP.Bancaire.Persistence.Seed;
using ERP.Bancaire.Domain.Constants;
using ERP.Bancaire.Infrastructure.Services;
using ERP.Bancaire.Application.Interfaces;
using ERP.Bancaire.Application.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDevClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ERPBancaireDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<ERP.Bancaire.Application.Interfaces.IEmailService, ERP.Bancaire.Infrastructure.Services.EmailService>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var key = Encoding.UTF8.GetBytes(
    builder.Configuration["JwtSettings:SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        Permissions.UserCreate,
        policy => policy.RequireClaim("Permission", Permissions.UserCreate));

    options.AddPolicy(
        Permissions.UserDelete,
        policy => policy.RequireClaim("Permission", Permissions.UserDelete));

    options.AddPolicy(
        Permissions.ClientRead,
        policy => policy.RequireClaim("Permission", Permissions.ClientRead));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- DATABASE MIGRATIONS AND SEEDING BLOCK ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ERPBancaireDbContext>();

    // FIXED: This line safely creates the database schema & tables if they don't exist yet
    await context.Database.MigrateAsync();

    // Now your seeders can query the tables without throwing a 42P01 exception
    await RoleSeeder.SeedAsync(context);
    await PermissionSeeder.SeedAsync(context);
    await RolePermissionSeeder.SeedAsync(context);
    await AdminSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDevClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();