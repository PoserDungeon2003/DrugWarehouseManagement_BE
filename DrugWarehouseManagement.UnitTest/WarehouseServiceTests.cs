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

namespace DrugWarehouseManagement.UnitTest
{
    public class WarehouseServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IWarehouseService _warehouseService;

        public WarehouseServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _warehouseService = new WarehouseService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateWarehouseAsync_CreatesWarehouseSuccessfully()
        {
            // Arrange
            var request = new CreateWarehouseRequest { WarehouseName = "Warehouse1", Address = "Address1" };
            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.CreateAsync(It.IsAny<Warehouse>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _warehouseService.CreateWarehouseAsync(request);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.WarehouseRepository.CreateAsync(It.IsAny<Warehouse>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchWarehousesAsync_ReturnsPaginatedWarehouses()
        {
            // Arrange
            var warehouses = new List<Warehouse>
            {
                new Warehouse { WarehouseId = 1, WarehouseName = "Warehouse1", Address = "Address1", Status = WarehouseStatus.Active }
            };
            var queryPaging = new SearchWarehouseRequest { Page = 1, PageSize = 10 };

            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.GetAll())
                .Returns(warehouses.AsQueryable().BuildMock());

            // Act
            var response = await _warehouseService.SearchWarehousesAsync(queryPaging);

            // Assert
            Assert.Single(response.Items);
            Assert.Equal(1, response.Items.First().WarehouseId);
        }

        [Fact]
        public async Task UpdateWarehouseAsync_WarehouseNotFound_ThrowsException()
        {
            // Arrange
            var warehouseId = 1;
            var request = new UpdateWarehouseRequest { WarehouseName = "UpdatedWarehouse", Address = "UpdatedAddress" };

            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.GetAll())
                .Returns(new List<Warehouse>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _warehouseService.UpdateWarehouseAsync(warehouseId, request));
        }

        [Fact]
        public async Task UpdateWarehouseAsync_UpdatesWarehouseSuccessfully()
        {
            // Arrange
            var warehouseId = 1;
            var warehouse = new Warehouse { WarehouseId = warehouseId, Status = WarehouseStatus.Active };
            var request = new UpdateWarehouseRequest { WarehouseName = "UpdatedWarehouse", Address = "UpdatedAddress" };

            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.GetAll())
                .Returns(new List<Warehouse> { warehouse }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.UpdateAsync(It.IsAny<Warehouse>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _warehouseService.UpdateWarehouseAsync(warehouseId, request);

            // Assert
            _unitOfWorkMock.Verify(uow => uow.WarehouseRepository.UpdateAsync(It.IsAny<Warehouse>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteWarehouseAsync_WarehouseNotFound_ThrowsException()
        {
            // Arrange
            var warehouseId = 1;

            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.GetAll())
                .Returns(new List<Warehouse>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _warehouseService.DeleteWarehouseAsync(warehouseId));
        }

        [Fact]
        public async Task DeleteWarehouseAsync_DeletesWarehouseSuccessfully()
        {
            // Arrange
            var warehouseId = 1;
            var warehouse = new Warehouse { WarehouseId = warehouseId, Status = WarehouseStatus.Active };

            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.GetAll())
                .Returns(new List<Warehouse> { warehouse }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(uow => uow.WarehouseRepository.UpdateAsync(It.IsAny<Warehouse>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _warehouseService.DeleteWarehouseAsync(warehouseId);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Warehouse deleted successfully.", response.Message);
            Assert.Equal(WarehouseStatus.Inactive, warehouse.Status);
            _unitOfWorkMock.Verify(uow => uow.WarehouseRepository.UpdateAsync(It.IsAny<Warehouse>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }
}
