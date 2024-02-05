namespace Solucao.Infrastructure.Data.Seguranca.Contexts;

using Microsoft.EntityFrameworkCore;

public class SegurancaContext : DbContext
{
    public SegurancaContext(DbContextOptions<SegurancaContext> options) : base(options) 
    {
        Database.Migrate();
    }

}
