using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.User;

namespace Selu383.SP26.Api.Controllers;

[ApiController]
[Route("api/controller")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    /*
    Plan (pseudocode):
    1. Validate input roles and username uniqueness as before.
    2. Create `user` instance and call `_userManager.CreateAsync(user, password)`.
    3. If creation succeeds, add roles to the same `user` instance (do not refetch).
       - Using the original `user` avoids a possible null return from FindByNameAsync
         and fixes the CS8604 nullability diagnostic.
    4. If role assignment fails, delete the user and return errors.
    5. Use the `user` instance to fetch roles and build the UserDto.
    6. Return the created `UserDto`.
    */

    // Register a new user
    [HttpPost]
    [Authorize(Roles = "Admin")] // Only admins can create users
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        // Validate that at least one role is provided
        if (createUserDto.Roles == null || createUserDto.Roles.Length == 0)
        {
            return BadRequest(new { message = "At least one role must be provided" });
        }

        // Check if username already exists
        var existingUser = await _userManager.FindByNameAsync(createUserDto.UserName);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Username already exists" });
        }

        // Validate that all roles exist
        foreach (var roleName in createUserDto.Roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return BadRequest(new { message = $"Role '{roleName}' does not exist" });
            }
        }

        // Create the user
        var user = new User
        {
            UserName = createUserDto.UserName
        };

        var result = await _userManager.CreateAsync(user, createUserDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        // Assign roles to the user
        var roleResult = await _userManager.AddToRolesAsync(user, createUserDto.Roles);

        if (!roleResult.Succeeded)
        {
            // If role assignment fails, delete the user and return error
            await _userManager.DeleteAsync(user);
            return BadRequest(new { errors = roleResult.Errors.Select(e => e.Description) });
        }

        // Use the created `user` instance (non-null) to get roles and build DTO.
        var userRoles = await _userManager.GetRolesAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = userRoles.ToArray()
        };

        return Ok(userDto);
    }


    // Login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Ok(new { message = "Login successful" });
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    // Get current user
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName
        });
    }

    // Update user
    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UpdateUserModel model)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        user.Email = model.Email;
        user.UserName = model.UserName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { message = "User updated successfully" });
        }

        return BadRequest(result.Errors);
    }

    // Delete user
    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return NotFound();

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return Ok(new { message = "User deleted successfully" });
        }

        return BadRequest(result.Errors);
    }

}