

using MediatR;
using EstudoApi.Configuration;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using EstudoApi.Domain.Configuration;
using EstudoApi.Infrastructure.Configuration;
// using EstudoApi.Infrastructure.Auth;
using EstudoApi.Infrastructure.Contexts;
using EstudoApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public partial class Program
{
    public static void Main(string[] args)
    {
        // Garante que claims customizadas não sejam reescritas
        System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        var builder = WebApplication.CreateBuilder(args);


        builder.Services.AddMediatRConfiguration();

        builder.Services.ConfigureInfrastructureDependencies();
        builder.Services.ConfigureDomainDependencies();
        builder.Services.ConfigureAutoMapperependencies();

        builder.Services.AddConnections(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerConfiguration();

        // Identity
        builder.Services.AddIdentityConfiguration();

        // JWT Configuration
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<EstudoApi.Infrastructure.Auth.JwtOptions>()!;
        builder.Services.Configure<EstudoApi.Infrastructure.Auth.JwtOptions>(builder.Configuration.GetSection("Jwt"));
        builder.Services.AddJwtAuthentication(jwtOptions);
        
        // JWT Token Service for DI
        builder.Services.AddScoped<EstudoApi.Domain.Contracts.IJwtTokenService, EstudoApi.Infrastructure.Auth.JwtTokenService>();
        
        builder.Services.AddAuthorization();

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
