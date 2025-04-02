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
        Task<byte[]> ExportInventoryReportPdf(int warehouseId, Instant startDate, Instant endDate);
        Task<byte[]> ExportStockCardPdf(int warehouseId, int productId, Instant startDate, Instant endDate);
    }
}
