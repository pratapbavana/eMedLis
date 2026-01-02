using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using eMedLis.Models;

namespace eMedLis.Models
{
    public class JwtTokenHandler
    {
        private readonly string _secretKey;
        private readonly int _expirationMinutes;
        private readonly string _issuer;
        private readonly string _audience;

        /// <summary>
        /// Constructor with configuration parameters
        /// </summary>
        public JwtTokenHandler()
        {
            _secretKey = "GTYGDhgyuteYTYE56785HESF879EFUGEDFYH32UGDJKHukuyerewh";
            _expirationMinutes = 60;
            _issuer = "eMedLis";
            _audience = "eMedLisUsers";
        }

        /// <summary>
        /// Generate JWT token from user claims
        /// </summary>
        public string GenerateToken(int userId, string username, string email, List<string> roles)
        {
            try
            {
                // Create claims list
                var claims = new List<Claim>
                {
                    new Claim("sub", userId.ToString()),                    // Subject (user ID)
                    new Claim("username", username),                        // Username
                    new Claim("email", email),                              // Email
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT ID
                };

                // Add roles as claims
                if (roles != null && roles.Count > 0)
                {
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }

                // Create signing key from secret
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create token
                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    signingCredentials: credentials
                );

                // Write token to string
                var tokenHandler = new JwtSecurityTokenHandler();
                string jwtToken = tokenHandler.WriteToken(token);

                return jwtToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating JWT token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate JWT token and extract claims
        /// </summary>
        public (bool isValid, ClaimsPrincipal claimsPrincipal, string errorMessage) ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero  // No tolerance for expired tokens
                };

                // Validate and get principal
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                return (true, principal, null);
            }
            catch (SecurityTokenExpiredException)
            {
                return (false, null, "Token has expired");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return (false, null, "Invalid token signature");
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return (false, null, "Invalid token issuer");
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return (false, null, "Invalid token audience");
            }
            catch (Exception ex)
            {
                return (false, null, $"Token validation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Decode token without validation (for debugging only)
        /// </summary>
        public JwtSecurityToken DecodeToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    throw new Exception("Invalid token format");
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error decoding token: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extract user ID from token claims
        /// </summary>
        public int? ExtractUserId(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                var claim = claimsPrincipal?.FindFirst("sub");

                if (claim != null && int.TryParse(claim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract username from token claims
        /// </summary>
        public string ExtractUsername(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                return claimsPrincipal?.FindFirst("username")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract email from token claims
        /// </summary>
        public string ExtractEmail(ClaimsPrincipal claimsPrincipal)
        {
            try
            {
                return claimsPrincipal?.FindFirst("email")?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract roles from token claims
        /// </summary>
        public List<string> ExtractRoles(ClaimsPrincipal claimsPrincipal)
        {
            var roles = new List<string>();
            try
            {
                var roleClaims = claimsPrincipal?.FindAll(ClaimTypes.Role);

                if (roleClaims != null)
                {
                    foreach (var claim in roleClaims)
                    {
                        roles.Add(claim.Value);
                    }
                }

                return roles;
            }
            catch
            {
                return roles;
            }
        }
    }
}
