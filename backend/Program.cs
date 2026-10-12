using System.Text;
using CampusEats.Api.Services;
using CampusEats.Api.Data;
using CampusEats.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// =======================
// Add services
// =======================

builder.Services.AddControllers();

var frontendOrigin = builder.Configuration["Cors:FrontendOrigin"]
    ?? throw new InvalidOperationException("Cors:FrontendOrigin must be configured.");

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(frontendOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// =======================
// Swagger + JWT Support
// =======================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste ONLY the token - no 'Bearer ' prefix."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// =======================
// Database Configuration
// =======================

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default must be configured.")
    ));


// =======================
// Application Services
// =======================

builder.Services.AddScoped<IMenuService, MenuService>();


// =======================
// JWT Authentication
// =======================

var jwt = builder.Configuration.GetSection("Jwt");

var jwtKey = jwt["Key"]
    ?? throw new InvalidOperationException("Jwt:Key must be configured.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes long.");

var issuer = jwt["Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer must be configured.");
var audience = jwt["Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience must be configured.");
var key = Encoding.UTF8.GetBytes(jwtKey);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),

            ValidateLifetime = true
        };
    });


builder.Services.AddAuthorization();


// =======================
// Build Application
// =======================

var app = builder.Build();


// =======================
// Database Seed
// =======================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(

            new MenuItem
            {
                Name = "Kottu Roti",
                Price = 750m,
                Category = "Mains"
            },

            new MenuItem
            {
                Name = "Fried Rice",
                Price = 850m,
                Category = "Mains"
            },

            new MenuItem
            {
                Name = "Watalappan",
                Price = 350m,
                Category = "Dessert"
            }

        );


        db.SaveChanges();
    }
}


// =======================
// HTTP Pipeline
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();


app.UseCors("Frontend");


// JWT Middleware
app.UseAuthentication();   // who are you?  (validates JWT) 
app.UseAuthorization();    // may you?      (checks roles) 


app.MapControllers();


app.Run();
