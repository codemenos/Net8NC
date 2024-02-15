namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Aggregates;

public class SecurityAuthorizationConfiguration : IEntityTypeConfiguration<SecurityAuthorization>
{
    public void Configure(EntityTypeBuilder<SecurityAuthorization> builder)
    {
        builder.ToTable("SecurityAuthorizations");
    }
}