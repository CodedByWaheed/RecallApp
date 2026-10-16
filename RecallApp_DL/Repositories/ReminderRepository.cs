using Microsoft.Data.SqlClient;
using RecallApp_DL.Data;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Repositories
{
    public class ReminderRepository :IReminderRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ReminderRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Guid> CreateAsync(Guid userId,Guid savedItemId,DateTime remindAt)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.CreateReminder, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;

            command.Parameters.Add("@RemindAt", SqlDbType.DateTime2).Value = remindAt;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return (Guid)result!;
        }
        public async Task<List<ReminderModel>> GetPendingByUserIdAsync(Guid userId)
        {
            List<ReminderModel> reminders = new();

            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetPendingRemindersByUserId, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reminders.Add(MapReminder(reader));
            }

            return reminders;
        }
        public async Task<List<ReminderModel>> GetBySavedItemIdAsync(Guid userId,Guid savedItemId)
        {
            List<ReminderModel> reminders = new();

            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.GetRemindersBySavedItemId, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@SavedItemId", SqlDbType.UniqueIdentifier).Value = savedItemId;

            await connection.OpenAsync();

            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                reminders.Add(MapReminder(reader));
            }

            return reminders;
        }
        public async Task<bool> UpdateAsync(Guid userId,Guid reminderId,DateTime remindAt)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.UpdateReminder, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@ReminderId", SqlDbType.UniqueIdentifier).Value = reminderId;

            command.Parameters.Add("@RemindAt", SqlDbType.DateTime2).Value = remindAt;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }
        public async Task<bool> SetCompletionAsync(Guid userId,Guid reminderId,bool isCompleted)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.SetReminderCompletion, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@ReminderId", SqlDbType.UniqueIdentifier).Value = reminderId;

            command.Parameters.Add("@IsCompleted", SqlDbType.Bit).Value = isCompleted;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }
        public async Task<bool> DeleteAsync(Guid userId,Guid reminderId)
        {
            using SqlConnection connection =_connectionFactory.CreateConnection();

            using SqlCommand command = new SqlCommand(DbConstants.DeleteReminder, connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;

            command.Parameters.Add("@ReminderId", SqlDbType.UniqueIdentifier).Value = reminderId;

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return result != null && Convert.ToInt32(result) > 0;
        }
        private static ReminderModel MapReminder(SqlDataReader reader)
        {
            return new ReminderModel
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                SavedItemId = reader.GetGuid(
                    reader.GetOrdinal("SavedItemId")),
                RemindAt = reader.GetDateTime(
                    reader.GetOrdinal("RemindAt")),
                IsCompleted = reader.GetBoolean(
                    reader.GetOrdinal("IsCompleted")),
                CreatedAt = reader.GetDateTime(
                    reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
