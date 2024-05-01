using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace Recipe_Generator.DTO
{
    public class LoginUserDTO
    {
        [Required]
        [Display(Name = "User Name")]
        public String  UserName { get; set; }
        [Required]
        public String Password { get; set; }
    }
}
