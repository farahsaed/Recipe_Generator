using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Recipe_Generator.DTO;
using Recipe_Generator.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Recipe_Generator
{
    public class JwtHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;
        private readonly IConfigurationSection _goolgeSettings;
        private readonly UserManager<User> _userManager;
        public JwtHandler(IConfiguration configuration, UserManager<User> userManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JWT");
            _goolgeSettings = _configuration.GetSection("ExternalAuth:Google");
        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuthDTO externalAuthDTO)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new List<string>() { _goolgeSettings.GetSection("ClientID").Value }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(externalAuthDTO.Token, settings);
                return payload;
            }
            catch (Exception ex) { return null; }
        }

    }
}
