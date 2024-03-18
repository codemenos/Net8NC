namespace Solucao.Application.Seguranca.ProfileMaps;

using AutoMapper;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Infrastructure.Shared.Models;
using Solucao.Infrastructure.Shared.Models.Module.Seguranca;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {

        
        //CreateMap<SecurityApplication, ClienteModel>()
        //.ForMember(dest => dest.TipoAplicativo, opt => opt.MapFrom(src => src.ApplicationType ?? string.Empty))
        //.ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.ClientId ?? string.Empty))
        //.ForMember(dest => dest.SegredoCliente, opt => opt.MapFrom(src => src.ClientSecret ?? string.Empty))
        //.ForMember(dest => dest.TipoCliente, opt => opt.MapFrom(src => src.ClientType ?? string.Empty))
        //.ForMember(dest => dest.TipoConsentimento, opt => opt.MapFrom(src => src.ConsentType ?? string.Empty))
        //.ForMember(dest => dest.NomeExibicao, opt => opt.MapFrom(src => src.DisplayName ?? string.Empty))
        //.ForMember(dest => dest.NomesExibicao, opt => opt.MapFrom(src => src.DisplayNames != null ? DeserializeJsonToList(src.DisplayNames) : new List<string>()))
        //.ForMember(dest => dest.ConjuntoChavesJsonWeb, opt => opt.MapFrom(src => src.JsonWebKeySet ?? string.Empty))
        //.ForMember(dest => dest.Permissoes, opt => opt.MapFrom(src => src.Permissions != null ? DeserializeJsonToList(src.Permissions) : new List<string>()))
        //.ForMember(dest => dest.UrlsRedirecionamentoAposLogout, opt => opt.MapFrom(src => src.PostLogoutRedirectUris != null ? DeserializeJsonToList(src.PostLogoutRedirectUris) : new List<string>()))
        //.ForMember(dest => dest.Propriedades, opt => opt.MapFrom(src => src.Properties != null ? DeserializeJsonToDictionary(src.Properties) : new Dictionary<string, string>()))
        //.ForMember(dest => dest.UrlsRedirecionamento, opt => opt.MapFrom(src => src.RedirectUris != null ? DeserializeJsonToList(src.RedirectUris) : new List<string>()))
        //.ForMember(dest => dest.Requisitos, opt => opt.MapFrom(src => src.Requirements != null ? DeserializeJsonToList(src.Requirements) : new List<string>()))
        //.ForMember(dest => dest.Configuracoes, opt => opt.MapFrom(src => src.Settings ?? string.Empty));

        //CreateMap<List<SecurityApplication>, List<ClienteModel>>();

        //CreateMap<ClienteModel, SecurityApplication>()
        //    .ForMember(dest => dest.ApplicationType, opt => opt.MapFrom(src => src.TipoAplicativo))
        //    .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClienteId))
        //    .ForMember(dest => dest.ClientSecret, opt => opt.MapFrom(src => src.SegredoCliente))
        //    .ForMember(dest => dest.ClientType, opt => opt.MapFrom(src => src.TipoCliente))
        //    .ForMember(dest => dest.ConsentType, opt => opt.MapFrom(src => src.TipoConsentimento))
        //    .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.NomeExibicao))
        //    .ForMember(dest => dest.DisplayNames, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.NomesExibicao)))
        //    .ForMember(dest => dest.JsonWebKeySet, opt => opt.MapFrom(src => src.ConjuntoChavesJsonWeb))
        //    .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Permissoes)))
        //    .ForMember(dest => dest.PostLogoutRedirectUris, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.UrlsRedirecionamentoAposLogout)))
        //    .ForMember(dest => dest.Properties, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Propriedades)))
        //    .ForMember(dest => dest.RedirectUris, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.UrlsRedirecionamento)))
        //    .ForMember(dest => dest.Requirements, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Requisitos)))
        //    .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => src.Configuracoes));

        //CreateMap<List<ClienteModel>, List<SecurityApplication>>();
    }

    private List<string> DeserializeJsonToList(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        return JsonConvert.DeserializeObject<List<string>>(json);
    }

    private Dictionary<string, string> DeserializeJsonToDictionary(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new Dictionary<string, string>();

        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
}
