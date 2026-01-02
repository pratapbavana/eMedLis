using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eMedLis.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
    public class LoginRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public LoginResponseData Data { get; set; }
    }

    public class LoginResponseData
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    public class AuthUserData
    {
        public List<string> Roles { get; set; }
        public string Token { get; set; }
    }
}
