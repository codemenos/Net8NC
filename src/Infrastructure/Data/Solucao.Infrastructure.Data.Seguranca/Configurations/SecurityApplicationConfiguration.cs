namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Solucao.Domain.Seguranca.Aggregates;

public class SecurityApplicationConfiguration : IEntityTypeConfiguration<SecurityApplication>
{
    public void Configure(EntityTypeBuilder<SecurityApplication> builder)
    {
        builder.ToTable("SecurityApplications");
    }
}
