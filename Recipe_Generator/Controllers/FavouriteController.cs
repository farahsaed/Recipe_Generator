using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.Collections.Generic;
using System.Security.Claims;
//using System.Web.Http;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
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

        [HttpGet("AllFavourites")]
        public async Task<IActionResult> GetAllFavourites()
        {
            var userId = userManager.GetUserId(HttpContext.User);
            if(userId != null)
            {

                List<Favourite> favouriteList = await _context.Favourites
               .Include(r => r.Recipe)
               .Include(u => u.User)
               .Where(r => r.User.Id.ToString() == userId)
               .ToListAsync();
                if (favouriteList != null)
                {
                    return Ok(favouriteList);
                }
                return NotFound("You have not added recipes to your favourites yet");

            }
            return NotFound("Can not find user info");
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
                return NotFound("Recipe is not found");
            }
        }


        [HttpPost("CreateFavourite")]
        public async Task<IActionResult> CreateFavourite(FavoriteWithRecipeInfoDTO favoriteDTO)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            User? user = await _context.Users.FindAsync(userId);
            Favourite favourite = new Favourite();

            if(userId != null)
            {
                var existingFavorite = await _context.Favourites
                .FirstOrDefaultAsync(f => f.User.Id == userId && f.RecipeId == favoriteDTO.RecipeId);

                if (existingFavorite != null)
                {
                    return BadRequest("You already added this to favourite");
                }

                favourite.User = user;
                favourite.User.Id = userId;
                favourite.RecipeId = favoriteDTO.RecipeId;

                if (ModelState.IsValid == true)
                {
                    
                    //Favourite favourite2 = _context.Favourites.Include(u => u.User);
                    _context.Favourites.Add(favourite);
                    await _context.SaveChangesAsync();
                    string url = Url.Link("GetOneFavourite", new { id = favoriteDTO.Id });
                    return Created(url, favoriteDTO);
                }
                return BadRequest(ModelState);
            }
            return NotFound("User info is not found");
           
        }

        [HttpDelete("DeleteFavourite/{id}")]
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

