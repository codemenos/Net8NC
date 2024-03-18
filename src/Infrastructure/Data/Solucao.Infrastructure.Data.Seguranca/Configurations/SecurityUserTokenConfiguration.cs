namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityUserTokenConfiguration : IEntityTypeConfiguration<SecurityUserToken>
{
    public void Configure(EntityTypeBuilder<SecurityUserToken> builder)
    {
        builder.ToTable("SecurityUserToken");
    }
}
