using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Interfaces
{
    public interface IUnitOfWork
    {
        //Task BeginAsync();
        //Task CommitAsync();
        //Task RollbackAsync();
        //SqlConnection Connection { get; }
        //SqlTransaction Transaction { get; }
        Task ExecuteInTransactionAsync(Func<Task> action);
    }
}
