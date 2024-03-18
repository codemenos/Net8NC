namespace Solucao.Infrastructure.Data.Seguranca.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Infrastructure.Data.Seguranca.Configurations;

public class SegurancaContext : IdentityDbContext<SecurityUser, SecurityRole, Guid, SecurityUserClaim, SecurityUserRole, SecurityUserLogin, SecurityRoleClaim, SecurityUserToken>
{
    public SegurancaContext(DbContextOptions<SegurancaContext> options) : base(options)
    {
        //Rodar da pasta raiz da solução
        //dotnet ef migrations add Inicial --project .\src\Infrastructure\Data\Solucao.Infrastructure.Data.Seguranca\Solucao.Infrastructure.Data.Seguranca.csproj --startup-project .\src\Service\API\Solucao.Service.API.Seguranca\Solucao.Service.API.Seguranca.csproj --output-dir Migrations --context SegurancaContext
        //dotnet ef migrations remove --project .\src\Infrastructure\Data\Solucao.Infrastructure.Data.Seguranca\Solucao.Infrastructure.Data.Seguranca.csproj --startup-project .\src\Service\API\Solucao.Service.API.Seguranca\Solucao.Service.API.Seguranca.csproj --context SegurancaContext

        Database.Migrate();
    }

    public override DbSet<SecurityUser> Users { get; set; }
    public override DbSet<SecurityRole> Roles { get; set; }
    public override DbSet<SecurityUserClaim> UserClaims { get; set; }
    public override DbSet<SecurityUserRole> UserRoles { get; set; }
    public override DbSet<SecurityUserLogin> UserLogins { get; set; }
    public override DbSet<SecurityRoleClaim> RoleClaims { get; set; }
    public override DbSet<SecurityUserToken> UserTokens { get; set; }
    public DbSet<SecurityApplication> SecurityApplications { get; set; }
    public DbSet<SecurityAuthorization> SecurityAuthorizations { get; set; }
    public DbSet<SecurityScope> SecurityScopes { get; set; }
    public DbSet<SecurityToken> SecurityTokens { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new SecurityUserConfiguration());
        builder.ApplyConfiguration(new SecurityRoleConfiguration());
        builder.ApplyConfiguration(new SecurityUserClaimConfiguration());
        builder.ApplyConfiguration(new SecurityUserRoleConfiguration());
        builder.ApplyConfiguration(new SecurityUserLoginConfiguration());
        builder.ApplyConfiguration(new SecurityRoleClaimConfiguration());
        builder.ApplyConfiguration(new SecurityUserTokenConfiguration());
        builder.ApplyConfiguration(new SecurityApplicationConfiguration());
        builder.ApplyConfiguration(new SecurityAuthorizationConfiguration());
        builder.ApplyConfiguration(new SecurityScopeConfiguration());
        builder.ApplyConfiguration(new SecurityTokenConfiguration());
    }

}
