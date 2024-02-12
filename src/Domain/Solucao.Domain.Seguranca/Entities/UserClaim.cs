namespace Solucao.Domain.Seguranca.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("UserClaim")]
public class UserClaim : IdentityUserClaim<Guid>
{    

}
