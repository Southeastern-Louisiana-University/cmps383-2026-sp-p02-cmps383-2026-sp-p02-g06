using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.User;

public class Role : IdentityRole<int>
{
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
