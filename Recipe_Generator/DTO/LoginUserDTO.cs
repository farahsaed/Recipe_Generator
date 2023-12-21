﻿using System.ComponentModel.DataAnnotations;

namespace Recipe_Generator.DTO
{
    public class LoginUserDTO
    {
        [Required]
        [Display(Name = "User Nsame")]
        public String  UserName { get; set; }
        [Required]
        public String Password { get; set; }
    }
}