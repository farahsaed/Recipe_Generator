using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        public CategoryController(RecipeContext context , IMapper mapper) {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCategories() 
        {
             List<Category> categoryList = await _context.Categories.Include(r=>r.Recipes).ToListAsync();
             return Ok(categoryList);
        }
        [HttpGet("{id:int}" , Name = "GetOneCategory")]
        public async Task<IActionResult> GetCategory(int id)
        {
            CategoryWithRecipeListDTO CategoryDTO = new CategoryWithRecipeListDTO();
            if (id != null || id != 0)
            {
                Category category = await _context.Categories.Include(r => r.Recipes).Where(c => c.Id == id).FirstOrDefaultAsync();
                //if (category.Recipes != null)
                //{
                //    CategoryDTO.CategoryName = category.CategoryName;
                    
                //    foreach (var recipe in category.Recipes)
                //    {

                //        CategoryDTO.Recipes.Add(recipe.Name);
                //    }
                //}
                return Ok(category);
            }
            else { return StatusCode(404); }
        }

        private Category MapCategoryObject(CategoryWithRecipeListDTO categoryWithRecipeListDTO) 
        {
            var category = new Category();
            category.CategoryName = categoryWithRecipeListDTO.CategoryName;
            category.Recipes = new List<Recipe>();

            categoryWithRecipeListDTO.Recipes.ForEach(recipeDTO =>
            {
                var recipeObj = new Recipe();
                recipeObj.Name = recipeDTO.RecipeName;
                recipeObj.Description = recipeDTO.Description;
                recipeObj.PrepareTime = recipeDTO.PrepareTime;

                category.Recipes.Add(recipeObj);
                
            });
     

            return category;

        }
        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryWithRecipeListDTO category)
        {
            var categoryMapping =  _mapper.Map<Category>(category);
            if (ModelState.IsValid == true)
            {
                _context.Categories.Add(categoryMapping);
                await _context.SaveChangesAsync(); 
                string url = Url.Link("GetOneCategory", new {id = category.Id});
                return Created(url,category);
            }
            return BadRequest(ModelState);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(CategoryWithRecipeListDTO category , int id)
        {
            var categoryMapping = _mapper.Map<Category>(category);
            Category oldCategory = await _context.Categories.Include(r=>r.Recipes).FirstOrDefaultAsync(c=>c.Id == id);
            if (oldCategory != null)
            {
                if (ModelState.IsValid == true)
                {
               
                    _context.Entry(oldCategory).CurrentValues.SetValues(categoryMapping);
                    await _context.SaveChangesAsync();
                    return Ok(categoryMapping);
                }
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category categoryToDelete = await _context.Categories.Include(r=>r.Recipes).FirstOrDefaultAsync(c => c.Id == id);
            if (categoryToDelete != null)
            {
                _context.Categories.Remove(categoryToDelete);
                await _context.SaveChangesAsync();
                return NoContent();
               
            }
            return NotFound();
        }

    }
}
