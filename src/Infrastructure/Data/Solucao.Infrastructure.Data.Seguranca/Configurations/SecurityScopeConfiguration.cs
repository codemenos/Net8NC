namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityScopeConfiguration : IEntityTypeConfiguration<SecurityScope>
{
    public void Configure(EntityTypeBuilder<SecurityScope> builder)
    {
        builder.ToTable("SecurityScopes");
    }
}
