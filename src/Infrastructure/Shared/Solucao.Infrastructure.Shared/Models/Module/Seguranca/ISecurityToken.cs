namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

public interface ISecurityToken
{
    Guid? Id { get; set; }
    string? ConcurrencyToken { get; set; }
    object? Application { get; set; }
    object? Authorization { get; set; }

    string? Payload { get; set; }
    string? Properties { get; set; }
    string? ReferenceId { get; set; }
    string? Status { get; set; }
    string? Subject { get; set; }
    string? Type { get; set; }

    DateTime? CreationDate { get; set; }
    DateTime? ExpirationDate { get; set; }
    DateTime? RedemptionDate { get; set; }
}