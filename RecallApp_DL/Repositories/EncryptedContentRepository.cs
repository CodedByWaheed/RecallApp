using Microsoft.Data.SqlClient;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Data;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Repositories
{
    public class EncryptedContentRepository : IEncryptedContentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDbTransactionContext _transactionContext;
        public EncryptedContentRepository(IDbConnectionFactory connectionFactory , IDbTransactionContext transactionContext)
        {
            _connectionFactory = connectionFactory;
            _transactionContext = transactionContext;
        }

        public async Task CreateAsync(EncryptedContentModel model)
        {
            using SqlCommand command = new SqlCommand(DbConstants.CreateEncryptedContent,_transactionContext.Connection,_transactionContext.Transaction);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = model.SavedItemId;

            command.Parameters.Add("@FieldType", SqlDbType.TinyInt).Value = model.FieldType;

            command.Parameters.Add("@Ciphertext", SqlDbType.VarBinary, -1).Value = model.Ciphertext;

            command.Parameters.Add("@Nonce", SqlDbType.VarBinary, 12).Value = model.Nonce;

            command.Parameters.Add("@Tag", SqlDbType.VarBinary, 16).Value = model.Tag;

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<EncryptedContentModel>> GetBySavedItemIdAsync(Guid userId, Guid savedItemId)
        {
            List<EncryptedContentModel> encryptedContents = new List<EncryptedContentModel>(); // or just simple use :: = new(); ::
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetEncryptedContentsBySavedItemId, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                EncryptedContentModel model = new EncryptedContentModel
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    SavedItemId = reader.GetGuid(reader.GetOrdinal("SavedItemId")),
                    FieldType = reader.GetByte(reader.GetOrdinal("FieldType")),
                    Ciphertext = (byte[])reader["Ciphertext"],
                    Nonce = (byte[])reader["Nonce"],
                    Tag = (byte[])reader["Tag"]
                };
                encryptedContents.Add(model);
            }
            return encryptedContents;
        }

        public async Task UpdateAsync(EncryptedContentModel model)
        {
            using SqlCommand command = new SqlCommand(DbConstants.UpdateEncryptedContent,_transactionContext.Connection,_transactionContext.Transaction);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = model.SavedItemId;

            command.Parameters.Add("@FieldType", SqlDbType.TinyInt).Value = model.FieldType;

            command.Parameters.Add("@Ciphertext", SqlDbType.VarBinary, -1).Value = model.Ciphertext;

            command.Parameters.Add("@Nonce", SqlDbType.VarBinary, 12).Value = model.Nonce;

            command.Parameters.Add("@Tag", SqlDbType.VarBinary, 16).Value = model.Tag;

            await command.ExecuteNonQueryAsync();
        }

    }
}
