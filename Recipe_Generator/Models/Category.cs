using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; }
<<<<<<< HEAD
        public string ImagePath { get; set; } 
=======
        public string ImageUrl { get; set; }
        public string Description { get; set; }  
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
        public List<Recipe> Recipes { get; set; }

    }
}
