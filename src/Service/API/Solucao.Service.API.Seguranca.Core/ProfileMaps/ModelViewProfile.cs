namespace Solucao.Service.API.Seguranca.Core.ProfileMaps;

using AutoMapper;
using Solucao.Infrastructure.Shared.Models.Module.Seguranca;
using Solucao.Service.API.Seguranca.Core.ViewModels;

public class ModelViewProfile : Profile
{
    public ModelViewProfile()
    {
        CreateMap<IClientViewModel, ISecurityApplication>()
            .ConstructUsing((src, ctx) => new SecurityApplicationModel())
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClienteId))
            .ForMember(dest => dest.ClientSecret, opt => opt.MapFrom(src => src.ClienteSecretKey))
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.ClienteNome))
            .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => src.TipoAplicativoCliente))
            .ForMember(dest => dest.ClientType, opt => opt.MapFrom(src => src.TipoCliente))
            .ForMember(dest => dest.ConsentType, opt => opt.MapFrom(src => src.TipoConsentimento))
            .ForMember(dest => dest.DisplayNames, opt => opt.MapFrom(src => src.DisplayNames))
            .ForMember(dest => dest.JsonWebKeySet, opt => opt.MapFrom(src => src.JsonWebKeySet))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissoes))
            .ForMember(dest => dest.PostLogoutRedirectUris, opt => opt.MapFrom(src => ConvertToString(src.UrlsRedirecionamentoAposLogout)))
            .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => src.Propriedades))
            .ForMember(dest => dest.RedirectUris, opt => opt.MapFrom(src => ConvertToString(src.UrlsRedirecionamento)))
            .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => src.Requisitos))
            .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Propriedades))
            .ForMember(dest => dest.Authorizations, opt => opt.Ignore()) 
            .ForMember(dest => dest.Tokens, opt => opt.Ignore()); 


        CreateMap<ISecurityApplication, IClientViewModel>()
            .ConstructUsing((src, ctx) => new ClientViewModel())
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.ClientId))
            .ForMember(dest => dest.ClienteSecretKey, opt => opt.MapFrom(src => src.ClientSecret))
            .ForMember(dest => dest.ClienteNome, opt => opt.MapFrom(src => src.DisplayName))
            .ForMember(dest => dest.TipoAplicativoCliente, opt => opt.MapFrom(src => src.ApplicationType))
            .ForMember(dest => dest.TipoCliente, opt => opt.MapFrom(src => src.ClientType))
            .ForMember(dest => dest.TipoConsentimento, opt => opt.MapFrom(src => src.ConsentType))
            .ForMember(dest => dest.DisplayNames, opt => opt.MapFrom(src => src.DisplayNames))
            .ForMember(dest => dest.JsonWebKeySet, opt => opt.MapFrom(src => src.JsonWebKeySet))
            .ForMember(dest => dest.Permissoes, opt => opt.MapFrom(src => src.Permissions))
            .ForMember(dest => dest.UrlsRedirecionamentoAposLogout, opt => opt.MapFrom(src => ConvertToList(src.PostLogoutRedirectUris)))
            .ForMember(dest => dest.Propriedades, opt => opt.MapFrom(src => src.Properties))
            .ForMember(dest => dest.UrlsRedirecionamento, opt => opt.MapFrom(src => ConvertToList(src.RedirectUris)))
            .ForMember(dest => dest.Requisitos, opt => opt.MapFrom(src => src.Requirements))
            .ForMember(dest => dest.Propriedades, opt => opt.MapFrom(src => src.Settings));

    }

    private static string ConvertToString(List<string> values)
    {
        if (values == null || !values.Any())
        {
            return "[]";
        }

        return "[" + string.Join(", ", values.Select(v => "\"" + v + "\"")) + "]";
    }

    private static List<string> ConvertToList(string value)
    {
        if (string.IsNullOrEmpty(value) || value == "[]")
        {
            return [];
        }

        return value.TrimStart('[').TrimEnd(']').Split(',').Select(s => s.Trim(' ', '"')).ToList();
    }
}
