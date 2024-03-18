namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

using System;
using System.Collections.Generic;

public interface ISecurityAuthorization
{
    Guid? Id { get; set; }
    string? ConcurrencyToken { get; set; }
    DateTime? CreationDate { get; set; }
    string? Subject { get; set; }
    object? Application { get; set; }
    string? Status { get; set; }
    string? Properties { get; set; }
    string? Scopes { get; set; }
    string? Type { get; set; }
    ICollection<object> Tokens { get; }
}