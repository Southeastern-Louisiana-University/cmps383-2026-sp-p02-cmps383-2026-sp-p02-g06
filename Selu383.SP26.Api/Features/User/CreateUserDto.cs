using System.ComponentModel.DataAnnotations;

namespace Selu383.SP26.Api.Features.User
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        
        public string Password { get; set; } = string.Empty;

        [Required]
        public string[] Roles { get; set; }
    }
}
