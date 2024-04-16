using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.Models;
using Recipe_Generator.DTO;
using AutoMapper;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RecipeController : ControllerBase
    {

        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<User> userManager;
        public RecipeController(RecipeContext context, IMapper mapper, IWebHostEnvironment environment, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
            this.userManager = userManager;
        }

        [HttpGet("AllRecipes")]
        public async Task<IActionResult> GetPagedRecipes(int pageNumber, int pageSize)
        {
            int itemsToSkip = (pageNumber - 1) * pageSize;

            List<Recipe> recipesList = await _context.Recipes
                .Skip(itemsToSkip)
                .Take(pageSize)
                .Include(c => c.Category)
                .Include(u => u.User)
                .Where(r => r.State == RecipeState.Approved || r.State == null)
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
            if(pagedResult != null)
            {
                //foreach (var item in pagedResult.Recipes)
                //{
                //    //string? url = Url.Link("AllRecipes", "");
                //    item.Image = "http://localhost:5115/" + item.Image;
                //}
                return Ok(pagedResult);

            }
            return NotFound("No recipes has been found"); 
            
        }

        [HttpGet("QueryedRecipes")]
        public async Task<IActionResult> SearchforRecipes(string? searchTerm)
        {
            try
            {
                IQueryable<Recipe> query = _context.Recipes.Include(u => u.User).Include(c => c.Category);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(r => r.Name.ToLower().Contains(searchTerm) || r.User.UserName.ToLower().Contains(searchTerm));
                }

                List<Recipe> recipesList = await query.ToListAsync();
                if (recipesList != null)
                {
                    return Ok(recipesList);
                }
                return NotFound("No recipes has been found");
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
            if (id != 0)
            {
                Recipe? recipe = await _context.Recipes
                    .Include(c => c.Category)
                    .Include(u => u.User)
                    .Where(r => r.Id == id)
                    .FirstOrDefaultAsync();
                return Ok(recipe);
            }
            else
            {
                return StatusCode(404);
            }
        }
        [HttpGet("UserRecipes")]
        public async Task<IActionResult> GetMyRecipes()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            List<Recipe> userRecipes = await _context.Recipes.Include(c => c.Category).Where(r => r.User.Id == userId).ToListAsync();
            if(userRecipes != null)
            {
                return Ok(userRecipes);
            }
            return NotFound("You have not created recipes yet");
        }

        [HttpPost("CreateRecipe")]
        public async Task<IActionResult> CreateRecipe([FromForm] RecipeWithCategoryNameDTO recipeDTO)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            User? user = await _context.Users.FindAsync(userId);
            var role = await userManager.GetRolesAsync(user);
            Recipe recipe = new Recipe();

            if (userId != null)
            {
                recipe.User = user;
                recipe.User.Id = userId;
                recipe.CategoryId = recipeDTO.CategoryId;
                recipe.CookTime = recipeDTO.CookTime;
                recipe.Directions = recipeDTO.Directions;
                recipe.Ingredients = recipeDTO.Ingredients;
                recipe.Name = recipeDTO.Name;
                recipe.Nutrition = recipeDTO.Nutrition;
                recipe.PrepareTime = recipeDTO.PrepareTime;
                recipe.Timing = recipeDTO.Timing;
                recipe.TotalTime = recipeDTO.TotalTime;
                if (role.Contains("User")) 
                {
                    recipe.State = RecipeState.Pending;
                }
                else
                {
                    recipe.State = RecipeState.Approved;
                }

                if (ModelState.IsValid)
                {
                    string wwwRootPath = _environment.WebRootPath;

                    if (recipeDTO.Image != null)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var uploads = Path.Combine(wwwRootPath, @"images\recipes");
                        var extension = Path.GetExtension(recipeDTO.Image.FileName);

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
                            recipeDTO.Image.CopyTo(fileStream);
                        }
                        recipe.Image = @"images\recipes\" + fileName + extension;
                    }

                }
                if (recipeDTO != null)
                {
                    
                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync();
                    string? url = Url.Link("GetOneRecipe", new { id = recipe.Id });
                    return Created(url, recipe);
                }
                return BadRequest(ModelState);
            }
            else
            {
                return NotFound("user id is not found");
            }

        }

        [HttpPut("UpdateRecipe/{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromForm] RecipeWithCategoryNameDTO recipe)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            var recipeMapping = _mapper.Map<Recipe>(recipe);

            Recipe? oldRecipe = await _context.Recipes.Include(c => c.Category).Where(u => u.User.Id == userId).FirstOrDefaultAsync(r => r.Id == id);

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
                    if (recipeMapping.Id == 0)
                    {
                        recipeMapping.Id = id;
                    }
                    //_context.Recipes.Update(recipeMapping);
                    _context.Entry(oldRecipe).CurrentValues.SetValues(recipeMapping);
                    await _context.SaveChangesAsync();
                    return Ok(recipeMapping);
                }
                return NotFound();

            }
            return BadRequest(ModelState);
        }
        [HttpPost("Rate/{id}")]
        public IActionResult RateRecipe(int id, int ratingValue)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            var recipe = _context.Recipes.Include(r=>r.Ratings).FirstOrDefault(r => id == r.Id);
            
            User? user = _context.Users.Find(userId); 
            if (recipe == null)
            {               
                return NotFound();
            }
            if(userId != null)
            {
                var rating = new Rating
                {
                    RecipeId = id,
                    RatingValue = ratingValue,
                    User = user
                };
                _context.Ratings.Add(rating);
                _context.SaveChanges();

                recipe.Ratings.Add(rating);
                recipe.AverageRating = recipe.CalcAvgRating();
                _context.SaveChanges();
                return Ok(rating);
            }
            return BadRequest("Unauthorized");
        }
        [HttpDelete("DeleteRecipe/{id:int}")]
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
        //[NonAction]
        //public IActionResult GetImagePath()
        //{
        //    string wwwRootPath = _environment.WebRootPath; ;
        //    return wwwRootPath + @"images\recipes\"
        //}
    }
}
