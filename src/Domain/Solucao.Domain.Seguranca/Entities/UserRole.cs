namespace Solucao.Domain.Seguranca.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("UserRole")]
public class UserRole : IdentityUserRole<Guid>
{
}
