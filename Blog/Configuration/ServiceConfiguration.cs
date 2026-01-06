using System.Text;
using Blog.Data;
using Blog.Repository;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Blog.Configuration;

public static class ServiceConfiguration
{
    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Host=localhost:5433;Database=blog;Username=alfheim_admin;password=password";
        services.AddDbContext<BlogDbContext>(options =>
            options.UseNpgsql(connectionString));
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<ArticleRepository>();
        services.AddScoped<TagRepository>();
        services.AddScoped<UserRepository>();
        services.AddScoped<UserCredentialRepository>();
    }

    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "ThisIsASecretKeyForDevelopmentOnly1234567890";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, // Set to true in production with proper config
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var userCredentialRepository = context.HttpContext.RequestServices.GetRequiredService<UserCredentialRepository>();
                    var userIdClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                    var iatClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Iat);

                    if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) && 
                        iatClaim != null && long.TryParse(iatClaim.Value, out var iat))
                    {
                        var credential = await userCredentialRepository.FindByIdAsync(userId);
                        if (credential != null && credential.LastChangedAt.HasValue)
                        {
                            var issueTime = DateTimeOffset.FromUnixTimeSeconds(iat).UtcDateTime;
                            if (credential.LastChangedAt.Value > issueTime)
                            {
                                context.Fail("Token is invalid due to password change.");
                            }
                        }
                    }
                }
            };
        });
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer"
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
    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }
}
