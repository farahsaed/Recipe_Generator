using Microsoft.AspNetCore.Http;

namespace Recipe_Generator.DTO
{
    public class RecipeWithCategoryNameDTO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public string  Description { get; set;}
        public RecipeState State { get; set; }
        public double Rating {  get; set; }
        public int TotalRating { get; set; }
        public string PrepareTime { get; set;}
        public string CategoryName { get; set;}
        public string CookTime { get; set; }
        public string TotalTime { get; set; }
        public string Ingredients { get; set; }
        public string Directions { get; set; }
        public int CategoryId { get; set; }
        public string Nutrition { get; set; }
        public string Timing { get; set; }
       //public string? UserId { get; set; }

    }
}
