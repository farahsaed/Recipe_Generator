using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
    //[Authorize(Roles = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _environment;
<<<<<<< HEAD
        public CategoryController(RecipeContext context, IMapper mapper , IWebHostEnvironment environment)
=======
        public CategoryController(RecipeContext context, IMapper mapper, IWebHostEnvironment environment)
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
        {
            _context = context;
            _mapper = mapper;
            _environment = environment;
        }


        [HttpGet("AllCategories")]
        public async Task<IActionResult> GetAllCategories(int pageNumber, int pageSize)
        {
            int itemsToSkip = (pageNumber - 1) * pageSize;
            List<Category> categoryList = await _context.Categories

                .Skip(itemsToSkip)
                .Take(pageSize)
                .Include(r => r.Recipes)
                .ToListAsync();

            int totalCategoriesCount = await _context.Categories.CountAsync();

            int totalPages = (int)Math.Ceiling((double)totalCategoriesCount / pageSize);

            var pagedResult = new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalItems = totalCategoriesCount,
                Categories = categoryList
            };
            if (pagedResult != null)
            {
                //foreach(var item in categoryList)
                //{
                //    item.ImageUrl = "http://localhost:5115/" + item.ImageUrl;
                //}
                return Ok(pagedResult);
            }
            return NotFound("No categories has been found");
        }


        [HttpGet("FilteredCategories")]
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
            if(filteredCategory != null)
            {
                return Ok(filteredCategory);
            }
            return NotFound("No categories found");
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
<<<<<<< HEAD
            else { return NotFound(); }
=======
            else { return NotFound("Category not found"); }
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
        }
        [HttpGet]
        public async Task<IActionResult> SearchinCategories(string searchTerm)
        {
            List<Category> categoriesList = await _context.Categories.Include(r => r.Recipes).ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                categoriesList = categoriesList.Where(c => c.CategoryName.ToLower().Contains(searchTerm)).ToList();
                //List<Category> categoriesesList = await _context.Categories.Include(r => r.Recipes).ToListAsync();
            }
            else
            {
                if(categoriesList != null)
                {
                    return Ok(categoriesList);
                }
                return NotFound("No categories has been found");
            }
            return Ok(categoriesList);

        } 
        [HttpPost("CreateCategory")]
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

<<<<<<< HEAD
                    if (categoryMapping.ImagePath != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, categoryMapping.ImagePath.TrimStart('\\'));
=======
                    if (categoryMapping.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, categoryMapping.ImageUrl.TrimStart('\\'));
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
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
<<<<<<< HEAD
                    categoryMapping.ImagePath = @"images\category\" + fileName + extension;
=======
                    categoryMapping.ImageUrl = @"images\category\" + fileName + extension;
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
                }
                _context.Categories.Add(categoryMapping);
                await _context.SaveChangesAsync();
                string? url = Url.Link("GetOneCategory", new { id = categoryMapping.Id });
                return Created(url, category);
<<<<<<< HEAD
                
=======

>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
            }
            return BadRequest(ModelState);
        }


<<<<<<< HEAD
        [HttpPut("Update category/{id}")]
        public async Task<IActionResult> UpdateCategory(CategoryWithRecipeListDTO category, int id)
=======
        [HttpPut("UpdateCategory/{id}")]
        public async Task<IActionResult> UpdateCategory([FromForm] CategoryWithRecipeListDTO category, int id)
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
        {
            var categoryMapping = _mapper.Map<Category>(category);
            Category? oldCategory = await _context.Categories
                .Include(r => r.Recipes)
                .FirstOrDefaultAsync(c => c.Id == id);
<<<<<<< HEAD
            if (ModelState.IsValid)
            {
                string wwwRootPath = _environment.WebRootPath;

                if (category.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\category");
                    var extension = Path.GetExtension(category.Image.FileName);

                    if (categoryMapping.ImagePath != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, categoryMapping.ImagePath.TrimStart('\\'));
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
                    categoryMapping.ImagePath = @"images\category\" + fileName + extension;
                }
                _context.Categories.Add(categoryMapping);
                await _context.SaveChangesAsync();
                string url = Url.Link("GetOneCategory", new { id = category.Id });
                return Created(url, category);

            }
            if (oldCategory != null)
=======
            
            if (ModelState.IsValid)
>>>>>>> acc659a2ce75d75f3b4232fbe99493481d1554c3
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
                    if(categoryMapping.Id == 0)
                    {
                        categoryMapping.Id = id;
                    }
                    _context.Entry(oldCategory).CurrentValues.SetValues(categoryMapping);
                    await _context.SaveChangesAsync();
                    return Ok(categoryMapping);
                }
                return NotFound("Not Found");
            }
            return BadRequest(ModelState);
        }

        [HttpDelete("DeleteCategory/{id}")]
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
