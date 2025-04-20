using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
   public  interface IDashboardService
    {
        Task<DashboardReportDto> GetDashboardReportAsync(string role,TimeFilterOption filterOption);

    }
}
