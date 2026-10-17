using Microsoft.Data.SqlClient;
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
    public class AuditLogRepository : IAuditLogRepository
    {
        public readonly IDbConnectionFactory _connectionFactory;
        public AuditLogRepository (IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<long> CreateAsync(AuditLogModel model)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand("dbo.sp_AuditLogs_Create",connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = (object?)model.UserId ?? DBNull.Value;

            command.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = model.Action;

            command.Parameters.Add("@EntityType", SqlDbType.NVarChar, 100).Value = model.EntityType;

            command.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value = (object?)model.EntityId ?? DBNull.Value;

            command.Parameters.Add("@Outcome", SqlDbType.NVarChar, 20).Value = model.Outcome;

            command.Parameters.Add("@TraceId", SqlDbType.NVarChar, 100).Value = (object?)model.TraceId ?? DBNull.Value;

            command.Parameters.Add("@Details", SqlDbType.NVarChar, 500).Value = (object?)model.Details ?? DBNull.Value;
           
            command.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 45).Value = (object?)model.IpAddress ?? DBNull.Value;
            
            command.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 512).Value = (object?)model.UserAgent ?? DBNull.Value;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
            {
                throw new InvalidOperationException("Failed to create audit log.");
            }

            return Convert.ToInt64(result);
        }
    }
}
