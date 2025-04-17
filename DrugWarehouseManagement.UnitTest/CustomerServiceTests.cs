using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Services;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using DrugWarehouseManagement.Service.Interface;

namespace DrugWarehouseManagement.UnitTest
{
    public class CustomerServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ICustomerService _customerService;

        public CustomerServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _customerService = new CustomerService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateCustomerAsync_CustomerWithPhoneNumberExists_ThrowsException()
        {
            // Arrange
            var request = new CreateCustomerRequest { PhoneNumber = "123456789" };
            var existingCustomer = new Customer { PhoneNumber = "123456789" };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository
                .GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                .Returns(new List<Customer> { existingCustomer }.AsQueryable().BuildMock());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _customerService.CreateCustomerAsync(request));
            Assert.Equal("Khách hàng với số điện thoại này đã tồn tại.", exception.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_CustomerWithDocumentNumberExists_ThrowsException()
        {
            // Arrange
            var request = new CreateCustomerRequest { DocumentNumber = "DOC123" };
            var existingCustomer = new Customer { DocumentNumber = "DOC123" };

            _unitOfWorkMock.SetupSequence(uow => uow.CustomerRepository
                .GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                .Returns(new List<Customer>().AsQueryable().BuildMock()) // No phone number conflict
                .Returns(new List<Customer> { existingCustomer }.AsQueryable().BuildMock()); // Document number conflict

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _customerService.CreateCustomerAsync(request));
            Assert.Equal(" Khách hàng với Số chứng từ này đã tồn tại.", exception.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_CreatesCustomerSuccessfully()
        {
            // Arrange
            var request = new CreateCustomerRequest { PhoneNumber = "123456789", DocumentNumber = "DOC123" };

            _unitOfWorkMock.SetupSequence(uow => uow.CustomerRepository
                .GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Customer, bool>>>()))
                .Returns(new List<Customer>().AsQueryable().BuildMock()) // No phone number conflict
                .Returns(new List<Customer>().AsQueryable().BuildMock()); // No document number conflict
            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.CreateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _customerService.CreateCustomerAsync(request);

            // Assert
            Assert.Equal("Tạo khách hàng thành công", response.Message);
            _unitOfWorkMock.Verify(uow => uow.CustomerRepository.CreateAsync(It.IsAny<Customer>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerAsync_DeletesCustomerSuccessfully()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { CustomerId = customerId, Status = CustomerStatus.Active };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer> { customer }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _customerService.DeleteCustomerAsync(customerId);

            // Assert
            Assert.Equal("Xoá khách hàng thành công", response.Message);
            Assert.Equal(CustomerStatus.Inactive, customer.Status);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteCustomerAsync_CustomerNotFound_ThrowsException()
        {
            // Arrange
            var customerId = 1;

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _customerService.DeleteCustomerAsync(customerId));
        }

        [Fact]
        public async Task GetCustomerDetailByIdAsync_ReturnsCustomerDetails()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { CustomerId = customerId, Status = CustomerStatus.Active };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer> { customer }.AsQueryable().BuildMock());

            // Act
            var response = await _customerService.GetCustomerDetailByIdAsync(customerId);

            // Assert
            Assert.Equal(customerId, response.CustomerId);
        }

        [Fact]
        public async Task GetCustomerDetailByIdAsync_CustomerNotFound_ThrowsException()
        {
            // Arrange
            var customerId = 1;

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _customerService.GetCustomerDetailByIdAsync(customerId));
        }

        [Fact]
        public async Task GetLoyalCustomersAsync_ReturnsLoyalCustomers()
        {
            // Arrange
            var loyalCustomers = new List<Customer>
        {
            new Customer { CustomerId = 1, IsLoyal = true, Status = CustomerStatus.Active }
        };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(loyalCustomers.AsQueryable().BuildMock());

            // Act
            var response = await _customerService.GetLoyalCustomersAsync();

            // Assert
            Assert.Single(response);
            Assert.Equal(1, response.First().CustomerId);
        }

        [Fact]
        public async Task SearchCustomersAsync_ReturnsPaginatedCustomers()
        {
            // Arrange
            var customers = new List<Customer>
        {
            new Customer { CustomerId = 1, CustomerName = "John Doe", Status = CustomerStatus.Active }
        };

            var request = new SearchCustomerRequest { Page = 1, PageSize = 10 };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(customers.AsQueryable().BuildMock());

            // Act
            var response = await _customerService.SearchCustomersAsync(request);

            // Assert
            Assert.Single(response.Items);
            Assert.Equal(1, response.Items.First().CustomerId);
        }

        [Fact]
        public async Task UpdateCustomerAsync_UpdatesCustomerSuccessfully()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { CustomerId = customerId, Status = CustomerStatus.Active };
            var request = new UpdateCustomerRequest { CustomerName = "Jane Doe" };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer> { customer }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _customerService.UpdateCustomerAsync(customerId, request);

            // Assert
            Assert.Equal("Jane Doe", customer.CustomerName);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerAsync_CustomerNotFound_ThrowsException()
        {
            // Arrange
            var customerId = 1;
            var request = new UpdateCustomerRequest { CustomerName = "Jane Doe" };

            _unitOfWorkMock.Setup(uow => uow.CustomerRepository.GetAll())
                .Returns(new List<Customer>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _customerService.UpdateCustomerAsync(customerId, request));
        }
    }
}
