using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AccountProvider.Functions
{
    public class SignIn
    {
        private readonly UserManager<UserAccount> _userManager;
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ILogger<SignIn> _logger;

        public SignIn(UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ILogger<SignIn> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            try
            {
                string body = await new StreamReader(req.Body).ReadToEndAsync();
                var ulr = JsonConvert.DeserializeObject<UserLoginRequest>(body);

                if (ulr != null && !string.IsNullOrEmpty(ulr.Email) && !string.IsNullOrEmpty(ulr.Password))
                {
                    var userAccount = await _userManager.FindByEmailAsync(ulr.Email);
                    var result = await _signInManager.CheckPasswordSignInAsync(userAccount!, ulr.Password, true);
                    if (result.Succeeded)
                    {
                        var token = GenerateJwtToken(userAccount!);
                            _logger.LogInformation($"Token generated: {token}"); ; // Generate JWT token
                        return new OkObjectResult(new { token }); // Return token
                    }
                    return new UnauthorizedResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SignIn function: {ex.Message}");
            }

            return new BadRequestResult();
        }

        private string GenerateJwtToken(UserAccount user)
        {
            var claims = new List<Claim>
        {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Using user Id as NameIdentifier claim
        // Add more claims as needed
         };

            var key = GenerateRandomKey(32); // Generate a random key of 256 bits (32 bytes)

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private byte[] GenerateRandomKey(int keySizeInBytes)
        {
            var rng = new RNGCryptoServiceProvider();
            var keyBytes = new byte[keySizeInBytes];
            rng.GetBytes(keyBytes);
            return keyBytes;
        }
    }
}
