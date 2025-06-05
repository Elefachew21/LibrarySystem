using LibraryWebAPI.Data;
using LibraryWebAPI.Middleware;
using LibraryWebAPI.Models;
using AutoMapper;
using LibraryWebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.IO; // Added for Directory.GetCurrentDirectory() if needed for DbContextFactory

var builder = WebApplication.CreateBuilder(args);

// Add services to the container

// 1. Add DbContext with SQL Server and connection string
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// 3. Add ASP.NET Core Identity
// Ensure you have these NuGet packages installed in LibraryWebAPI project:
// Microsoft.AspNetCore.Identity.EntityFrameworkCore
// Microsoft.EntityFrameworkCore.SqlServer
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Optional: Configure Identity options like password requirements, lockout settings etc.
    // These settings directly impact user registration (e.g., "Passwords must be at least 6 characters")
    options.SignIn.RequireConfirmedAccount = false; // Set to true if you want email confirmation
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6; // Minimum length
    options.Password.RequireNonAlphanumeric = true; // At least one special character
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true; // Ensure unique email for users
})
.AddEntityFrameworkStores<LibraryDbContext>() // Tells Identity to use your DbContext
.AddDefaultTokenProviders(); // For things like password reset tokens

// 4. Add Authentication with JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// 5. Register your custom application services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IBorrowerService, BorrowerService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 6. Add Controllers (for API endpoints)
builder.Services.AddControllers();

// 7. Configure Swagger/OpenAPI for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Library API", Version = "v1" });

    // Configure Swagger for JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer YOUR_TOKEN_HERE')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer" // The scheme name must be lowercase for Swagger UI to correctly interpret it
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

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline (middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Enforces HTTPS (remove if not using HTTPS)

// Authentication and Authorization middleware must be placed before MapControllers
app.UseAuthentication();
app.UseAuthorization();

// Add custom middleware (ensure these are correctly implemented)
// Example: app.UseMiddleware<ExceptionMiddleware>();
// Example: app.UseMiddleware<LoggingMiddleware>();

app.MapControllers(); // Maps controller routes

// Seed database with initial data (users, roles, etc.)
// This block ensures the database is created and seeded on application startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LibraryDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created and apply any pending migrations
        context.Database.Migrate(); // This will apply all pending migrations

        // Seed initial data (e.g., admin user, roles)
        await DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        // Consider re-throwing or handling the exception based on your application's needs
    }
}

app.Run(); // Runs the application
