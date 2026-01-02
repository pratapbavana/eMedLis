using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using eMedLis.Models;

namespace eMedLis.DAL
{
    public class UserDAL
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["emeddb"].ConnectionString;

        public static User GetUserByUsername(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserId, Username, Email, PasswordHash, PasswordSalt, FirstName, LastName, IsActive, IsLocked FROM Users WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserId = (int)reader["UserId"],
                                UserName = reader["Username"].ToString(),
                                Email = reader["Email"].ToString(),
                                PasswordHash = reader["PasswordHash"].ToString(),
                                PasswordSalt = reader["PasswordSalt"].ToString(),
                                FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : null,
                                LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : null,
                                IsActive = (bool)reader["IsActive"],
                                IsLocked = (bool)reader["IsLocked"]
                            };
                        }
                    }
                }
            }
            return null;
        }

        public static int CreateUser(User user)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, FirstName, LastName, IsActive, IsLocked) 
                                VALUES (@Username, @Email, @PasswordHash, @PasswordSalt, @FirstName, @LastName, @IsActive, @IsLocked);
                                SELECT SCOPE_IDENTITY();";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", user.UserName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    cmd.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName ?? "");
                    cmd.Parameters.AddWithValue("@LastName", user.LastName ?? "");
                    cmd.Parameters.AddWithValue("@IsActive", 1);
                    cmd.Parameters.AddWithValue("@IsLocked", 0);
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public static bool UpdateLastLogin(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET LastLoginDate = GETUTCDATE(), FailedLoginAttempts = 0 WHERE UserId = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static List<string> GetUserRoles(int userId)
        {
            List<string> roles = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT r.RoleName FROM UserRoles ur INNER JOIN Roles r ON ur.RoleId = r.RoleId WHERE ur.UserId = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader["RoleName"].ToString());
                        }
                    }
                }
            }
            return roles;
        }
    }
}
