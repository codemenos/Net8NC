namespace Solucao.Domain.Seguranca.Aggregates;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

[Table("User")]
public class User : IdentityUser<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public User(string userName) : this()
    {
        UserName = userName;
    }
}
