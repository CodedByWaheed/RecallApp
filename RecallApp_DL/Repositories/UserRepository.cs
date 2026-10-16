using Microsoft.Data.SqlClient;
using RecallApp_DL.Data;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Repositories
{
    public sealed class UserRepository:IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public UserRepository(IDbConnectionFactory ConnectionFactory)
        {
            _connectionFactory = ConnectionFactory;
        }

        public async Task<UserModel?> GetByIdAsync(Guid ID)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetUserByID, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@ID" , SqlDbType.UniqueIdentifier).Value = ID;
            
            await connection.OpenAsync();
            
            SqlDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;
            
            return new UserModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Email = reader.GetString(reader.GetOrdinal("Email")),   
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        public async Task<Guid> CreateAsync(UserModel user)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.CreateUser, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 320).Value = user.Email;
            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = user.PasswordHash;
            await connection.OpenAsync();
            object? result = await command.ExecuteScalarAsync();
            if(result is not Guid userID)
                throw new InvalidOperationException("Failed to create user and retrieve the new ID.");
            return userID;
        }
        public async Task<UserModel?> GetByEmailAsync(string email)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetUserByEmail, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 320).Value = email;
            await connection.OpenAsync();
            SqlDataReader reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;
            return new UserModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        public async Task<bool> UpdateEmailAsync(Guid userId, string email)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.UpdateUserEmail, connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@Email", SqlDbType.NVarChar, 320).Value = email.Trim().ToLowerInvariant();
               
            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> UpdatePasswordHashAsync(Guid userId,string passwordHash)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.UpdateUserPasswordHash, connection);
           

            command.CommandText = DbConstants.UpdateUserPasswordHash;
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = userId;


            command.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 512).Value = passwordHash;
               
            await connection.OpenAsync();
            object? result = await command.ExecuteScalarAsync();

            return result is not null&& result != DBNull.Value && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> DeleteAsync(Guid userId)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.DeleteUser, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                
            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result is not null
                && result != DBNull.Value
                && Convert.ToBoolean(result);
        }


    }
}
