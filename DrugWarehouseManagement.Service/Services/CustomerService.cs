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
                throw new Exception("Customer not found or already inactive.");
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
                throw new Exception("Customer not found or inactive.");
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
            if (!string.IsNullOrEmpty(request.Search))
            {
                var searchLower = request.Search.Trim().ToLower();

                query = query.Where(c =>
                    c.CustomerName.ToLower().Contains(searchLower) ||
                    c.PhoneNumber.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower)
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

            if (customer == null || customer.Status == CustomerStatus.Inactive)
            {
                throw new Exception("Customer not found or inactive.");
            }
            if (!string.IsNullOrEmpty(request.CustomerName))
            {
                customer.CustomerName = request.CustomerName;
            }
            if (!string.IsNullOrEmpty(request.Address))
            {
                customer.Address = request.Address;
            }
            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                customer.PhoneNumber = request.PhoneNumber;
            }
            if (!string.IsNullOrEmpty(request.Email))
            {
                customer.Email = request.Email;
            }
            await _unitOfWork.CustomerRepository.UpdateAsync(customer);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
