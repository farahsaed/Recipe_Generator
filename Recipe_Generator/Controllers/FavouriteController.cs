using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.Collections.Generic;
using System.Security.Claims;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly RecipeContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<User> userManager;
        public FavouriteController(RecipeContext context, IMapper mapper, IWebHostEnvironment environment, UserManager<User> userManager)
        {
            _context = context;
            _mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet("All Favourites")]
        public async Task<IActionResult> GetAllFavourites()
        {
            var userId = userManager.GetUserId(HttpContext.User);

            List<Favourite> favouriteList = await _context.Favourites
                .Include(r => r.Recipe)
                .Include(u => u.User)
                .Where(r => r.User.Id.ToString() == userId)
                .ToListAsync();
            return Ok(favouriteList);
        }


        [HttpGet("GetFavouriteByID/{id:int}", Name = "GetOneFavourite")]
        public async Task<IActionResult> GetFavourite(int id)
        {
            Favourite? favourite = await _context.Favourites
                .Include(r => r.Recipe)
                .Include(u => u.User)
                .Where(f => f.Id == id)
                .FirstOrDefaultAsync();
            if (favourite != null)
            {
                return Ok(favourite);
            }
            else
            {
                return StatusCode(404);
            }
        }


        [HttpPost("Create favourite")]
        public async Task<IActionResult> CreateFavourite(FavoriteWithRecipeInfoDTO favoriteDTO)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            User user = await _context.Users.FindAsync(userId);
            Favourite favourite = new Favourite();

            favourite.User = user;
            if (ModelState.IsValid == true)
            {
                _context.Favourites.Add(favourite);
                await _context.SaveChangesAsync();
                string url = Url.Link("GetOneFavourite", new { id = favoriteDTO.Id });
                return Created(url, favoriteDTO);
            }
            return BadRequest(ModelState);
        }




        [HttpDelete("Delete favourite/{id}")]
        public async Task<IActionResult> DeleteFavourite(int id)
        {
            Favourite? favouriteToDelete = await _context.Favourites
                .Include(r => r.Recipe)
                .FirstOrDefaultAsync(c => c.Id == id);
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

