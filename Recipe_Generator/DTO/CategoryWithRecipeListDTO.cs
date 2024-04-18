using Recipe_Generator.Models;
namespace Recipe_Generator.DTO
{
    public class CategoryWithRecipeListDTO
    {
        public int? Id { get; set; }

        public string CategoryName { get; set; }
<<<<<<< HEAD
        public IFormFile Image { get; set; }

       // public List<RecipeWithCategoryNameDTO> Recipes { get; set; } 
=======
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        // public List<RecipeWithCategoryNameDTO> Recipes { get; set; } 
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
    }
}
