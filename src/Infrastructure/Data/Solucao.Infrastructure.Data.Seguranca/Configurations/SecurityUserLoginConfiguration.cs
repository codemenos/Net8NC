namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityUserLoginConfiguration : IEntityTypeConfiguration<SecurityUserLogin>
{
    public void Configure(EntityTypeBuilder<SecurityUserLogin> builder)
    {
        builder.ToTable("SecurityUserLogin");

        builder.HasKey(e => new { e.LoginProvider, e.ProviderKey });
    }
}
