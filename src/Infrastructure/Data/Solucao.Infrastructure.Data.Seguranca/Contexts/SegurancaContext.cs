namespace Solucao.Infrastructure.Data.Seguranca.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;
using Solucao.Infrastructure.Data.Seguranca.Configurations;

public class SegurancaContext : IdentityDbContext<SecurityUser, SecurityRole, Guid, SecurityUserClaim, SecurityUserRole, SecurityUserLogin, SecurityRoleClaim, SecurityUserToken>
{
    private const string IDENTITY_USER = "SecurityUser";
    private const string IDENTITY_ROLE = "SecurityRole";
    private const string IDENTITY_USER_CLAIM = "SecurityUserClaim";
    private const string IDENTITY_USER_ROLE = "SecurityUserRole";
    private const string IDENTITY_USER_LOGIN = "SecurityUserLogin";
    private const string IDENTITY_ROLE_CLAIM = "SecurityRoleClaim";
    private const string IDENTITY_USER_TOKEN = "SecurityUserToken";

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

        builder.Entity<SecurityUser>(b => { b.ToTable(IDENTITY_USER); });
        builder.Entity<SecurityRole>(b => { b.ToTable(IDENTITY_ROLE); });
        builder.Entity<SecurityUserClaim>(b => { b.ToTable(IDENTITY_USER_CLAIM); });
        builder.Entity<SecurityUserRole>(b => { b.ToTable(IDENTITY_USER_ROLE); });
        builder.Entity<SecurityUserLogin>(b => { b.ToTable(IDENTITY_USER_LOGIN); b.HasKey(e => new { e.LoginProvider, e.ProviderKey }); });
        builder.Entity<SecurityRoleClaim>(b => { b.ToTable(IDENTITY_ROLE_CLAIM); });
        builder.Entity<SecurityUserToken>(b => { b.ToTable(IDENTITY_USER_TOKEN); });

        builder.ApplyConfiguration(new SecurityApplicationConfiguration());
        builder.ApplyConfiguration(new SecurityAuthorizationConfiguration());
        builder.ApplyConfiguration(new SecurityScopeConfiguration());
        builder.ApplyConfiguration(new SecurityTokenConfiguration());
    }

}
