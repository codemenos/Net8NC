namespace Solucao.Domain.Seguranca.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("RoleClaim")]
public class RoleClaim : IdentityRoleClaim<Guid>
{
}
