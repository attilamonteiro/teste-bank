using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace EstudoApi.Configuration
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EstudoApi", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header usando o esquema Bearer. Exemplo: 'Bearer {token}'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
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
                        new string[] {}
                    }
                });
                c.ExampleFilters();
            });
            services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.CreateAccountCommandExample>();
            services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.LoginAccountCommandExample>();
            services.AddSwaggerExamplesFromAssemblyOf<EstudoApi.SwaggerExamples.AccountMovementCommandExample>();
            return services;
        }
    }
}
