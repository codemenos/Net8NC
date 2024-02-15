namespace Solucao.Domain.Seguranca.Aggregates;

using OpenIddict.EntityFrameworkCore.Models;

public class SecurityToken : OpenIddictEntityFrameworkCoreToken<Guid, SecurityApplication, SecurityAuthorization>
{
    public SecurityToken()
    {
        Id = Guid.NewGuid();
    }
}
