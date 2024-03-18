namespace Solucao.Infrastructure.Data.Seguranca.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Entities;

public class SecurityUserRoleConfiguration : IEntityTypeConfiguration<SecurityUserRole>
{
    public void Configure(EntityTypeBuilder<SecurityUserRole> builder)
    {
        builder.ToTable("SecurityUserRole");
    }
}