namespace Solucao.Domain.Seguranca.Aggregates;

using OpenIddict.EntityFrameworkCore.Models;

public class SecurityAuthorization : OpenIddictEntityFrameworkCoreAuthorization<Guid, SecurityApplication, SecurityToken>
{
    public SecurityAuthorization()
    {
        Id = Guid.NewGuid();
    }
}
