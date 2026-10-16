using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using RecallApp_DL.Interfaces;

namespace RecallApp_DL.Data
{
    public sealed class DbUnitOfWork :IUnitOfWork , IDbTransactionContext
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private SqlConnection? _connection;
        private SqlTransaction? _transaction;
        public DbUnitOfWork(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public SqlConnection Connection => _connection ?? throw new InvalidOperationException("Connection has not been initialized.");
        public SqlTransaction Transaction => _transaction ?? throw new InvalidOperationException("Transaction has not been initialized.");

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            if (_connection is not null) 
                throw new InvalidOperationException("Transaction has already started.");

            _connection = _connectionFactory.CreateConnection();
            await _connection.OpenAsync();
            _transaction = (SqlTransaction)await _connection.BeginTransactionAsync();
            try
            {
                await action();
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                await _connection.DisposeAsync();

                _transaction = null;
                _connection = null;
            }
        }
        //public async Task BeginAsync()
        //{
        //    _connection = _connectionFactory.CreateConnection();
        //    await _connection.OpenAsync();
        //    _transaction = (SqlTransaction) await _connection.BeginTransactionAsync();
        //}

        //public async Task CommitAsync()
        //{
        //    if (_transaction is null)
        //        throw new InvalidOperationException("Transaction has not started.");

        //    await _transaction.CommitAsync();

        //    await DisposeAsync();
        //}

        //public async Task RollbackAsync()
        //{
        //    if (_transaction is null)
        //        return;
        //    await _transaction.RollbackAsync();

        //    await DisposeAsync();
        //}
        //private async Task DisposeAsync()
        //{
        //    if (_transaction is not null)
        //    {
        //        await _transaction.DisposeAsync();
        //        _transaction = null;
        //    }
        //    if (_connection is not null)
        //    {
        //        await _connection.CloseAsync();
        //        await _connection.DisposeAsync();
        //        _connection = null;
        //    }
        //}

    }
}
