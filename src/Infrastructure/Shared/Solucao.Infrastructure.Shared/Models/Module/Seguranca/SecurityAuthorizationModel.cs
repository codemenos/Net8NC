namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

using System;
using System.Collections.Generic;

public class SecurityAuthorizationModel : ISecurityAuthorization
{
    public object? Application { get; set; }
    public string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
    public DateTime? CreationDate { get; set; }
    public Guid? Id { get; set; }
    public string? Properties { get; set; }
    public string? Scopes { get; set; }
    public string? Status { get; set; }
    public string? Subject { get; set; }
    public ICollection<object> Tokens { get; } = new HashSet<object>();
    public string? Type { get; set; }
}
