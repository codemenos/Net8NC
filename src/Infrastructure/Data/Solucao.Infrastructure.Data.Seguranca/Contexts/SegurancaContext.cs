namespace Solucao.Infrastructure.Data.Seguranca.Contexts;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Solucao.Domain.Seguranca.Aggregates;
using Solucao.Domain.Seguranca.Entities;

public class SegurancaContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public SegurancaContext(DbContextOptions<SegurancaContext> options) : base(options)
    {
        //Rodar da pasta raiz da solução
        //dotnet ef migrations add Inicial --project .\src\Infrastructure\Data\Solucao.Infrastructure.Data.Seguranca\Solucao.Infrastructure.Data.Seguranca.csproj --startup-project .\src\Service\API\Solucao.Service.API.Seguranca\Solucao.Service.API.Seguranca.csproj --output-dir Migrations --context SegurancaContext

        Database.Migrate();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

}
