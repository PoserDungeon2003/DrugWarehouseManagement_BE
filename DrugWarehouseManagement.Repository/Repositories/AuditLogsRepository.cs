using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class AuditLogsRepository : GenericRepository<AuditLogs>, IAuditLogsRepository
    {
        public AuditLogsRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
