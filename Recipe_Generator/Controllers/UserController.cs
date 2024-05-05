using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Cors;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Recipe_Generator.Data;
using Recipe_Generator.DTO;
using Recipe_Generator.Interface;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [EnableCors(origins:"*" , headers:"*" ,methods:"*")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IEmailSender emailSender;
        private readonly SignInManager<User> signInManager;
        private readonly ILogger<UserController> _logger;
        private readonly RecipeContext db;
        private readonly JwtHandler jwtHandler;

        public UserController(
            UserManager<User> userManager,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IEmailSender emailSender,
            SignInManager<User> signInManager,
            ILogger<UserController> logger,
            JwtHandler jwtHandler,
            RecipeContext db
            )
        {
            this.userManager = userManager;
            this.configuration = configuration;
            _environment = environment;
            this.emailSender = emailSender;
            this.signInManager = signInManager;
            this._logger = logger;
            this.jwtHandler = jwtHandler;
            this.db = db;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserDataDTO userDTO)
        {
            User user = new User();
            user.UserName = userDTO.UserName;
            user.Email = userDTO.Email;
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.ImagePath = "\\image\\ProfilePhotos\\00000000-0000-0000-0000-000000000000.png";

            await userManager.CreateAsync(user, userDTO.Password);

            IdentityResult result;
            var roles = await userManager.GetUsersInRoleAsync("admin");

            if (user.Email.ToLower().Contains("admin") && user.UserName.ToLower().Contains("admin"))
            {
                if(roles.Count > 1) 
                {
                    return Unauthorized("You can't be an admin");
                }
                else
                {
                    result = await userManager.AddToRoleAsync(user, "admin");
                }
            }
            else
            {
                result = await userManager.AddToRoleAsync(user, "user");
                bool IsValidEmail(string email)
                {
                    try
                    {
                        var mailAddress = new System.Net.Mail.MailAddress(user.Email);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                await emailSender.SendEmailGreeting(user.Email, user.FirstName);
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
                    bool foundPassword = await userManager.CheckPasswordAsync(user, userDTO.Password);
                    if (foundPassword)
                    {
                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                        var roles = await userManager.GetRolesAsync(user);
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));

                        }

                        var securityKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])
                            );
                        SigningCredentials signingCredentials = new SigningCredentials(
                                securityKey, SecurityAlgorithms.HmacSha256
                            );

                        JwtSecurityToken validToken = new JwtSecurityToken(
                            issuer: configuration["JWT:IssuerValid"],
                            audience: configuration["JWT:AudianceValid"],
                            claims: claims,
                            expires: DateTime.Now.AddHours(24),
                            signingCredentials: signingCredentials
                            );

                        return Ok(new
                        {
                            message = "Logged in successfully",
                            token = new JwtSecurityTokenHandler().WriteToken(validToken),
                            expires = validToken.ValidTo
                        }
                        );
                    }
                }
            }

            return Unauthorized("Invalid user name or password");
        }

//        [HttpPost("signin-google")]
//        public async Task<IActionResult> HandleGoogleResponse([FromBody] ExternalAuthDTO externalAuthDTO)
//        {
//            var payload = await jwtHandler.VerifyGoogleToken(externalAuthDTO);
//            if (payload == null)
//                return BadRequest("Invalid auth");

//            var info = new UserLoginInfo(externalAuthDTO.Provider, payload.Subject, externalAuthDTO.Provider);

//            var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
//            if(user == null)
//            {
//                user = await userManager.FindByEmailAsync(payload.Email);
//                if(user == null)
//                {
//                    user = new User
//                    {
//                        Email = payload.Email,
//                        UserName = payload.Email,
//                        FirstName = payload.Name,
//                        LastName = payload.FamilyName
//                    };
//                    await userManager.CreateAsync(user);

//                    await userManager.AddToRoleAsync(user, "User");
//                    await userManager.AddLoginAsync(user, info);
//                }
//                else
//                {
//                    await userManager.AddLoginAsync(user, info);
//                }
//            }
//            if (user == null)
//                return BadRequest("Invalid authentication");

//            var claims = new List<Claim>();
//            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
//            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
//            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

//            var roles = await userManager.GetRolesAsync(user);
//            foreach (var role in roles)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, role));

//            }

//            var securityKey = new SymmetricSecurityKey(
//                    Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"])
//                );
//            SigningCredentials signingCredentials = new SigningCredentials(
//                    securityKey, SecurityAlgorithms.HmacSha256
//                );

//            JwtSecurityToken validToken = new JwtSecurityToken(
//                issuer: configuration["JWT:IssuerValid"],
//                audience: configuration["JWT:AudianceValid"],
//                claims: claims,
//                expires: DateTime.Now.AddHours(24),
//                signingCredentials: signingCredentials
//                );
//            return Ok(new
//            {
//                message = "Logged in successfully using google",
//                token = new JwtSecurityTokenHandler().WriteToken(validToken),
//                expires = validToken.ValidTo
//            });
//;       }


        [HttpPost("SignInWithGoogle")]
        public async Task<IActionResult> LoginWithGoogle()
        {
            //var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(HandleGoogleResponse)) };
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", Url.Action(nameof(HandleGoogleResponse)));
            var res = new ChallengeResult("Google", properties);
            Response.Headers.Append("Access-Control-Allow-Origin", "*");
            Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");

            return res;
        }

        [HttpGet("signin-google")]
        [Authorize]
        public async Task<IActionResult> HandleGoogleResponse()
        {
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external information");
                return BadRequest("Couldn't get user info");
            }

            var signInResutl = await signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResutl.Succeeded)
                return Ok(signInResutl);

            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email != null)
                {

                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName),
                            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname)
                        };
                        await userManager.CreateAsync(user);
                    }
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(signInResutl);
                }
            }
            return BadRequest();
        }


        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost("UploadImage/{id}")]
        public async Task<IActionResult> UploadImage(string id, IFormFile userImage)
        {
            var userId = userManager.GetUserId(HttpContext.User);
            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    if (user.Id == id)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        if (userImage != null)
                        {
                            string fileName = Guid.NewGuid().ToString();
                            var filePath = Path.Combine(wwwRootPath, @"images\ProfilePhotos");
                            var extension = Path.GetExtension(userImage.FileName);
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
                                userImage.CopyTo(fileStream);
                            }
                            user.ImagePath = @"images\ProfilePhotos\" + fileName + extension;
                            db.SaveChanges();
                            return Ok();
                        }
                    }
                    return BadRequest("Unauthorized user");

                }
                return NotFound("No user found");

            }
            return NotFound("Unauthorized user. You must login first");
        }

        [HttpPost("RemoveImage/{id}")]
        public async Task<IActionResult> RemoveImage(string id)
        {
            var userid = userManager.GetUserId(HttpContext.User);
            if(userid == null)
            {
                return BadRequest("Unauthorized user , Please log in first");
            }
            else
            {
                var user = await userManager.FindByIdAsync(userid);
                if (user == null)
                {
                    return BadRequest("Unauthorized user");
                }
                else
                {
                   if(user.Id != id)
                    {
                        return BadRequest("Unauthorized user, you are not allowed");
                    }
                    else
                    {
                        var oldImage = Path.Combine(_environment.WebRootPath, user.ImagePath.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                        user.ImagePath = @"images\ProfilePhotos\00000000-0000-0000-0000-000000000000.png";
                        db.SaveChanges();
                    }
                }
            }
            return Ok();
        }
    }
}
