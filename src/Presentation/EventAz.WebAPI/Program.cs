using System.Text;
using EventAz.Application.Behaviors;
using EventAz.Application.Common;
using EventAz.Application.Interfaces;
using EventAz.Infrastructure.Services;
using EventAz.WebAPI.Services;
using EventAz.WebAPI.Middleware;
using EventAz.Persistence;
using EventAz.Persistence.Identity;
using EventAz.Persistence.Repositories;
using EventAz.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// --- DbContext + Identity ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false; // dev üçün sadələşdirmə
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
    throw new InvalidOperationException("Set JwtSettings:SecretKey to a secret with at least 32 bytes using User Secrets or an environment variable.");

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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// --- Servislər ---
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ITicketPurchaseRepository, TicketPurchaseRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IPromoCodeRepository, PromoCodeRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ITicketOperationsRepository, TicketOperationsRepository>();
builder.Services.AddScoped<IAdminStatisticsRepository, AdminStatisticsRepository>();
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<ITicketPurchaseEmailNotifier, TicketPurchaseEmailNotifier>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- MediatR + FluentValidation ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EventAz.Application.Features.Auth.Commands.Login.LoginCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(
    typeof(EventAz.Application.Features.Auth.Commands.Login.LoginCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT access token"
    });
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- Rolların seed edilməsi ---
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Organizer", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var defaultCategories = new[] { "Musiqi", "Teatr", "Festival", "İdman", "Konfrans", "İncəsənət" };
    var existingCategoryNames = await dbContext.Categories.Select(category => category.Name).ToListAsync();
    foreach (var categoryName in defaultCategories)
    {
        if (!existingCategoryNames.Contains(categoryName, StringComparer.OrdinalIgnoreCase))
            dbContext.Categories.Add(new Category { Name = categoryName });
    }
    await dbContext.SaveChangesAsync();
}

app.Run();
