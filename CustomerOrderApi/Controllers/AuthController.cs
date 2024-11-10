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
using System.Linq;
using Azure.Identity;
using CustomerOrderApi.Request;
using Microsoft.AspNetCore.Identity;
using CustomerOrderApi.DTOs;
using CustomerOrderApi.Repositories.Interface;

namespace CustomerOrderApi.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string _key = "MIICWwIBAAKBgHZO8IQouqjDyY47ZDGdw9jPDVHadgfT1kP3igz5xamdVaYPHaN24UZMeSXjW9sW";
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginuserDto)
        {
            var retrievedUser = await _unitOfWork.Users.GetUserByEmailAsync(loginuserDto.Email);

            if (retrievedUser == null)
            {
                return Unauthorized();
            }

            if (retrievedUser.Password == loginuserDto.Password)
            {
                var tokenHandler = new JwtSecurityTokenHandler();   
                var key = Encoding.UTF8.GetBytes(_key);

                // Define the token claims and expiration
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, loginuserDto.Email)

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
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registeruserDto)
        {
            if (registeruserDto == null)
            {
                return BadRequest("User data is null.");
            }
            var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(registeruserDto.Email);
            if (existingUser != null)
            {
                return BadRequest("A user with the mail already exists");
            }

            var existingUsername = await _unitOfWork.Users.GetUserByUsernameAsync(registeruserDto.UserName);
            if (existingUsername != null)
            {
                return BadRequest("Username taken already ");
            }

            // Generate email verification token
            var emailVerificationToken = Guid.NewGuid().ToString();
            //Console.WriteLine(user.EmailVerificationToken);


            // Map RegisterUserDto to User model
            var user = new User
            {
                UserName = registeruserDto.UserName,
                Email = registeruserDto.Email,
                Password = registeruserDto.Password,  // Ensure to hash the password in production
                EmailVerificationToken = emailVerificationToken
            };
            try
            {

                ;

                await _unitOfWork.Users.AddUserAsync(user);
                await _unitOfWork.SaveChangesAsync();  //had errors so i had to await the save operation
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
            var username = await _unitOfWork.Users.GetUserByUsernameAsync(user.UserName);
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
                Text = $@"Dear {username},

                 Thank you for registering with us! We’re excited to have you on board.

                To complete your registration and activate your account, please verify your email address by clicking the link below:

                 [Verify Your Email]({verificationLink})

                 If the link does not work, you can copy and paste the URL into your web browser:

                   {verificationLink}

                 If you did not create an account with us, please disregard this email.

                     Thank you,  
                  The CustomerOrderWeb Team"
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
            //var user = _context.Users.FirstOrDefault(u => u.EmailVerificationToken == token);
            var user = await _unitOfWork.Users.GetUserByEmailVerificationTokenAsync(token);
            if (user == null) 
                {
                    return BadRequest("Invalid verification token.");
                }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = token;
            await _unitOfWork.SaveChangesAsync();

            return Ok("Email verified Successfully");
            }


            [HttpPost("logout")]
        public IActionResult Logout()
        {
            
            return Ok(new { Message = "Logged out successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var user = await _unitOfWork.Users.GetUserByEmailAsync(email);

            if (user == null)
            {
                return BadRequest("User with this email does not exist.");
            }

            // Generate reset token and expiry
            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _unitOfWork.SaveChangesAsync();

            try
            {
                await SendResetPasswordEmail(user); // Send email with reset link
            }
            catch (Exception emailEx)
            {
                Console.WriteLine($"Email sending failed: {emailEx.Message}");
                return StatusCode(500, "Error sending email.");
            }

            return Ok("Password reset link has been sent to your email.");
        }

        private async Task SendResetPasswordEmail(User user)
        {
            var username = await _unitOfWork.Users.GetUserByUsernameAsync(user.UserName);
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("your-email@example.com"));
            email.To.Add(MailboxAddress.Parse(user.Email));
            email.Subject = "Password Reset";

            // Get the BaseUrl from appsettings
            var baseUrl = _configuration["ApiSettings:frontEndBaseUrl"];
            var resetLink = $"{baseUrl}/Authentication/ResetPassword?token={user.PasswordResetToken}";

            email.Body = new TextPart("plain")
            {
                Text = $@"Dear {username},

                We received a request to reset your password. If you did not make this request, you can safely ignore this email.

                 To reset your password, please click the link below:

                 [Reset Your Password]({resetLink})

                If the link does not work, you can copy and paste the following URL into your web browser:

                 {resetLink}

                 For security reasons, this link will expire in 24 hours. After resetting your password, you will be able to log in to your account with your new password.

                 Thank you,  
                 The CustomerOrderWeb Team"
            };


            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("thalesmedia29@gmail.com", "dzzypmguiznfkwnr");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, [FromBody] string newPassword)
        {
            //var user = _context.Users.FirstOrDefault(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow);
            var user = await _unitOfWork.Users.GetUserByPasswordResetTokenAsync(token);

            if (user == null)
            {
                return BadRequest("Invalid or expired token.");
            }

            // Update user's password and clear the reset token
            user.Password = newPassword; // Hash this password in a real application
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _unitOfWork.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }


        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword( [FromBody] ChangePasswordRequest request)
        {
            var userEmail = User.Identity?.Name;
            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);

            if (user == null) {
                return Unauthorized("User not Found");
                    }
            
            if (user.Password != request.CurrentPassword)
            {
                return BadRequest("cURRENT pASSWORD IS in inCORRECT");
            }
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("New password field do not match with the confirm password field");
            }
            user.Password = request.NewPassword;
            await _unitOfWork.SaveChangesAsync();

            return Ok("Password changed successfully.");
        }

        [HttpPost("check-user")]
        public async Task<IActionResult> CheckUser(CheckUserDto checkuserdto)

        {
            var user = await _unitOfWork.Users.GetUserByUsernameAsync(checkuserdto.Email);
            if (user != null)
            {
                return Ok("User successfully retrieved");
            }
            else
            {
                return BadRequest("User not found");
            }
        }
    }

}
