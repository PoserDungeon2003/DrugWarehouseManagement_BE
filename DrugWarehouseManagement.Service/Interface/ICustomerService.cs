using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface ICustomerService
    {
        Task<BaseResponse> CreateCustomerAsync(CreateCustomerRequest request);

        // Lấy thông tin chi tiết 1 khách hàng
        Task<CustomerResponse> GetCustomerDetailByIdAsync(int customerId);

        // Tìm kiếm khách hàng theo Id, Name, PhoneNumber, có phân trang
        Task<PaginatedResult<CustomerResponse>> SearchCustomersAsync(SearchCustomerRequest request);

        // Sửa khách hàng
        Task UpdateCustomerAsync(int customerId, UpdateCustomerRequest request);

        // Xoá mềm (status -> Inactive)
        Task <BaseResponse> DeleteCustomerAsync(int customerId);

        // Lấy danh sách khách hàng thân thiết (IsLoyal = true)
        Task<List<CustomerResponse>> GetLoyalCustomersAsync();
    }
}
