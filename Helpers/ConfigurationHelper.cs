using System;
using System.Configuration;

namespace eMedLis.Helpers
{
    /// <summary>
    /// Centralized configuration management for sensitive settings
    /// Reads from encrypted Web.config
    /// </summary>
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Get JWT Secret Key from Web.config
        /// </summary>
        public static string GetJwtSecretKey()
        {
            try
            {
                string secretKey = ConfigurationManager.AppSettings["Jwt:SecretKey"];

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    throw new ConfigurationErrorsException(
                        "JWT Secret Key is not configured. " +
                        "Please add 'Jwt:SecretKey' to Web.config appSettings.");
                }

                // Validate minimum key length (32 characters minimum for HS256)
                if (secretKey.Length < 32)
                {
                    throw new ConfigurationErrorsException(
                        "JWT Secret Key must be at least 32 characters long for security.");
                }

                return secretKey;
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new InvalidOperationException(
                    "Failed to read JWT configuration from Web.config. " +
                    "Check that encrypted configuration section exists and key is valid.", ex);
            }
        }

        /// <summary>
        /// Get JWT Expiration Minutes from Web.config
        /// </summary>
        public static int GetJwtExpirationMinutes()
        {
            try
            {
                string expirationStr = ConfigurationManager.AppSettings["Jwt:ExpirationMinutes"];

                if (string.IsNullOrWhiteSpace(expirationStr))
                {
                    return 60; // Default to 60 minutes
                }

                if (int.TryParse(expirationStr, out int expiration) && expiration > 0)
                {
                    return expiration;
                }

                throw new ConfigurationErrorsException(
                    "Jwt:ExpirationMinutes must be a positive integer.");
            }
            catch (ConfigurationErrorsException ex)
            {
                throw new InvalidOperationException(
                    "Failed to read JWT Expiration configuration.", ex);
            }
        }

        /// <summary>
        /// Get JWT Issuer from Web.config
        /// </summary>
        public static string GetJwtIssuer()
        {
            try
            {
                string issuer = ConfigurationManager.AppSettings["Jwt:Issuer"];

                if (string.IsNullOrWhiteSpace(issuer))
                {
                    return "eMedLis"; // Default issuer
                }

                return issuer;
            }
            catch
            {
                return "eMedLis";
            }
        }

        /// <summary>
        /// Get JWT Audience from Web.config
        /// </summary>
        public static string GetJwtAudience()
        {
            try
            {
                string audience = ConfigurationManager.AppSettings["Jwt:Audience"];

                if (string.IsNullOrWhiteSpace(audience))
                {
                    return "eMedLisUsers"; // Default audience
                }

                return audience;
            }
            catch
            {
                return "eMedLisUsers";
            }
        }

        /// <summary>
        /// Validate all JWT configuration is properly set
        /// Call this on application startup
        /// </summary>
        public static void ValidateJwtConfiguration()
        {
            try
            {
                // This will throw if configuration is invalid
                var secretKey = GetJwtSecretKey();
                var expiration = GetJwtExpirationMinutes();
                var issuer = GetJwtIssuer();
                var audience = GetJwtAudience();

                // Log validation success (will implement logging in Issue #2)
                System.Diagnostics.Debug.WriteLine(
                    $"✓ JWT Configuration validated successfully. " +
                    $"Issuer: {issuer}, Audience: {audience}, Expiration: {expiration} min");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "JWT Configuration validation failed. " +
                    "Ensure Web.config is properly encrypted and configured.", ex);
            }
        }
    }
}
