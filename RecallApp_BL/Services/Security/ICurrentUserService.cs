using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_BL.Services.Security
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
    }
}
