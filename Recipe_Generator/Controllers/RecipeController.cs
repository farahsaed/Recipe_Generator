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

        public RecipeController(RecipeContext context, IMapper mapper , IWebHostEnvironment environment)
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
        public async Task<IActionResult> CreateRecipe(RecipeWithCategoryNameDTO recipe, IFormFile? file)
        {
            var categoryMapping = _mapper.Map<Recipe>(recipe);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(file.FileName);

                    if (recipe.Image != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipe.Image.TrimStart('\\'));
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
                        file.CopyTo(fileStream);
                    }
                    recipe.Image = @"\images\recipes\" + fileName + extension;
                }

                if (recipe.Id == 0)
                {
                    // Create Product
                    _context.Recipes.Add(categoryMapping);
                    await _context.SaveChangesAsync();
                    string url = Url.Link("GetOneRecipe", new { id = recipe.Id });
                    return Created(url, recipe);
                }
               
            }
            return BadRequest(ModelState);
        }



        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateRecipe(RecipeWithCategoryNameDTO recipe , int id,[FromForm] IFormFile? file)
        {
            var recipeMapping = _mapper.Map<Recipe>(recipe);
            Recipe oldRecipe = await _context.Recipes.Include(c=> c.Category).FirstOrDefaultAsync(r => r.Id == id);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(file.FileName);

                    if (recipe.Image != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipe.Image.TrimStart('\\'));
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
                        file.CopyTo(fileStream);
                    }
                    recipe.Image = @"\images\recipes\" + fileName + extension;
                }

                if (recipe.Id != 0)
                {
                    // Create Product
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
                // delete product row from db
                _context.Recipes.Remove(recipeToDelete);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }
    }
}
