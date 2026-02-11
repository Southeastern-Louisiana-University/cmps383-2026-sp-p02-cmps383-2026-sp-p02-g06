using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.User;


namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(
    DataContext dataContext
    ) : ControllerBase
{
    [HttpGet]

    [HttpPost]
    public ActionResult<UserDto> Create(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest();
        }
        var user = new User
        {
            UserName = dto.UserName,
            Password = dto.Password,
        };
        dataContext.Users.Add(user);
        dataContext.SaveChanges();
        return Ok(new CreateUserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Password = user.Password,
        });
    }
}
