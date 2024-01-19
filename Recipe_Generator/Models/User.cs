using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class User : IdentityUser
    {
        [Required]
        public String FirstName { get; set; }
        [Required]
        public String LastName { get; set; }
        public List<Favourite> Favourites { get; set; }
        public List<Recipe> Recipes { get; set; }
        public string? ImagePath { get; set; }
    }
}
