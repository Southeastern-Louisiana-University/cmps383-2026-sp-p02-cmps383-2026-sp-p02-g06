using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.User;

// keep IdentityUserRole<int> for the keys; expose navigations only
public class UserRole : IdentityUserRole<int>
{
    public virtual User User { get; set; }
    public virtual Role Role { get; set; }
}
