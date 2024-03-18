namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityRoleClaimConfiguration : IEntityTypeConfiguration<SecurityRoleClaim>
{
    public void Configure(EntityTypeBuilder<SecurityRoleClaim> builder)
    {
        builder.ToTable("SecurityRoleClaim");
    }
}
