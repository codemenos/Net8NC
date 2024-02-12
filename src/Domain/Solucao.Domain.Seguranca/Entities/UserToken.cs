namespace Solucao.Domain.Seguranca.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("UserToken")]
public class UserToken : IdentityUserToken<Guid>
{
}
