namespace Solucao.Domain.Seguranca.Aggregates;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("Role")]
public class Role: IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }

    public Role(string roleName) : this()
    {
        Name = roleName;
    }
}
