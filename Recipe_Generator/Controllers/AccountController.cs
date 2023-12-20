using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;

        public AccountController(UserManager<User> userManager,IConfiguration configuration)
        {
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDTO userDTO)
        {
            if(ModelState.IsValid)
            {
                User user = new User();
                user.UserName =  userDTO.UserName;
                user.Email = userDTO.Email;
                IdentityResult result = await userManager.CreateAsync(user, userDTO.Password);

                if (result.Succeeded)
                {
                    return Ok("Account created successfully");
                }
                else
                {
                    return BadRequest(result.Errors.ToString());
                }
            }
            
            return BadRequest();
        }

        [HttpPost("login")]
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
            return Unauthorized();
        }

    }
}
