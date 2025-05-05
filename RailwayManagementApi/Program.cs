using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RailwayManagementApi.Helper;
using RailwayManagementApi.Models;
using Microsoft.AspNetCore.Identity;
using RailwayManagementApi.Services;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// DBContext
builder.Services.AddDbContext<RailwayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<RailwayContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<TicketHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITrainService,TrainService>();
builder.Services.AddScoped<IPayment, PaymentService>();
builder.Services.AddScoped<IStationService, StationService>();

// JWT

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        ),
    };
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Authentication API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Description = "Enter JWT Bearer token only",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


// Add services to the container.
builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = new[] { "User", "Admin" };

    foreach (var role in roles)
    {
        var roleExists = await roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Railway API v1");
        c.RoutePrefix = string.Empty; // Swagger at root (localhost:5000/)
    }
    );
}

// Admin seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminRole = "Admin";
    string adminEmail = "admin@railway.com";
    string adminUsername = "admin";
    string adminPassword = "Admin@123";
    string adminAadhar = "123456789012";

    // Create roles if not exist
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Create admin user if not exist
    var existingAdmin = await userManager.FindByNameAsync(adminUsername);
    if (existingAdmin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminUsername,
            Email = adminEmail,
            AadharNumber = adminAadhar
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
            Console.WriteLine(" Admin seeded successfully.");
        }
        else
        {
            Console.WriteLine(" Failed to seed admin user: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
