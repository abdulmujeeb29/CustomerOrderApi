﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CustomerOrderApi.Models;
using CustomerOrderApi.Data;
using MimeKit;
using MailKit.Net.Smtp;

namespace CustomerOrderApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _key = "MIICWwIBAAKBgHZO8IQouqjDyY47ZDGdw9jPDVHadgfT1kP3igz5xamdVaYPHaN24UZMeSXjW9sW";
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            var retrievedUser = _context.Users.FirstOrDefault(u=> u.Email == user.Email);
            
            if (retrievedUser == null)
            {
                return Unauthorized();
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
            if (user == null)
            {
                return BadRequest("User data is null.");
            }
            var existingUser = _context.Users.FirstOrDefault(u=> u.Email==user.Email);
            if (existingUser != null)
            {
                return BadRequest("A user with the mail already exists");
            }

            var existingUsername = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (existingUsername != null)
            {
                return BadRequest("Username taken already ");
            }

            user.EmailVerificationToken = Guid.NewGuid().ToString();
            //Console.WriteLine(user.EmailVerificationToken);
            try
            {
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); //had errors so i had to await the save operation
                try
                {
                    await SendVerificationEmail(user); // Attempt to send the verification email
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine($"Email sending failed: {emailEx.Message}");
                    ModelState.AddModelError(string.Empty, "Registration completed but verification email failed.");
                    return StatusCode(500, ModelState); // Return 500 to indicate email failure
                }

                return Ok(); 
            }

            catch (Exception ex)
            {
                // Log the exception or handle it accordingly
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Registration failed. Please try again.");
                return BadRequest(ModelState); // Return a BadRequest response with ModelState errors
            }

        }



        private async Task SendVerificationEmail(User user)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("thalesmedia229@gmail.com"));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Email Verification";

            //var verificationLink = $"https://localhost:7052/api/Auth/verify-email?token={user.EmailVerificationToken}";
            // Get the BaseUrl from appsettings
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var verificationLink = $"{baseUrl}/api/Auth/verify-email?token={user.EmailVerificationToken}";

            email.Body = new TextPart("plain")
            {
                Text = $"Click the link to verify your email : {verificationLink}"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("thalesmedia29@gmail.com", "dzzypmguiznfkwnr");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

        }
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailVerificationToken == token);

            if (user == null) 
                {
                    return BadRequest("Invalid verification token.");
                }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = token;
            await _context.SaveChangesAsync();

            return Ok("Email verified Successfully");
            }


            [HttpPost("logout")]
        public IActionResult Logout()
        {
            
            return Ok(new { Message = "Logged out successfully" });
        }
    }

}
