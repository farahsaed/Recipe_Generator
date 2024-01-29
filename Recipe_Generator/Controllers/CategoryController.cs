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
        private readonly IWebHostEnvironment _environment;
        public CategoryController(RecipeContext context, IMapper mapper, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }


        [HttpGet("All categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            List<Category> categoryList = await _context.Categories
                .Include(r => r.Recipes)
                .ToListAsync();
            return Ok(categoryList);
        }


        [HttpGet("Filtered Categories")]
        public async Task<IActionResult> FilterByCategory(string query)
        {
            query = query.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Please provide a valid category.");
            }

            var filteredCategory = await _context.Categories
                .Include(c => c.Recipes)
                .Where(c => c.CategoryName.ToLower() == query)
                .ToListAsync();

            return Ok(filteredCategory);
        }


        [HttpGet("GetCategoryByID/{id:int}", Name = "GetOneCategory")]
        public async Task<IActionResult> GetCategory(int id)
        {
            CategoryWithRecipeListDTO CategoryDTO = new CategoryWithRecipeListDTO();
            if (id != 0)
            {
                Category? category = await _context.Categories
                    .Include(r => r.Recipes)
                    .Where(c => c.Id == id)
                    .FirstOrDefaultAsync();

                return Ok(category);
            }
            else { return NotFound(); }
        }

        [HttpPost("Create category")]
        public async Task<IActionResult> CreateCategory([FromForm] CategoryWithRecipeListDTO category)
        {
            var categoryMapping = _mapper.Map<Category>(category);
            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (category.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\category");
                    var extension = Path.GetExtension(category.Image.FileName);

                    if (categoryMapping.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, categoryMapping.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (
                        var fileStream = new FileStream(
                            Path.Combine(uploads, fileName + extension),
                            FileMode.Create)
                        )
                    {
                        category.Image.CopyTo(fileStream);
                    }
                    categoryMapping.ImageUrl = @"images\category\" + fileName + extension;
                }
                _context.Categories.Add(categoryMapping);
                await _context.SaveChangesAsync();
                string? url = Url.Link("GetOneCategory", new { id = categoryMapping.Id });
                return Created(url, category);

            }
            return BadRequest(ModelState);
        }


        [HttpPut("Update category/{id}")]
        public async Task<IActionResult> UpdateCategory([FromForm] CategoryWithRecipeListDTO category, int id)
        {
            var categoryMapping = _mapper.Map<Category>(category);
            Category? oldCategory = await _context.Categories
                .Include(r => r.Recipes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (category.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\category");
                    var extension = Path.GetExtension(category.Image.FileName);

                    if (categoryMapping.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, categoryMapping.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (
                        var fileStream = new FileStream(
                            Path.Combine(uploads, fileName + extension),
                            FileMode.Create)
                        )
                    {
                        category.Image.CopyTo(fileStream);
                    }
                    categoryMapping.ImageUrl = @"images\category\" + fileName + extension;
                }
                if (oldCategory != null)
                {
                    _context.Entry(oldCategory).CurrentValues.SetValues(categoryMapping);
                    await _context.SaveChangesAsync();
                    return Ok(categoryMapping);
                }
                return NotFound("Not Found");
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("Delete category/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category? categoryToDelete = await _context.Categories
                .Include(r => r.Recipes)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (categoryToDelete != null)
            {
                _context.Categories.Remove(categoryToDelete);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }
        //private Category MapCategoryObject(CategoryWithRecipeListDTO categoryWithRecipeListDTO) 
        //{
        //    var category = new Category();
        //    category.CategoryName = categoryWithRecipeListDTO.CategoryName;
        //    category.Recipes = new List<Recipe>();

        //    categoryWithRecipeListDTO.Recipes.ForEach(recipeDTO =>
        //    {
        //        var recipeObj = new Recipe();
        //        recipeObj.Name = recipeDTO.RecipeName;
        //        recipeObj.Description = recipeDTO.Description;
        //        recipeObj.PrepareTime = recipeDTO.PrepareTime;

        //        category.Recipes.Add(recipeObj);

        //    });


        //    return category;

        //}

    }
}
