using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Features.User;

namespace Selu383.SP26.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = "Admin")] // Only admins can manage roles
public class RoleController : ControllerBase
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public RoleController(
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // Create a new role
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
    {
        // Check if role name already exists
        var existingRole = await _roleManager.FindByNameAsync(createRoleDto.Name);
        if (existingRole != null)
        {
            return BadRequest(new { message = "Role name already exists" });
        }

        // Create the role
        var role = new Role
        {
            Name = createRoleDto.Name
        };

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name
        };

        return Ok(roleDto);
    }

    // Get all roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
    {
        var roles = await _roleManager.Roles.ToListAsync();

        var roleDtos = roles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name
        }).ToList();

        return Ok(roleDtos);
    }

    // Get role by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role == null)
        {
            return BadRequest(new { message = "Role not found" });
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name
        };

        return Ok(roleDto);
    }

    // Update role
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, [FromBody] UpdateRoleDto updateRoleDto)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role == null)
        {
            return BadRequest(new { message = "Role not found" });
        }

        // Check if new name already exists (if changing name)
        if (role.Name != updateRoleDto.Name)
        {
            var existingRole = await _roleManager.FindByNameAsync(updateRoleDto.Name);
            if (existingRole != null)
            {
                return BadRequest(new { message = "Role name already exists" });
            }
            role.Name = updateRoleDto.Name;
        }

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name
        };

        return Ok(roleDto);
    }

    // Delete role
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role == null)
        {
            return BadRequest(new { message = "Role not found" });
        }

        // Check if any users have this role
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        if (usersInRole.Any())
        {
            return BadRequest(new { message = $"Cannot delete role. {usersInRole.Count} user(s) are assigned to this role" });
        }

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { message = "Role deleted successfully" });
    }

    // Get users in a specific role
    [HttpGet("{id}/users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersInRole(int id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());

        if (role == null)
        {
            return BadRequest(new { message = "Role not found" });
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

        var userDtos = new List<UserDto>();

        foreach (var user in usersInRole)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Roles = userRoles.ToArray()
            });
        }

        return Ok(userDtos);
    }
}