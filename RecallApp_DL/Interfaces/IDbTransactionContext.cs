using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RecallApp_DL.Interfaces
{
    public interface IDbTransactionContext
    {
        SqlConnection Connection { get; }
        SqlTransaction Transaction { get; }
    }
}
