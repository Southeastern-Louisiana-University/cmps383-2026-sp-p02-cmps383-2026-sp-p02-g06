using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Selu383.SP26.Api.Features.User;

public class Role : IdentityRole<int>
{
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
public class CreateRoleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class UpdateRoleDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class RoleDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}