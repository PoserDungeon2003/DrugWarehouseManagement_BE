using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Extenstions;
using DrugWarehouseManagement.Service.Interface;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CustomerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse> CreateCustomerAsync(CreateCustomerRequest request)
        {
            var existedCustomer = await _unitOfWork.CustomerRepository
                .GetByWhere(c => c.PhoneNumber == request.PhoneNumber)
                .FirstOrDefaultAsync();
            if (existedCustomer != null && existedCustomer.PhoneNumber == request.PhoneNumber)
            {
                throw new Exception("Khách hàng với số điện thoại này đã tồn tại.");
            }
            var existedDocumentNumber = await _unitOfWork.CustomerRepository
                .GetByWhere(c => c.DocumentNumber == request.DocumentNumber)
                .FirstOrDefaultAsync();
            if (existedDocumentNumber != null && existedDocumentNumber.DocumentNumber == request.DocumentNumber)
            {
                throw new Exception(" Khách hàng với Số chứng từ này đã tồn tại.");
            }
            var customer = request.Adapt<Customer>();
            await _unitOfWork.CustomerRepository.CreateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse { Message = "Tạo khách hàng thành công" };
        }

        public async Task<BaseResponse> DeleteCustomerAsync(int customerId)
        {
            var customer = await _unitOfWork.CustomerRepository
                .GetAll()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null || customer.Status == CustomerStatus.Inactive)
            {
                throw new Exception("Không tìm thấy khác hàng hoặc đã bị xóa .");
            }
            customer.Status = CustomerStatus.Inactive;
            await _unitOfWork.SaveChangesAsync();
            return new BaseResponse { Message = "Xoá khách hàng thành công" };
        }

        public async Task<CustomerResponse> GetCustomerDetailByIdAsync(int customerId)
        {
            var customer = await _unitOfWork.CustomerRepository
                .GetAll()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null || customer.Status == CustomerStatus.Inactive)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }
            return customer.Adapt<CustomerResponse>();
        }

        public async Task<List<CustomerResponse>> GetLoyalCustomersAsync()
        {
            var loyalCustomers = await _unitOfWork.CustomerRepository
                 .GetAll()
                 .Where(c => c.IsLoyal == true && c.Status == CustomerStatus.Active)
                 .ToListAsync();
            return loyalCustomers.Adapt<List<CustomerResponse>>();
        }

        public async Task<PaginatedResult<CustomerResponse>> SearchCustomersAsync(SearchCustomerRequest request)
        {
            var query = _unitOfWork.CustomerRepository
               .GetAll()
               .Where(c => c.Status == CustomerStatus.Active) // ẩn khách hàng inactive
               .AsQueryable();

            if (!request.ShowInactive)
            {
                query = query.Where(c => c.Status == CustomerStatus.Active);
            }
            
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.Trim().ToLower();

                query = query.Where(c =>
                    c.CustomerName.ToLower().Contains(searchLower) ||
                    c.PhoneNumber.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower)||
                    c.DocumentNumber.ToLower().Contains(searchLower)
                );
            }
            query = query.OrderBy(c => c.CustomerId);
            var paginatedCustomers = await query.ToPaginatedResultAsync(request.Page, request.PageSize);
            var customerResponses = paginatedCustomers.Items.Adapt<List<CustomerResponse>>();
            return new PaginatedResult<CustomerResponse>
            {
                Items = customerResponses,
                TotalCount = paginatedCustomers.TotalCount,
                PageSize = paginatedCustomers.PageSize,
                CurrentPage = paginatedCustomers.CurrentPage
            };
        }

        public async Task UpdateCustomerAsync(int customerId, UpdateCustomerRequest request)
        {
            var customer = await _unitOfWork.CustomerRepository
                 .GetAll()
                 .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null)
            {
                throw new Exception("Không tìm thấy khách hàng.");
            }

            request.Adapt(customer);
           
            await _unitOfWork.CustomerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
