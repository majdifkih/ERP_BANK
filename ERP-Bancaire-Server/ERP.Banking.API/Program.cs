using ERP.Banking.API.Extensions;
using ERP.Banking.API.Middlewares;
using ERP.Banking.Application;
using ERP.Banking.Infrastructure;
using Serilog;

// ── Bootstrap logger (captures startup errors before host is ready) ──
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ERP Banking API…");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console());

    // ── Infrastructure (DB, JWT auth, services) ───────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Application layer ─────────────────────────────────────────
    builder.Services.AddApplication();

    // ── API layer ─────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerWithJwt();
    builder.Services.AddPermissionAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularDevClient", policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });


    // ── Build ─────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Seed database ─────────────────────────────────────────────
    await app.SeedDatabaseAsync();

    // ── Middleware pipeline ───────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Banking API v1");
            options.RoutePrefix = string.Empty; // Swagger at root
        });
    }

    //app.UseHttpsRedirection();
    app.UseCors("AllowAngularDevClient");
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}