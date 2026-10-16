using Microsoft.Data.SqlClient;
using RecallApp_DL.Data;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public TagRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Guid userId, string name)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.CreateTag, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return (Guid)result!;
        }

        public async Task<List<TagModel>> GetByUserIdAsync(Guid userId)
        {
            List<TagModel> tags = new();

            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetTagsByUserId, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier)
                .Value = userId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tags.Add(new TagModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            return tags;
        }

        public async Task<bool> UpdateAsync(Guid tagId,Guid userId,string name)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.UpdateTag, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = tagId;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task<bool> DeleteAsync(Guid tagId, Guid userId)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.DeleteTag, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = tagId;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task AssignAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.AssignTagToSavedItem, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@TagId", SqlDbType.UniqueIdentifier).Value = tagId;

            await connection.OpenAsync();

            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> RemoveAsync(Guid userId,Guid savedItemId,Guid tagId)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.RemoveTagFromSavedItem, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@TagId", SqlDbType.UniqueIdentifier).Value = tagId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }

        public async Task<List<TagModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            List<TagModel> tags = new();

            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetTagsBySavedItemId, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                tags.Add(new TagModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            return tags;
        }
    }
}
