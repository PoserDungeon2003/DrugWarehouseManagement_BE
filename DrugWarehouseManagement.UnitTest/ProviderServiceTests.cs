using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using MockQueryable;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace DrugWarehouseManagement.UnitTest
{
    public class ProviderServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IProviderService _providerService;

        public ProviderServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _providerService = new ProviderService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateProviderAsync_ProviderAlreadyExists_ThrowsException()
        {
            // Arrange
            var request = new CreateProviderRequest { PhoneNumber = "1234567890" };
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider> { new Provider { PhoneNumber = "1234567890" } }.AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _providerService.CreateProviderAsync(request));
        }

        [Fact]
        public async Task CreateProviderAsync_CreatesProviderSuccessfully()
        {
            // Arrange
            var request = new CreateProviderRequest { PhoneNumber = "1234567890", ProviderName = "Provider1" };
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider>().AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.CreateAsync(It.IsAny<Provider>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _providerService.CreateProviderAsync(request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Provider created successfully.", response.Message);
            _unitOfWorkMock.Verify(uow => uow.ProviderRepository.CreateAsync(It.IsAny<Provider>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProviderAsync_ProviderNotFound_ThrowsException()
        {
            // Arrange
            var providerId = 1;
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _providerService.DeleteProviderAsync(providerId));
        }

        [Fact]
        public async Task DeleteProviderAsync_DeletesProviderSuccessfully()
        {
            // Arrange
            var providerId = 1;
            var provider = new Provider { ProviderId = providerId, Status = ProviderStatus.Active };
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider> { provider }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.UpdateAsync(It.IsAny<Provider>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _providerService.DeleteProviderAsync(providerId);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Provider deleted successfully.", response.Message);
            Assert.Equal(ProviderStatus.Deleted, provider.Status);
            _unitOfWorkMock.Verify(uow => uow.ProviderRepository.UpdateAsync(It.IsAny<Provider>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetProviderByIdAsync_ProviderNotFound_ThrowsException()
        {
            // Arrange
            var providerId = 1;
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetByWhere(It.IsAny<Expression<Func<Provider, bool>>>()))
                .Returns(new List<Provider>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _providerService.GetProviderByIdAsync(providerId));
        }

        [Fact]
        public async Task GetProviderByIdAsync_ReturnsProviderSuccessfully()
        {
            // Arrange
            var providerId = 1;
            var provider = new Provider { ProviderId = providerId, ProviderName = "Provider1" };
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetByWhere(It.IsAny<Expression<Func<Provider, bool>>>()))
                .Returns(new List<Provider> { provider }.AsQueryable().BuildMock());

            // Act
            var response = await _providerService.GetProviderByIdAsync(providerId);

            // Assert
            Assert.Equal(providerId, response.ProviderId);
            Assert.Equal("Provider1", response.ProviderName);
        }

        [Fact]
        public async Task SearchProvidersAsync_ReturnsPaginatedProviders()
        {
            // Arrange
            var providers = new List<Provider>
            {
                new Provider { ProviderId = 1, ProviderName = "Provider1", Status = ProviderStatus.Active }
            };
            var queryPaging = new QueryPaging { Page = 1, PageSize = 10 };

            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetByWhere(It.IsAny<Expression<Func<Provider, bool>>>()))
                .Returns(providers.AsQueryable().BuildMock());

            // Act
            var response = await _providerService.SearchProvidersAsync(queryPaging);

            // Assert
            Assert.Single(response.Items);
            Assert.Equal(1, response.Items.First().ProviderId);
        }

        [Fact]
        public async Task UpdateProviderAsync_ProviderNotFound_ThrowsException()
        {
            // Arrange
            var providerId = 1;
            var request = new UpdateProviderRequest { ProviderName = "UpdatedProvider" };
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _providerService.UpdateProviderAsync(providerId, request));
        }

        [Fact]
        public async Task UpdateProviderAsync_UpdatesProviderSuccessfully()
        {
            // Arrange
            var providerId = 1;
            var provider = new Provider { ProviderId = providerId, Status = ProviderStatus.Active };
            var request = new UpdateProviderRequest { ProviderName = "UpdatedProvider" };

            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.GetAll())
                .Returns(new List<Provider> { provider }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.ProviderRepository.UpdateAsync(It.IsAny<Provider>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _providerService.UpdateProviderAsync(providerId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Provider updated successfully.", response.Message);
            _unitOfWorkMock.Verify(uow => uow.ProviderRepository.UpdateAsync(It.IsAny<Provider>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}
