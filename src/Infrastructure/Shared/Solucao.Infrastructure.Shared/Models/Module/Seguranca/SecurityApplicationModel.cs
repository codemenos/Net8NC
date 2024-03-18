namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

using System;
using System.Collections.Generic;

public class SecurityApplicationModel : ISecurityApplication
{
    public string? ApplicationType { get; set; }
    public ICollection<ISecurityAuthorization> Authorizations { get; } = new List<ISecurityAuthorization>();
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ClientType { get; set; }
    public string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
    public string? ConsentType { get; set; }
    public string? DisplayName { get; set; }
    public string? DisplayNames { get; set; }
    public Guid? Id { get; set; }
    public string? JsonWebKeySet { get; set; }
    public string? Permissions { get; set; }
    public string? PostLogoutRedirectUris { get; set; }
    public string? Properties { get; set; }
    public string? RedirectUris { get; set; }
    public string? Requirements { get; set; }
    public string? Settings { get; set; }
    public ICollection<ISecurityToken> Tokens { get; } = new List<ISecurityToken>();
}
