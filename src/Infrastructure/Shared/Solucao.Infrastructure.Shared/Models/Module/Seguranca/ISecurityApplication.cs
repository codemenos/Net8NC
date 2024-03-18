namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

public interface ISecurityApplication
{
    string? ApplicationType { get; set; }
    ICollection<ISecurityAuthorization> Authorizations { get; }
    string? ClientId { get; set; }
    string? ClientSecret { get; set; }
    string? ClientType { get; set; }
    string? ConcurrencyToken { get; set; }
    string? ConsentType { get; set; }
    string? DisplayName { get; set; }
    string? DisplayNames { get; set; }
    Guid? Id { get; set; }
    string? JsonWebKeySet { get; set; }
    string? Permissions { get; set; }
    string? PostLogoutRedirectUris { get; set; }
    string? Properties { get; set; }
    string? RedirectUris { get; set; }
    string? Requirements { get; set; }
    string? Settings { get; set; }
    ICollection<ISecurityToken> Tokens { get; }
}
