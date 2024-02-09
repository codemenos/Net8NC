namespace Solucao.Infrastructure.Data.Cadastro.Contexts;

using Microsoft.EntityFrameworkCore;

public class CadastroContext : DbContext
{
    public CadastroContext(DbContextOptions<CadastroContext> options) : base(options) 
    {
        Database.Migrate();
    }

}
