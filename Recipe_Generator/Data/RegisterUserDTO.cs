using System.ComponentModel.DataAnnotations;

namespace Recipe_Generator.Data
{
    public class RegisterUserDTO
    {
        [Required]
        [Display(Name = "User Name")]
        public String UserName { get; set; }
        [Required]
        public String Password { get; set; }
        [Required]
        [Display(Name = "Confirm Password")]
        [Compare("Password")]
        public String ConfirmPassword { get; set; }
        [Required]
        public String Email { get; set; }
    }
}
