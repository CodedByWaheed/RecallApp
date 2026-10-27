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
        public async Task<List<SavedItemModel>?> GetByUserIdAsync(Guid userId)
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
                List<EncryptedContentModel> encryptedContent = new List<EncryptedContentModel>();
                encryptedContent.Add(new EncryptedContentModel
                {
                    Ciphertext = reader.IsDBNull(reader.GetOrdinal("TitleCiphertext")) ? Array.Empty<byte>() : (byte[])reader["TitleCiphertext"],
                    Nonce = reader.IsDBNull(reader.GetOrdinal("TitleNonce")) ? Array.Empty<byte>() : (byte[])reader["TitleNonce"],
                    Tag = reader.IsDBNull(reader.GetOrdinal("TitleTage")) ? Array.Empty<byte>() : (byte[])reader["TitleTage"],
                    FieldType =  2,
                    SavedItemId = reader.GetGuid(reader.GetOrdinal("SavedItemId")),
                    Id = reader.IsDBNull(reader.GetOrdinal("TitleId")) ? Guid.Empty: reader.GetGuid(reader.GetOrdinal("TitleId"))
                });
                encryptedContent.Add(new EncryptedContentModel
                {
                    Ciphertext = reader.IsDBNull(reader.GetOrdinal("ContentCiphertext")) ? Array.Empty<byte>() : (byte[])reader["ContentCiphertext"],
                    Nonce = reader.IsDBNull(reader.GetOrdinal("ContentNonce")) ? Array.Empty<byte>() : (byte[])reader["ContentNonce"],
                    Tag = reader.IsDBNull(reader.GetOrdinal("ContentTag")) ? Array.Empty<byte>() : (byte[])reader["ContentTag"],
                    FieldType = 1,
                    SavedItemId = reader.GetGuid(reader.GetOrdinal("SavedItemId")),
                    Id = reader.IsDBNull(reader.GetOrdinal("ContentId")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("ContentId"))

                });
                encryptedContent.Add(new EncryptedContentModel
                {
                    Ciphertext = reader.IsDBNull(reader.GetOrdinal("NoteCiphertext")) ? Array.Empty<byte>() : (byte[])reader["NoteCiphertext"],
                    Nonce = reader.IsDBNull(reader.GetOrdinal("NoteNonce")) ? Array.Empty<byte>() : (byte[])reader["NoteNonce"],
                    Tag = reader.IsDBNull(reader.GetOrdinal("NoteTag")) ? Array.Empty<byte>() : (byte[])reader["NoteTag"],
                    FieldType = 3,
                    SavedItemId = reader.GetGuid(reader.GetOrdinal("SavedItemId")),
                    Id = reader.IsDBNull(reader.GetOrdinal("NoteId")) ? Guid.Empty : reader.GetGuid(reader.GetOrdinal("NoteId"))

                });
                savedItems.Add(new SavedItemModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("SavedItemId")),
                    UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                    Type = reader.GetByte(reader.GetOrdinal("type")),
                    Priority = reader.GetByte(reader.GetOrdinal("Priority")),
                    Status = reader.GetByte(reader.GetOrdinal("Status")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    encryptedContents = encryptedContent

                });
                
            }
            
            return savedItems?? null;
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
    