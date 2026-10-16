using RecallApp_DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<long> CreateAsync(AuditLogModel model);

    }
}
