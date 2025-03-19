using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IInventoryReportService
    {
        byte[] ExportInventoryReport(int warehouseId, Instant startDate, Instant endDate);
    }
}
