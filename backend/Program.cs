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
    ));


// =======================
// Application Services
// =======================

builder.Services.AddScoped<IMenuService, MenuService>();


// =======================
// JWT Authentication
// =======================

var jwt = builder.Configuration.GetSection("Jwt");

var key = Encoding.UTF8.GetBytes(jwt["Key"]!);


builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwt["Audience"],

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

    db.Database.EnsureCreated();


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


// JWT Middleware
app.UseAuthentication();   // who are you?  (validates JWT) 
app.UseAuthorization();    // may you?      (checks roles) 


app.MapControllers();


app.Run();