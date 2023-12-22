using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Recipe_Generator.Controllers
{
    //[Authorize(Roles = ("User"))]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment _environment;

        public UserController(UserManager<User> userManager,IConfiguration configuration , IWebHostEnvironment environment)
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        [Route("Register")]
        [Route("Create User")]
        public async Task<IActionResult> Register([FromForm]UserDataDTO userDTO)
        {
            User user = new User();
            user.UserName =  userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            userManager.AddToRoleAsync(user, "user");
                
            string wwwRootPath = _environment.WebRootPath;
            if(userDTO.Image != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var filePath = Path.Combine(wwwRootPath, @"images\ProfilePhotos");
                var extension = Path.GetExtension(userDTO.Image.FileName);
                if(userDTO.ImagePath != null)
                {
                    var oldImage = Path.Combine(wwwRootPath, userDTO.ImagePath.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                using(var fileStream = new FileStream(Path.Combine(filePath, fileName + extension), FileMode.Create))
                {
                    userDTO.Image.CopyTo(fileStream);
                }
                userDTO.ImagePath = @"images\ProfilePhotos\" + fileName + extension;
            }

            user.ImagePath = userDTO.ImagePath;

            IdentityResult result = await userManager.CreateAsync(user, userDTO.Password);

            if (result.Succeeded)
            {
                return Ok("Account created successfully");
            }
            else
            {
                var message = string.Join(", ", result.Errors.Select(x => "Code " + x.Code + " Description" + x.Description));
                return BadRequest(message);
            }
            
            //return BadRequest();
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUserDTO userDTO)
        {
            if (ModelState.IsValid)
            {
                User user = await userManager.FindByNameAsync(userDTO.UserName);
                if (user != null) 
                {
                    bool foundPassword = await userManager.CheckPasswordAsync(user,userDTO.Password);
                    if (foundPassword)
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        
                        var roles =await userManager.GetRolesAsync(user);
                        foreach(var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));

                        }

                        var securityKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])
                            ) ;
                        SigningCredentials signingCredentials = new SigningCredentials(
                                securityKey,SecurityAlgorithms.HmacSha256
                            );

                        JwtSecurityToken validToken = new JwtSecurityToken(
                            issuer: configuration["JWT:IssuerValid"],
                            audience: configuration["JWT:AudianceValid"],
                            claims: claims,
                            expires: DateTime.Now.AddHours(2),
                            signingCredentials:signingCredentials 
                            ) ;

                        return Ok(new
                            {
                                message= "Logged in successfully",
                                token = new JwtSecurityTokenHandler().WriteToken(validToken),
                                expires = validToken.ValidTo
                            }
                        );
                    }
                }
            }
            return Unauthorized("Invalid user name or password");
        }

        [HttpPost("Update User/{id}")]
        [Authorize(Roles = ("Admin"))]

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
            
            return BadRequest("User not found");
        }

        [HttpDelete("Delete User/{id}")]
        [Authorize(Roles = ("Admin"))]

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
                return BadRequest("User not found");
        }

        [HttpGet("All Users")]
        [Authorize(Roles = ("Admin"))]

        public IActionResult GetAllUsers() 
        {
            var usersList = userManager.Users.ToList();
            if (usersList.Count > 0)
            {
                return Ok(usersList);
            }

            return BadRequest("No user found");
            
        }

        [HttpGet("{id}")]
        [Authorize(Roles = ("Admin"))]

        public async Task<IActionResult> GetUser(string id)
        {
            User user = await userManager.FindByIdAsync(id);
            if(user != null)
            {
                return Ok(user);
            }
            else
            {
                return BadRequest("User not found");
            }
        }

        

    }
}
