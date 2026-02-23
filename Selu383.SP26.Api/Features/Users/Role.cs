using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Users;

public class Role : IdentityRole<int>
{
    // Add this line to fix the error
    public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}