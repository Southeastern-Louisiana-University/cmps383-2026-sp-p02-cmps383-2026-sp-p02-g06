using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.User;

public class User : IdentityUser<int>
{
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
