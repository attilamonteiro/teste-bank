using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace EstudoApi.Configuration
{
    public static class MediatRConfiguration
    {
        public static IServiceCollection AddMediatRConfiguration(this IServiceCollection services)
        {
            services.AddMediatR(typeof(EstudoApi.Domain.CQRS.Handlers.ContaBancaria.CreateContaBancariaCommandHandler));
            return services;
        }
    }
}
