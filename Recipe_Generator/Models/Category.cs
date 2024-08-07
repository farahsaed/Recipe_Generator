﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; } 
        public string Description { get; set; }  
        public List<Recipe> Recipes { get; set; }

    }
}
