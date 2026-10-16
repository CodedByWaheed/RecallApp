using RecallApp_DL.Interfaces;
using System;
using RecallApp_DL.Models;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using RecallApp_DL.Data;
using System.Data;

namespace RecallApp_DL.Repositories
{
    public sealed class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionFactory _connectionFactory; 
        public RefreshTokenRepository(IDbConnectionFactory ConnectionFactory)
        {
            _connectionFactory = ConnectionFactory;
        }
        public async Task<RefreshTokenModel?> GetByTokenHashAsync(byte[] tokenHash)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.GetRefreshTokenByTokenHash);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@TokenHash", SqlDbType.VarBinary, 32).Value = tokenHash;

            await connection.OpenAsync();

            using SqlDataReader reader =await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new RefreshTokenModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                TokenHash = (byte[])reader["TokenHash"],
                ExpiresAtUtc = reader.GetDateTime(reader.GetOrdinal("ExpiresAtUtc")),
                CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
                RevokedAtUtc = reader.IsDBNull(reader.GetOrdinal("RevokedAtUtc")) ? null: reader.GetDateTime(reader.GetOrdinal("RevokedAtUtc")),
                ReplacedByTokenHash = reader.IsDBNull(reader.GetOrdinal("ReplacedByTokenHash")) ? null: (byte[])reader["ReplacedByTokenHash"]
            };
        }
        public async Task CreateAsync(RefreshTokenModel model)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.CreateRefreshToken, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = model.UserId;
            command.Parameters.Add("@TokenHash", SqlDbType.VarBinary, 32).Value = model.TokenHash;
            command.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = model.ExpiresAtUtc;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        public async Task<Guid?> RotateAsync(byte[] oldTokenHash,byte[] newTokenHash,DateTime newExpiresAtUtc)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();
            using SqlCommand command = new SqlCommand(DbConstants.RotateRefreshToken, connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@OldTokenHash", SqlDbType.VarBinary, 32).Value = oldTokenHash;
            command.Parameters.Add("@NewTokenHash", SqlDbType.VarBinary, 32).Value = newTokenHash;
            command.Parameters.Add("@NewExpiresAtUtc", SqlDbType.DateTime2).Value = newExpiresAtUtc;
            await connection.OpenAsync();
            object? result = await command.ExecuteScalarAsync();
            if (result is null || result == DBNull.Value)
                return null;

            return (Guid)result;
        }
        public async Task RevokeAsync(byte[] tokenHash)
        {
            using SqlConnection connection = _connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.RevokeRefreshToken, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@TokenHash", SqlDbType.VarBinary, 32).Value = tokenHash;

            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }

    }
}
