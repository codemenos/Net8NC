namespace Solucao.Domain.Seguranca.Entities;

using OpenIddict.EntityFrameworkCore.Models;

public class SecurityScope : OpenIddictEntityFrameworkCoreScope<Guid>
{
    public SecurityScope()
    {
        Id = Guid.NewGuid();
    }
}
