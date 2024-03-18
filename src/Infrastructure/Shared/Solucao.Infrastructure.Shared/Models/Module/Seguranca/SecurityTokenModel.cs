namespace Solucao.Infrastructure.Shared.Models.Module.Seguranca;

#nullable enable

public class SecurityTokenModel : ISecurityToken
{
    public Guid? Id { get; set; }
    public string? ConcurrencyToken { get; set; } = Guid.NewGuid().ToString();
    public object? Application { get; set; }
    public object? Authorization { get; set; }

    public string? Payload { get; set; }
    public string? Properties { get; set; }
    public string? ReferenceId { get; set; }
    public string? Status { get; set; }
    public string? Subject { get; set; }
    public string? Type { get; set; }

    public DateTime? CreationDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? RedemptionDate { get; set; }
}
