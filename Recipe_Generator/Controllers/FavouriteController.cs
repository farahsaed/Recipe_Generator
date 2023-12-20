using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.Collections.Generic;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        public FavouriteController(RecipeContext context, IMapper mapper) {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFavourites()
        {
            List<Favourite> favouriteList = await _context.Favourites.Include(r => r.Recipe).Include(u => u.User).ToListAsync();
            return Ok(favouriteList);
        }
        [HttpGet("{id:int}" , Name = "GetOneFavourite")]
        public async Task<IActionResult> GetFavourite(int id)
        {
            Favourite favourite = await _context.Favourites.Include(r => r.Recipe).Include(u => u.User).Where(f => f.Id == id).FirstOrDefaultAsync();
            if (favourite != null)
              {
                return Ok(favourite);

                //    CategoryDTO.CategoryName = category.CategoryName;

                //    foreach (var recipe in category.Recipes)
                //    {

                //        CategoryDTO.Recipes.Add(recipe.Name);
                //    }
            }
            else 
            { 
                return StatusCode(404); 
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateFavourite(FavoriteWithRecipeInfoDTO favoriteDTO)
        {
            var categoryMapping = _mapper.Map<Category>(favoriteDTO);
            if (ModelState.IsValid == true)
            {
                _context.Categories.Add(categoryMapping);
                await _context.SaveChangesAsync();
                string url = Url.Link("GetOneFavourite", new { id = favoriteDTO.Id });
                return Created(url, favoriteDTO);
            }
            return BadRequest(ModelState);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFavourite(FavoriteWithRecipeInfoDTO favouriteDTO , int id)
        {
            var favouriteMapping = _mapper.Map<Favourite>(favouriteDTO);
            Favourite oldFavourite = await _context.Favourites.Include(r => r.Recipe).FirstOrDefaultAsync(f => f.Id == id);
            if (oldFavourite != null)
            {
                if (ModelState.IsValid == true)
                {

                    _context.Entry(oldFavourite).CurrentValues.SetValues(favouriteMapping);
                    await _context.SaveChangesAsync();
                    return Ok(favouriteMapping);
                }
            }
            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavourite(int id)
        {
            Favourite favouriteToDelete = await _context.Favourites.Include(r => r.Recipe).FirstOrDefaultAsync(c => c.Id == id);
            if (favouriteToDelete != null)
            {
                _context.Favourites.Remove(favouriteToDelete);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            return NotFound();
        }


    }
}
       
