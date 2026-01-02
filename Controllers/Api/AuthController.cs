using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Security.Claims;
using eMedLis.DAL;
using eMedLis.Models;

namespace eMedLis.Controllers.Api
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            var response = new ApiResponse<LoginResponseData>();

            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    response.Success = false;
                    response.Message = "Username and password required";
                    response.Errors.Add("Validation failed");
                    return Ok(response);
                }

                User user = UserDAL.GetUserByUsername(request.UserName.Trim());

                if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Success = false;
                    response.Message = "Invalid username or password";
                    response.Errors.Add("Authentication failed");
                    return Ok(response);
                }

                if (user.IsLocked || !user.IsActive)
                {
                    response.Success = false;
                    response.Message = user.IsLocked ? "Account is locked" : "Account is inactive";
                    return Ok(response);
                }

                List<string> roles = UserDAL.GetUserRoles(user.UserId);
                UserDAL.UpdateLastLogin(user.UserId);

                // ✅ Generate JWT token
                var tokenHandler = new JwtTokenHandler();
                string jwtToken = tokenHandler.GenerateToken(user.UserId, user.UserName, user.Email, roles);

                response.Success = true;
                response.Message = "Login successful";
                response.Data = new LoginResponseData
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    Roles = roles,
                    Token = jwtToken
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error during login";
                response.Errors.Add(ex.Message);
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public IHttpActionResult Register([FromBody] LoginRequest request)
        {
            var response = new ApiResponse<object>();

            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
                {
                    response.Success = false;
                    response.Message = "Username and password required";
                    response.Errors.Add("Validation failed");
                    return Ok(response); 
                }

                // Hash password
                var (hash, salt) = PasswordHasher.HashPassword(request.Password);

                // Create new user
                User newUser = new User
                {
                    UserName = request.UserName.Trim(),
                    Email = request.UserName, // Use username as email for now
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsActive = true,
                    IsLocked = false
                };

                int userId = UserDAL.CreateUser(newUser);

                response.Success = true;
                response.Message = "User created successfully";
                response.Data = new { UserId = userId };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error creating user";
                response.Errors.Add(ex.Message);
                return Ok(response);  
            }
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public IHttpActionResult Logout()
        {
            var response = new ApiResponse<object>();

            try
            {
                
                var claimsPrincipal = User as ClaimsPrincipal;
                var userIdClaim = claimsPrincipal?.FindFirst("sub");

                if (userIdClaim != null)
                {
                    int userId = int.Parse(userIdClaim.Value);
                    // Log logout if needed
                }

                response.Success = true;
                response.Message = "Logout successful";
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error during logout";
                response.Errors.Add(ex.Message);
                return Ok(response);  
            }
        }

        [HttpGet]
        [Route("validate")]
        [Authorize]
        public IHttpActionResult ValidateToken()
        {
            var response = new ApiResponse<object>();

            try
            {
                var claimsPrincipal = User as ClaimsPrincipal;
                var userIdClaim =
                    claimsPrincipal?.FindFirst("sub") ??
                    claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);

                var usernameClaim =
                    claimsPrincipal?.FindFirst("username") ??
                    claimsPrincipal?.FindFirst(ClaimTypes.Name);

                var emailClaim =
                    claimsPrincipal?.FindFirst("email") ??
                    claimsPrincipal?.FindFirst(ClaimTypes.Email);

                if (usernameClaim == null || userIdClaim == null)
                {
                    response.Success = false;
                    response.Message = "Invalid token";
                    return Ok(response);
                }

                string username = usernameClaim.Value;
                int userId = int.Parse(userIdClaim.Value);
                string email = emailClaim?.Value ?? "";

                User user = UserDAL.GetUserByUsername(username);

                if (user == null || !user.IsActive)
                {
                    response.Success = false;
                    response.Message = "User not found or inactive";
                    return Ok(response);
                }

                // ✅ FIX: Create explicit object with PascalCase properties
                response.Success = true;
                response.Message = "Token is valid";
                response.Data = new
                {
                    UserId = user.UserId.ToString(),    // ← Explicit PascalCase
                    Username = user.UserName,            // ← Explicit PascalCase
                    Email = user.Email ?? email,         // ← Explicit PascalCase
                    Roles = UserDAL.GetUserRoles(user.UserId)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error validating token";
                response.Errors.Add(ex.Message);
                return Ok(response);
            }
        }
    }
}
