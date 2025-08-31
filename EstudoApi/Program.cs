
using MediatR;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using EstudoApi.Domain.Configuration;
using EstudoApi.Infrastructure.Configuration;
using EstudoApi.Infrastructure.Auth;
using EstudoApi.Infrastructure.Contexts;
using EstudoApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);


        // MediatR registration
        builder.Services.AddMediatR(
            typeof(EstudoApi.Domain.CQRS.Handlers.Account.CreateAccountCommandHandler),
            typeof(EstudoApi.Domain.CQRS.Handlers.Account.LoginAccountCommandHandler),
            typeof(EstudoApi.Domain.CQRS.Handlers.Account.GetAccountBalanceQueryHandler)
        );
        builder.Services.AddScoped<EstudoApi.Infrastructure.CQRS.Handlers.LoginUserCommandHandler>();

        builder.Services.ConfigureInfrastructureDependencies();
        builder.Services.ConfigureDomainDependencies();
        builder.Services.ConfigureAutoMapperependencies();

        builder.Services.AddConnections(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "EstudoApi", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header usando o esquema Bearer. Exemplo: 'Bearer {token}'",
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
            c.ExampleFilters();
        });
        builder.Services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.CreateAccountCommandExample>();
        builder.Services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.RegisterUserRequestExample>();
        builder.Services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.LoginRequestExample>();
        builder.Services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.LoginAccountCommandExample>();

        // Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
        {
            opt.Password.RequiredLength = 8;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireUppercase = false;
            opt.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
        builder.Services.AddSingleton(jwt);

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<EstudoApi.Domain.Contracts.IJwtTokenService, JwtTokenService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
