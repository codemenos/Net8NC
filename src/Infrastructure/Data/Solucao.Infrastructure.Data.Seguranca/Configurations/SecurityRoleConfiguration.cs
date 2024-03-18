namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Aggregates;

public class SecurityRoleConfiguration : IEntityTypeConfiguration<SecurityRole>
{
    public void Configure(EntityTypeBuilder<SecurityRole> builder)
    {
        builder.ToTable("SecurityRole");
    }
}
