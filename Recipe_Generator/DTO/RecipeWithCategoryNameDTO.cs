namespace Recipe_Generator.DTO
{
    public class RecipeWithCategoryNameDTO
    {
        public int Id { get; set; }

        public string RecipeName { get; set; }
        public string? ImageUrl {get; set;}
        public IFormFile? Image { get; set; }

        public string  Description { get; set;}
        public string PrepareTime { get; set;}
        public string CategoryName { get; set;}
        public string CookTime { get; set; }
        public string TotalTime { get; set; }
        public string Ingredients { get; set; }
        public string Directions { get; set; }
        public int CategoryId { get; set; }
        public string Nutrition { get; set; }
        public string Timing { get; set; }

    }
}
