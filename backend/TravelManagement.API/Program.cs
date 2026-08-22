using Microsoft.EntityFrameworkCore;
using TravelManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure DB (Postgres)
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Port=5432;Database=tms_dev;Username=tms;Password=tms_pass";
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

// CORS - allow React dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddControllers();
// JWT Authentication settings (dev/demo)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_dev_key_please_change";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TravelManagementAPI";
var keyBytes = System.Text.Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // for local dev
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(keyBytes)
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApproverOnly", policy => policy.RequireRole("Direct Manager", "Department Manager", "Admin"));
    options.AddPolicy("CoordinatorOnly", policy => policy.RequireRole("Travel Coordinator", "Admin"));
    options.AddPolicy("HrOnly", policy => policy.RequireRole("HR", "Admin"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok("OK"));

// Apply any pending migrations at startup (safe for local dev) and seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        // Seed only in Development
        if (app.Environment.IsDevelopment())
        {
            try { TravelManagement.Infrastructure.Data.DbSeeder.Seed(db); } catch (Exception seedEx) { var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>(); logger.LogError(seedEx, "Seeding failed"); }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
