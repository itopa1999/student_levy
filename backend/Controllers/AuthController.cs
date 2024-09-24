using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.Mappers;
using backend.models;
using backend.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("auth/api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public readonly ApplicationDBContext _context;
        public readonly IJwtRepository _token;
        public readonly UserManager<AppUser> _userManager;
        public readonly SignInManager<AppUser> _signInManager;
        public readonly IUserRepository _userRepo;
        
        public AuthController(
            IUserRepository userRepo,
            ApplicationDBContext context,
            IJwtRepository token,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager

        )
        {
            _userRepo = userRepo;
            _context = context;
            _token = token;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpPost("create/admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto createAdminDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            try{
              var user = new AppUser{
                Email = createAdminDto.Email,
                UserName = createAdminDto.Username,
                FirstName = createAdminDto.FirstName,
                LastName = createAdminDto.LastName,
                CreatedAt = DateOnly.FromDateTime(DateTime.Now),
                IsAdmin = true,
                IsStudent = false
              };
              var userModel = await _userManager.CreateAsync(user, createAdminDto.Password);
              if (userModel.Succeeded){
                var role = await _userManager.AddToRoleAsync(user, "Admin");
                if (role.Succeeded){
                    return StatusCode(201, new {message = $"Admin Account Successfully Created for {createAdminDto.Username}"});
                }else{return StatusCode(500, new {message = role.Errors});}
                
              }else{return StatusCode(500, new {message = userModel.Errors});}

            }catch(Exception e) {
                return StatusCode(500, new {message = e});
            }
            
        }

        [HttpPost("login/admin")]
        public async Task<IActionResult> LoginAdmin([FromBody] LoginDto loginDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(x=> x.UserName == loginDto.Username);
            if (user == null){
                return BadRequest(new { message = "incorrect credentials"});
            }else{
                var loginUser = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
                if (!loginUser.Succeeded){
                    return BadRequest(new { message = "incorrect credentials"});
                }else{
                    var roles = await _userManager.GetRolesAsync(user);
                    return StatusCode (200,new {
                    message = "login successfully",
                    firstname = user.FirstName,
                    lastname = user.LastName,
                    email = user.Email,
                    roles,
                    IsAdmin = user.IsAdmin,
                    IsStudent = user.IsStudent,
                    username = user.UserName,
                    token = _token.CreateToken(user),
                });
                }
            }
        }
        
        [HttpPost("forgot/password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var userModel = await _userManager.Users.FirstOrDefaultAsync(x=> x.UserName == forgotPasswordDto.Username);
            if (userModel == null){
                return StatusCode(400, new{message = "Username Not Found"});
            }else{
                var exisitingOtp = await _context.Otps.FirstOrDefaultAsync(x=> x.AppUserId == userModel.Id);
                if (exisitingOtp == null){
                    var otp = new Otp{
                        Token = _token.GenerateToken(),
                        AppUserId = userModel.Id
                    };
                    await _context.Otps.AddAsync(otp);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Token is {otp.Token}");
                    return StatusCode(200, new{message = $"Otp has been sent and here is your otp {otp.Token}",username = userModel.UserName});
                }
                exisitingOtp.Token = _token.GenerateToken();
                exisitingOtp.CreatedAt = DateTime.Now;
                exisitingOtp.IsActive = true;

                await _context.SaveChangesAsync();
                Console.WriteLine($"Token is {exisitingOtp.Token}");
                return StatusCode(200, new{message = $"Otp has been sent and here is your otp {exisitingOtp.Token}",username = userModel.UserName});
  
            }
            
        }

        [HttpPost("verify/otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var userModel = await _userManager.Users.FirstOrDefaultAsync(x=> x.UserName == verifyOtpDto.Username);
            if (userModel == null){
                return StatusCode(400, new{message = "Username Not Found"});
            }
            var getotp = await _context.Otps.FirstOrDefaultAsync(x=>x.Token == verifyOtpDto.Token);
            if (getotp == null){
                return StatusCode(400, new{message = "Otp is not correct"});
            }else if (getotp.IsActive == false){
                return StatusCode(400, new{message = "Otp is has been used"});
            }else if (getotp.CreatedAt.AddMinutes(10) <= DateTime.Now ){
                getotp.IsActive = false;
                getotp.CreatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return StatusCode(400, new {message = "token has expired"});
            }
            else{
                getotp.IsActive = false;
                getotp.CreatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(userModel);
                string newPassword = _token.RandomPassword();
                IdentityResult result = await _userManager.ResetPasswordAsync(userModel, resetToken, newPassword);
                if (result.Succeeded){
                    Console.WriteLine($"new password: {newPassword}");
                    return StatusCode(200, new{message=$"your password has been reset, here is your new password {newPassword}"});
                }
                return StatusCode(400, new {message=result.Errors});               
            }

        }

        [HttpPost("change/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if (passwordDto.Password1 != passwordDto.Password2){
                return StatusCode(400, new{message="password mismatch"});
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }
            var result = await _userManager.ChangePasswordAsync(user, passwordDto.OldPassword, passwordDto.Password1);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { message = "Password change failed", errors });
            }
            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return Ok(new { message = "User logged out successfully" });
        }

    }
}