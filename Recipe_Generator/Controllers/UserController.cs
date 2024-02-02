using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Recipe_Generator.DTO;
using Recipe_Generator.Interface;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender emailSender;

        public UserController( 
            UserManager<User> userManager,
            IConfiguration configuration , 
            IWebHostEnvironment environment,
            IEmailSender emailSender
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _environment = environment;
            this.emailSender = emailSender;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserDataDTO userDTO)
        {
            User user = new User();
            user.UserName =  userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
                
            string wwwRootPath = _environment.WebRootPath;
            if(userDTO.Image != null)
            {
                string fileName = Guid.NewGuid().ToString();
                var filePath = Path.Combine(wwwRootPath, @"images\ProfilePhotos");
                var extension = Path.GetExtension(userDTO.Image.FileName);
                if(user.ImagePath != null)
                {
                    var oldImage = Path.Combine(wwwRootPath, user.ImagePath.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImage))
                    {
                        System.IO.File.Delete(oldImage);
                    }
                }

                using(var fileStream = new FileStream(Path.Combine(filePath, fileName + extension), FileMode.Create))
                {
                    userDTO.Image.CopyTo(fileStream);
                }
                user.ImagePath = @"images\ProfilePhotos\" + fileName + extension;
            }

            await userManager.CreateAsync(user, userDTO.Password);

            IdentityResult result ;
            if (user.Email.Contains("admin") && user.UserName.Contains("admin"))
            {
                result = await userManager.AddToRoleAsync(user, "admin");
            }
            else
            {
                result = await userManager.AddToRoleAsync(user, "user");
                await emailSender.SendEmail(user.Email,user.FirstName + " " + user.LastName);
            }
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
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
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

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

    }
}
