using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Users;

public class UserRole : IdentityUserRole<int>
{
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}