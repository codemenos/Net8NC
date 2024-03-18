namespace Solucao.Domain.Seguranca.Aggregates;

using OpenIddict.EntityFrameworkCore.Models;

public class SecurityApplication : OpenIddictEntityFrameworkCoreApplication<Guid, SecurityAuthorization, SecurityToken>
{

}
