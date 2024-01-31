using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? Rating { get; set; } 
        public string? PrepareTime { get; set; }
        public string? Image { get; set; }
        public string? CookTime { get; set; }
        public string? TotalTime { get; set; }
        public string? Ingredients { get; set; }
        public string? Directions { get; set; }
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }
        public string? Nutrition { get; set; }
        public string? Timing { get; set; }

        public User User { get; set; }

    }
}
