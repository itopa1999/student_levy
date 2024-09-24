using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Dtos
{
    public class AuthDtos
    {
        public int MyProperty { get; set; }
    }

    public class CreateAdminDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? Password { get; set; }

    }


    public class LoginDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required]
        public string? Username { get; set; }
    }

    public class VerifyOtpDto
    {
        [Required]
        public string? Username { get; set; }
        public int Token { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string? OldPassword { get; set; }
        [Required]
        public string? Password1 { get; set; }
        [Required]
        public string? Password2 { get; set; }
    }

}
