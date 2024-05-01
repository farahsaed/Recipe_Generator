using System.ComponentModel.DataAnnotations.Schema;

namespace Recipe_Generator.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double? AverageRating { get; set; } 
        public ICollection<Rating> Ratings { get; set; }
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
<<<<<<< HEAD
        
=======
        public RecipeState State { get; set; }
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
        public User User { get; set; }
        public double CalcAvgRating()
        {
            if(Ratings == null || Ratings.Count == 0)
            {
                return 0;
            }
            return Ratings.Average(r => r.RatingValue);
        } 

    }
}
