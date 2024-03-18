namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityUserClaimConfiguration : IEntityTypeConfiguration<SecurityUserClaim>
{
    public void Configure(EntityTypeBuilder<SecurityUserClaim> builder)
    {
        builder.ToTable("SecurityUserClaim");
    }
}