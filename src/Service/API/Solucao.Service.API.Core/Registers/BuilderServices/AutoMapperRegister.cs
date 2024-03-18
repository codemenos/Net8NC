namespace Solucao.Service.API.Core.Registers.BuilderServices;

using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Solucao.Application.Seguranca.ProfileMaps;

public static class AutoMapperRegister
{
    public static IServiceCollection AddAutoMapper(this IServiceCollection services, params Type[] profileTypes)
    {
        //var config = new MapperConfiguration(cfg =>
        //{
        //    cfg.AddProfile<ClienteProfile>(); 
        //});

        //var mapper = config.CreateMapper();
        //services.AddSingleton(mapper);

        return services;
    }
}