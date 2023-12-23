using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using Recipe_Generator.DTO;
using AutoMapper;
using System.IO;
namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipeController : ControllerBase
    {

        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;

        public RecipeController(RecipeContext context, IMapper mapper, IWebHostEnvironment environment)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllRecipes()
        {
            List<Recipe> recipesList = await _context.Recipes.Include(c => c.Category).ToListAsync();
            return Ok(recipesList);
        }

        [HttpGet("{id:int}", Name = "GetOneRecipe")]

        public async Task<IActionResult> GetRecipe(int id)
        {
            RecipeWithCategoryNameDTO RecipeDTO = new RecipeWithCategoryNameDTO();
            if (id != null || id != 0)
            {
                Recipe? recipe = await _context.Recipes.Include(c => c.Category).Where(r => r.Id == id).FirstOrDefaultAsync();
                return Ok(recipe);
            }
            else
            {
                return StatusCode(404);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeWithCategoryNameDTO recipe)
        {
            var recipeMapping = _mapper.Map<Recipe>(recipe);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (recipe.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(recipe.Image.FileName);

                    if (recipe.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipe.ImageUrl.TrimStart('\\'));
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
                        recipe.Image.CopyTo(fileStream);
                    }
                    recipe.ImageUrl = @"images\recipes\" + fileName + extension;
                }

                if (recipe != null)
                {
                    // Create recipe
                    _context.Recipes.Add(recipeMapping);
                    await _context.SaveChangesAsync();
                    string url = Url.Link("GetOneRecipe", new { id = recipe.Id });
                    return Created(url, recipe);
                }

            }
            return BadRequest(ModelState);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRecipe([FromForm] RecipeWithCategoryNameDTO recipe, int id)
        {
            var recipeMapping = _mapper.Map<Recipe>(recipe);
            Recipe oldRecipe = await _context.Recipes.Include(c => c.Category).FirstOrDefaultAsync(r => r.Id == id);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;
                if (recipe.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(recipe.Image.FileName);

                    if (recipe.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipe.ImageUrl.TrimStart('\\'));
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
                        recipe.Image.CopyTo(fileStream);
                    }
                    recipe.ImageUrl = @"images\recipes\" + fileName + extension;
                }

                if (recipe.Id != 0)
                {
                    _context.Entry(oldRecipe).CurrentValues.SetValues(recipeMapping);
                    await _context.SaveChangesAsync();
                    return Ok(recipeMapping);
                }
                return NotFound();

            }
            return BadRequest(ModelState);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteRecipe(int id)

        {
            Recipe recipeToDelete = await _context.Recipes.Include(c => c.Category).FirstOrDefaultAsync(r => r.Id == id);
            if (recipeToDelete != null)
            {
                var oldImagePath = Path.Combine(_environment.WebRootPath, recipeToDelete.Image.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                _context.Recipes.Remove(recipeToDelete);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }
    }
}
