using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using EstudoApi.Banking.Configuration;
using EstudoApi.Domain.Configuration;
using EstudoApi.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configurar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EstudoApi.Banking",
        Version = "v1",
        Description = "API especializada em operações bancárias e transferências"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
});

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

Console.WriteLine($"DEBUG: JWT Key = '{jwtKey}'");
Console.WriteLine($"DEBUG: JWT Issuer = '{jwtIssuer}'");
Console.WriteLine($"DEBUG: Environment = '{builder.Environment.EnvironmentName}'");

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException("JWT Key e Issuer devem estar configurados");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Configurar banco de dados e dependências
builder.Services.AddConnections(builder.Configuration);
builder.Services.ConfigureInfrastructureDependencies();
builder.Services.ConfigureDomainDependencies();
builder.Services.ConfigureBankingServices();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Dump presence of TransferCommandHandler type before pipeline registration
try
{
    var asm = typeof(EstudoApi.Banking.Configuration.BankingDependencyConfiguration).Assembly;
    var handlerType = asm.GetType("EstudoApi.Banking.Transfer.Handlers.TransferCommandHandler");
    Console.WriteLine(handlerType == null ? "[StartupDiag] Handler type NOT in assembly" : "[StartupDiag] Handler type found in assembly: " + handlerType.FullName);
}
catch (Exception ex)
{
    Console.WriteLine("[StartupDiag] Exception checking handler type: " + ex.Message);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EstudoApi.Banking v1");
        c.RoutePrefix = string.Empty; // Para acessar o Swagger na raiz
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Log de inicialização
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("EstudoApi.Banking iniciado em {Environment} na porta 8081",
    app.Environment.EnvironmentName);

app.Run();

// Classe Program pública para testes de integração
namespace EstudoApi.Banking
{
    public partial class Program { }
}
