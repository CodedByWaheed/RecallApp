
using Microsoft.Extensions.Logging;
using RecallApp_BL.Services.Interfaces;
using RecallApp_DL.Interfaces;
using RecallApp_DL.Models;
using RecallApp_DL.Repositories.Interfaces;

namespace RecallApp_BL.Services
{
    public sealed class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _repository;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(IAuditLogRepository repository,ILogger<AuditLogService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<long> LogAsync(AuditLogModel model)
        {
            try
            {
                return await _repository.CreateAsync(model);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,"Failed to write audit log. Action: {Action}, EntityType: {EntityType}, TraceId: {TraceId}",
                    model.Action,model.EntityType,model.TraceId);
                throw;
            }
        }
    }
}
