using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using Recipe_Generator.DTO;
using AutoMapper;
using System.IO;
using System.Security.Claims; 
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
       
        [HttpGet("All recipes")]
        public async Task<IActionResult> GetPagedRecipes(int pageNumber, int pageSize)
        {
            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<Recipe> recipesList = await _context.Recipes
                .Skip(itemsToSkip)
                .Take(pageSize)
                .Include(c => c.Category)
                .Include(u => u.User)
                .ToListAsync();

            int totalRecipesCount = await _context.Recipes.CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalRecipesCount / pageSize);

            var pagedResult = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalRecipesCount,
                Recipes = recipesList
            };

            return Ok(pagedResult);
        }

        [HttpGet("Queryed recipes")]
        public async Task<IActionResult> SearchforRecipes(string? searchTerm)
        {
            try
            {
                IQueryable<Recipe> query = _context.Recipes.Include(c => c.Category).Include(u => u.User);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(r => r.Name.ToLower().Contains(searchTerm));
                }

                List<Recipe> recipesList = await query.ToListAsync();
                return Ok(recipesList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        
        [HttpGet("GetRecipeByID/{id:int}", Name = "GetOneRecipe")]

        public async Task<IActionResult> GetRecipe(int id)
        {
            RecipeWithCategoryNameDTO RecipeDTO = new RecipeWithCategoryNameDTO();
            if (id != null || id != 0)
            {
                Recipe? recipe = await _context.Recipes
                    .Include(c => c.Category)
                    .Where(r => r.Id == id)
                    .FirstOrDefaultAsync();
                return Ok(recipe);
            }
            else
            {
                return StatusCode(404);
            }
        }
        [HttpGet("User recipes")]
        public async Task<IActionResult> GetMyRecipes()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BadRequest("Unable to get user info");
            }
            var userId = userIdClaim.Value;
            List<Recipe> userRecipes = await _context.Recipes.Include(c => c.Category).Where(r => r.User.Id == userId).ToListAsync();
            return Ok(userRecipes);
        }
        [HttpPost("Create recipes")]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeWithCategoryNameDTO recipe)
        {
            Recipe recipe2 = new Recipe();

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return BadRequest("Unable to retrieve user info");
            }
            recipe.UserId = userIdClaim.Value;
            var recipeMapping = _mapper.Map<Recipe>(recipe);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (recipe.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(recipe.Image.FileName);

                    if (recipeMapping.Image != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipeMapping.Image.TrimStart('\\'));
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
                    recipeMapping.Image = @"images\recipes\" + fileName + extension;
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



        [HttpPut("Update Recipe/{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromForm] RecipeWithCategoryNameDTO recipe)
        {
            var recipeMapping = _mapper.Map<Recipe>(recipe);
            Recipe oldRecipe = await _context.Recipes.FindAsync(id);

            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (recipe.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                    var extension = Path.GetExtension(recipe.Image.FileName);

                    if (recipeMapping.Image != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, recipeMapping.Image.TrimStart('\\'));
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
                    recipeMapping.Image = @"images\recipes\" + fileName + extension;
                }


                if (oldRecipe != null)
                {
                    //_context.Recipes.Update(recipeMapping);
                    _context.Entry(oldRecipe).CurrentValues.SetValues(recipeMapping);
                    await _context.SaveChangesAsync();
                    return Ok(recipeMapping);
                }
                return NotFound();

            }
            return BadRequest(ModelState);
        }

        [HttpDelete("Delete recipe/{id:int}")]
        public async Task<IActionResult> DeleteRecipe(int id)

        {
            Recipe? recipeToDelete = await _context.Recipes
                .Include(c => c.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
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
