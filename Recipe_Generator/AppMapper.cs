using AutoMapper;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;

namespace Recipe_Generator
{
    public class AppMapper :Profile 
    {
        public AppMapper() {
            CreateMap<CategoryWithRecipeListDTO, Category>();
            CreateMap<RecipeWithCategoryNameDTO, Recipe>();
            CreateMap<FavoriteWithRecipeInfoDTO, Favourite>();

        }
    }
}
