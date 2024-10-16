using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CustomerOrderApi.Models;
using CustomerOrderApi.Data;

namespace CustomerOrderApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _key = "MIICWwIBAAKBgHZO8IQouqjDyY47ZDGdw9jPDVHadgfT1kP3igz5xamdVaYPHaN24UZMeSXjW9sW";
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var retrievedUser = _context.Users.FirstOrDefault(u=> u.Email == user.Email);
            //hardcoded user credentials just to keep things basic and focus on the jwt authentication task 
            if (retrievedUser == null)
            {
                return NotFound();
            }

            if (retrievedUser.Password == user.Password)
            {
                var tokenHandler = new JwtSecurityTokenHandler();   
                var key = Encoding.UTF8.GetBytes(_key);

                // Define the token claims and expiration
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Email)

                    }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                // Create and return the JWT
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(new {Token = tokenString});
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); //had errors so i had to await the save operation

                return Ok(); 
            }

            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return BadRequest(ModelState); // Return a BadRequest response with ModelState errors
            }

        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            
            return Ok(new { Message = "Logged out successfully" });
        }
    }

}
