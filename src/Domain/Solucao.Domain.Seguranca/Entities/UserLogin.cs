namespace Solucao.Domain.Seguranca.Entities;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("UserLogin")]
public class UserLogin : IdentityUserLogin<Guid>
{
}
