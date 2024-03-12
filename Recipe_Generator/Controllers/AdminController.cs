using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;
namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly RecipeContext db;

        public AdminController(
            UserManager<User> userManager, 
            IConfiguration configuration, 
            IWebHostEnvironment environment,
            RecipeContext db
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _environment = environment;
            this.db = db;
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> Register(UserDataDTO userDTO)
        {
            User user = new User();
            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;

            string wwwRootPath = _environment.WebRootPath;
            //if (userDTO.Image != null)
            //{
            //    string fileName = Guid.NewGuid().ToString();
            //    var filePath = Path.Combine(wwwRootPath, @"images\ProfilePhotos");
            //    var extension = Path.GetExtension(userDTO.Image.FileName);
            //    if (user.ImagePath != null)
            //    {
            //        var oldImage = Path.Combine(wwwRootPath, user.ImagePath.TrimStart('\\'));
            //        if (System.IO.File.Exists(oldImage))
            //        {
            //            System.IO.File.Delete(oldImage);
            //        }
            //    }

            //    using (var fileStream = new FileStream(Path.Combine(filePath, fileName + extension), FileMode.Create))
            //    {
            //        userDTO.Image.CopyTo(fileStream);
            //    }
            //    user.ImagePath = @"images\ProfilePhotos\" + fileName + extension;
            //}

            await userManager.CreateAsync(user, userDTO.Password);

            IdentityResult result = await userManager.AddToRoleAsync(user, "user");
            if (result.Succeeded)
            {
                return Ok("Account created successfully");
            }
            else
            {
                var message = string.Join(", ", result.Errors.Select(x => "Code " + x.Code + " Description" + x.Description));
                return BadRequest(message);
            }
        }

        [HttpPost("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(UserDataDTO userData,string id)
        {
            User user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.UserName = userData.UserName;
                user.Email = userData.Email;
                user.FirstName = userData.FirstName;
                user.LastName = userData.LastName;
                IdentityResult result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok("User Updated Successfully");
                }
                else
                {
                    var message = string.Join(", ", result.Errors.Select(x => "Code " + x.Code + " Description" + x.Description));
                    return BadRequest(message);
                }

            }

            return NotFound("User not found");
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            User user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Ok("User Deleted successfully");
                }
            }
            return NotFound("User not found");
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsers(int page, int limit)
        {
            var users = userManager.Users;

            var totalCount = await userManager.Users.CountAsync(); 

            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);

            var pagedUsers = await users.Skip((page - 1) * limit).Take(limit).ToListAsync();

            var pagedUser = new 
            {
                Users = pagedUsers,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            if (pagedUser.TotalCount>0)
            {
                return Ok(pagedUser);
            }

            return NotFound("No user found");

        }

        [HttpGet("SearchUser")]
        public IActionResult GetSearchedUser(string? searchTerm)
        {
            IQueryable<User> users;
            if (string.IsNullOrWhiteSpace(searchTerm))
                users = userManager.Users;

            else
            {
                searchTerm = searchTerm.Trim().ToLower();
                users = userManager.Users
                    .Where(u => u.UserName.ToLower().Contains(searchTerm)
                        || u.Email.ToLower().Contains(searchTerm)
                    );
            }

            if (users.Any())
            {
                return Ok(users.ToList());
            }
            return NotFound("No user matches " + searchTerm);
        }

        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            User user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("User not found");
            }
        }

        [HttpGet("PendingRecipes")]
        public async Task<IActionResult> GEtPendingRecipes()
        {
            var recipes = db.Recipes
                          .Where(r=> r.State == RecipeState.Pending)
                          .Include(r=>r.User)
                          .ToList();

            return Ok(recipes);
        }

        [HttpPost("ApproveRecipe/{id}")]
        public IActionResult ApproveRecipe(int id)
        {
            var recipe = db.Recipes.Find(id);
            if(recipe != null)
            {
                recipe.State = RecipeState.Approved;
                db.Update(recipe);
                db.SaveChanges();
                return Ok();
            }
            return BadRequest("Recipe not found");
        }

        [HttpPost("DeleteRecipe/{id}")]
        public IActionResult DeleteRecipe(int id)
        {
            var recipe = db.Recipes.Find(id);
            if (recipe != null)
            {
                recipe.State = RecipeState.Deleted;
                db.Remove(recipe);
                db.SaveChanges();
                return Ok();
            }
            return BadRequest("Recipe not found");
        }

    }
}
