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
    public class SavedItemRepository : ISavedItemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbTransactionContext _transactionContext;
        public SavedItemRepository(IDbConnectionFactory connectionFactory, IDbTransactionContext transactionContext)
        {
            _connectionFactory = connectionFactory;
            _transactionContext = transactionContext;
        }
        public async Task<SavedItemModel?> GetByIdAsync(Guid id)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetSavedItemByID, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@Id" , SqlDbType.UniqueIdentifier).Value = id; 
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if(!await reader.ReadAsync())
            {
                return null;
            }
            return new SavedItemModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),

                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),

                Type = reader.GetByte(reader.GetOrdinal("Type")),

                Priority = reader.GetByte(reader.GetOrdinal("Priority")),

                Status = reader.GetByte(reader.GetOrdinal("Status")),

                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),

                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))

            };
        }
        public async Task<Guid>CreateAsync(SavedItemModel model)
        {
               
            using SqlCommand command = new SqlCommand(DbConstants.CreateSavedItem,_transactionContext.Connection,_transactionContext.Transaction);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = model.UserId;
            command.Parameters.Add("@Type", SqlDbType.TinyInt).Value = model.Type;
            command.Parameters.Add("@Status", SqlDbType.TinyInt).Value = model.Status;
            command.Parameters.Add("@Priority", SqlDbType.TinyInt).Value = model.Priority;

            object? result = await command.ExecuteScalarAsync();

            return (Guid)result!;
        }
        public async Task<List<SavedItemModel>> GetByUserIdAsync(Guid userId)
        {
            List<SavedItemModel> savedItems = new List<SavedItemModel>();
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetSavedItemsByUserId, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserID", SqlDbType.UniqueIdentifier).Value = userId;
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                savedItems.Add(new SavedItemModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Type = reader.GetByte(reader.GetOrdinal("Type")),
                    Priority = reader.GetByte(reader.GetOrdinal("Priority")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });
            }
            return savedItems;
        }
        public async Task UpdateAsync(SavedItemModel model)
        {
            using SqlCommand command = new SqlCommand(DbConstants.UpdateSavedItem, _transactionContext.Connection,_transactionContext.Transaction);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = model.Id;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = model.UserId;

            command.Parameters.Add("@Type", SqlDbType.TinyInt).Value = model.Type;

            command.Parameters.Add("@Priority", SqlDbType.TinyInt).Value = model.Priority;

            command.Parameters.Add("@Status", SqlDbType.TinyInt).Value = model.Status;

            await command.ExecuteNonQueryAsync();
        }
        public async Task<bool> ArchiveAsync( Guid savedItemId,Guid userId)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.ArchiveSavedItem,connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id",SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@UserId",SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result is bool success && success;
        }
        public async Task<bool> RestoreAsync(Guid savedItemId,Guid userId)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.RestoreSavedItem,connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result is bool success && success;
        }
        public async Task<List<SavedItemModel>> GetArchivedByUserIdAsync(Guid userId)
        {
            List<SavedItemModel> savedItems = new();

            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetArchivedSavedItemsByUserId,connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId",SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                savedItems.Add(new SavedItemModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Type = reader.GetByte(reader.GetOrdinal("Type")),
                    Priority = reader.GetByte(reader.GetOrdinal("Priority")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });
            }

            return savedItems;
        }
        public async Task<bool> DeleteAsync(Guid savedItemId,Guid userId)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.DeleteSavedItem,connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result is bool success && success;
        }
        public async Task<List<SavedItemModel>> GetByTagIdAsync(Guid userId,Guid tagId)
        {
            List<SavedItemModel> savedItems = new();

            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetSavedItemsByTagId,connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@TagId", SqlDbType.UniqueIdentifier).Value = tagId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                savedItems.Add(new SavedItemModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Type = reader.GetByte(reader.GetOrdinal("Type")),
                    Priority = reader.GetByte(reader.GetOrdinal("Priority")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });
            }

            return savedItems;
        }

    }
}
    