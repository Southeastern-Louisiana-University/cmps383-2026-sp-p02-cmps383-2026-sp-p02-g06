using System.ComponentModel.DataAnnotations;

namespace Selu383.SP26.Api.Features.User
{
    public class UserDto
    {
        public int Id { get; set; }
        
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string[] Roles { get; set; }
    }
}
