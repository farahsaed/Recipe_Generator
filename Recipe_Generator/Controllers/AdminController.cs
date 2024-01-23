using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = ("admin"))]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            UserManager<User> userManager, 
            IConfiguration configuration, 
            IWebHostEnvironment environment
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        [Route("Create User")]
        public async Task<IActionResult> Register(UserDataDTO userDTO)
        {
            User user = new User();
            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;

            string wwwRootPath = _environment.WebRootPath;
            if (userDTO.Image != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var filePath = Path.Combine(wwwRootPath, @"images\ProfilePhotos");
                var extension = Path.GetExtension(userDTO.Image.FileName);
                if (user.ImagePath != null)
                {
                    var oldImage = Path.Combine(wwwRootPath, user.ImagePath.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(filePath, fileName + extension), FileMode.Create))
                {
                    userDTO.Image.CopyTo(fileStream);
                }
                user.ImagePath = @"images\ProfilePhotos\" + fileName + extension;
            }

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

        [Authorize(Roles = ("admin"))]
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

        [HttpDelete("Delete User/{id}")]
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

        [HttpGet("All Users")]
        public IActionResult GetAllUsers()
        {
            var usersList = userManager.Users.ToList();
            if (usersList.Count > 0)
            {
                return Ok(usersList);
            }

            return NotFound("No user found");

        }

        [HttpGet("Get User/{id}")]
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
    }
}
