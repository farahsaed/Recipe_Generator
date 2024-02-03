using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [ForeignKey("Recipe")]
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
        public User? User { get; set; }
        public double RatingValue { get; set; } 
    }
}
