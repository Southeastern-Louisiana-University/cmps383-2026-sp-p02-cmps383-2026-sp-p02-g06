using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController(SignInManager<User> signInManager, UserManager<User> userManager) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var result = await signInManager.PasswordSignInAsync(dto.UserName, dto.Password, false, false);
        if (!result.Succeeded) return BadRequest();

        var user = await userManager.FindByNameAsync(dto.UserName);
        return Ok(new UserDto
        {
            Id = user!.Id,
            UserName = user.UserName!,
            Roles = (await userManager.GetRolesAsync(user)).ToArray()
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var user = await userManager.FindByNameAsync(User.Identity!.Name!);
        return Ok(new UserDto
        {
            Id = user!.Id,
            UserName = user.UserName!,
            Roles = (await userManager.GetRolesAsync(user)).ToArray()
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }
}