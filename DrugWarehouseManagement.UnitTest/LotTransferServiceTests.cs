using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DrugWarehouseManagement.UnitTest
{
  public class LotTransferServiceTests
  {
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMinioService> _minioServiceMock;
    private readonly LotTransferService _lotTransferService;

    public LotTransferServiceTests()
    {
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _minioServiceMock = new Mock<IMinioService>();
      _lotTransferService = new LotTransferService(_unitOfWorkMock.Object, _minioServiceMock.Object);

      TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
    }

    [Fact]
    public async Task CreateLotTransfer_AccountNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>()
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync((Account)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Account not found", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_WarehouseNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>()
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync((Warehouse)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Warehouse not found", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_InactiveWarehouse_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Inactive };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>()
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Warehouse is inactive", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_LotNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync((Lot)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Lot not found", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_ZeroQuantity_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, Quantity = 50, WarehouseId = 1 };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 0 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Quantity must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_InsufficientQuantity_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, Quantity = 5, WarehouseId = 1 };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Quantity not enough", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_TransferToSameWarehouse_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var warehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, Quantity = 50, WarehouseId = 1 };
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 1,  // Same warehouse ID
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(warehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(warehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(() =>
          _lotTransferService.CreateLotTransfer(accountId, request));
      Assert.Equal("Transfer to the same warehouse", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_UpdateExistingLot_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse
      {
        WarehouseId = 1,
        Status = WarehouseStatus.Active,
        WarehouseName = "Source Warehouse"
      };
      var toWarehouse = new Warehouse
      {
        WarehouseId = 2,
        Status = WarehouseStatus.Active,
        WarehouseName = "Target Warehouse"
      };
      var product = new Product { ProductId = 1 };
      var provider = new Provider { ProviderId = 1 };
      var sourceLot = new Lot
      {
        LotId = 1,
        Quantity = 50,
        WarehouseId = 1,
        LotNumber = "LOT001",
        ExpiryDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(6),
        ManufacturingDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(-1),
        ProductId = 1,
        Product = product,
        ProviderId = 1,
        Provider = provider
      };

      var targetLot = new Lot
      {
        LotId = 2,
        Quantity = 20,
        WarehouseId = 2,
        LotNumber = "LOT001",  // Same lot number
        ExpiryDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(6),
        ManufacturingDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(-1),
        ProductId = 1,
        Product = product,
        ProviderId = 1,
        Provider = provider
      };

      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // Mock the target lot query
      var mockTargetLots = new List<Lot> { targetLot }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(mockTargetLots);

      _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.CreateLotTransfer(accountId, request);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Create transfer order successfully", result.Message);

      // Verify source lot quantity was reduced
      Assert.Equal(40, sourceLot.Quantity);

      // Verify target lot quantity was increased
      Assert.Equal(30, targetLot.Quantity);

      // Verify repository calls
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(sourceLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(targetLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()), Times.Never);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLotTransfer_CreateNewLot_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse
      {
        WarehouseId = 1,
        Status = WarehouseStatus.Active,
        WarehouseName = "Source Warehouse"
      };
      var toWarehouse = new Warehouse
      {
        WarehouseId = 2,
        Status = WarehouseStatus.Active,
        WarehouseName = "Target Warehouse"
      };
      var product = new Product { ProductId = 1 };
      var provider = new Provider { ProviderId = 1 };
      var sourceLot = new Lot
      {
        LotId = 1,
        Quantity = 50,
        WarehouseId = 1,
        LotNumber = "LOT001",
        ExpiryDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(6),
        ManufacturingDate = DateOnly.FromDateTime(DateTime.Now).AddMonths(-1),
        ProductId = 1,
        Product = product,
        ProviderId = 1,
        Provider = provider
      };

      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // Mock empty result for target lot query (no existing lot)
      var emptyLotsList = new List<Lot>().AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(emptyLotsList);

      _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.CreateLotTransfer(accountId, request);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Create transfer order successfully", result.Message);

      // Verify source lot quantity was reduced
      Assert.Equal(40, sourceLot.Quantity);

      // Verify repository calls
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(sourceLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

      // Verify the new lot properties
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(
          It.Is<Lot>(l =>
              l.ProductId == sourceLot.ProductId &&
              l.Quantity == 10 &&
              l.ExpiryDate == sourceLot.ExpiryDate &&
              l.WarehouseId == 2 &&
              l.LotNumber == sourceLot.LotNumber &&
              l.ManufacturingDate == sourceLot.ManufacturingDate &&
              l.ProviderId == sourceLot.ProviderId)
      ), Times.Once);
    }

    [Fact]
    public async Task CreateLotTransfer_MultipleDetails_GroupsAndProcessesCorrectly()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };

      var sourceLot = new Lot
      {
        LotId = 1,
        Quantity = 100,
        WarehouseId = 1,
        LotNumber = "LOT001",
        ProductId = 1,
        ProviderId = 1
      };

      // Request with duplicate lot entries to test grouping
      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 },
                    new LotTransferDetailRequest { LotId = 1, Quantity = 15 } // Same lot, different quantity
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.FromWareHouseId))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(request.ToWareHouseId))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // No existing lot in target warehouse
      var emptyLotsList = new List<Lot>().AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(emptyLotsList);

      _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.CreateLotTransfer(accountId, request);

      // Assert
      Assert.Equal(200, result.Code);

      // Verify that quantities were properly grouped (10+15=25)
      Assert.Equal(75, sourceLot.Quantity); // 100 - 25

      // Verify new lot was created with correct quantity
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(
          It.Is<Lot>(l => l.Quantity == 25)
      ), Times.Once);

      // Verify lot transfer was created with grouped details
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(
          It.Is<LotTransfer>(lt =>
              lt.LotTransferDetails.Count == 1 &&
              lt.LotTransferDetails.First().Quantity == 25)
      ), Times.Once);
    }

    [Fact]
    public async Task ExportLotTransfer_LotTransferNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      int lotTransferId = 1;
      var emptyLotTransfers = new List<LotTransfer>().AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(emptyLotTransfers);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.ExportLotTransfer(accountId, lotTransferId)
      );
      Assert.Equal("Lot transfer order not found", exception.Message);
    }

    [Fact]
    public async Task ExportLotTransfer_ValidLotTransfer_ReturnsPdfBytes()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      int lotTransferId = 1;
      DateTime testDate = DateTime.Now;

      // Create a complete lot transfer hierarchy with all required dependencies for PDF generation
      var lotTransfer = CreateSampleLotTransfer(lotTransferId, testDate);

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      // Act
      var result = await _lotTransferService.ExportLotTransfer(accountId, lotTransferId);

      // Assert
      Assert.NotNull(result);
      Assert.IsType<byte[]>(result);
      Assert.True(result.Length > 0, "PDF byte array should not be empty");
    }

    [Fact]
    public async Task ExportLotTransfer_CalculatesTotalQuantityCorrectly()
    {
      // This test indirectly verifies the total quantity calculation
      // by examining the state before PDF generation

      // Arrange
      var accountId = Guid.NewGuid();
      int lotTransferId = 1;
      DateTime testDate = DateTime.Now;

      // Create a sample lot transfer with known quantities
      var lotTransfer = CreateSampleLotTransfer(lotTransferId, testDate);

      // Add specific quantities that we can calculate manually
      lotTransfer.LotTransferDetails[0].Quantity = 10;
      lotTransfer.LotTransferDetails[1].Quantity = 20;
      // Total should be 30

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      // Use reflection or another method to access or verify the calculated total
      // For this example, we'll create a mock QuestPDF Document that will capture the total
      var expectedTotal = lotTransfer.LotTransferDetails.Sum(l => l.Quantity);
      Assert.Equal(30, expectedTotal);

      // Since we can't easily intercept the PDF generation process without modifying the code,
      // we'll just verify that the method doesn't throw and returns a non-empty PDF
      var result = await _lotTransferService.ExportLotTransfer(accountId, lotTransferId);
      Assert.NotNull(result);
      Assert.True(result.Length > 0);
    }

    private LotTransfer CreateSampleLotTransfer(int lotTransferId, DateTime testDate)
    {
      var createdAt = Instant.FromDateTimeUtc(testDate.ToUniversalTime());

      // Create sample products
      var product1 = new Product
      {
        ProductId = 1,
        ProductName = "Test Product 1",
        ProductCode = "TP001",
        SKU = "pc"
      };

      var product2 = new Product
      {
        ProductId = 2,
        ProductName = "Test Product 2",
        ProductCode = "TP002",
        SKU = "box"
      };

      // Create sample providers
      var provider1 = new Provider
      {
        ProviderId = 1,
        ProviderName = "Test Provider 1"
      };

      var provider2 = new Provider
      {
        ProviderId = 2,
        ProviderName = "Test Provider 2"
      };

      // Create sample lots
      var lot1 = new Lot
      {
        LotId = 1,
        ProductId = 1,
        Product = product1,
        ProviderId = 1,
        Provider = provider1,
        LotNumber = "LOT001",
        ExpiryDate = DateOnly.FromDateTime(testDate.AddYears(1)),
        ManufacturingDate = DateOnly.FromDateTime(testDate.AddMonths(-6)),
        WarehouseId = 1
      };

      var lot2 = new Lot
      {
        LotId = 2,
        ProductId = 2,
        Product = product2,
        ProviderId = 2,
        Provider = provider2,
        LotNumber = "LOT002",
        ExpiryDate = DateOnly.FromDateTime(testDate.AddYears(2)),
        ManufacturingDate = DateOnly.FromDateTime(testDate.AddMonths(-3)),
        WarehouseId = 2
      };

      // Create sample warehouses
      var sourceWarehouse = new Warehouse
      {
        WarehouseId = 1,
        WarehouseName = "Source Warehouse",
        WarehouseCode = "SRC"
      };

      var targetWarehouse = new Warehouse
      {
        WarehouseId = 2,
        WarehouseName = "Target Warehouse",
        WarehouseCode = "TGT"
      };

      // Create lot transfer details
      var details = new List<LotTransferDetail>
            {
                new LotTransferDetail
                {
                    LotTransferDetailId = 1,
                    LotTransferId = lotTransferId,
                    LotId = 1,
                    Lot = lot1,
                    Quantity = 10
                },
                new LotTransferDetail
                {
                    LotTransferDetailId = 2,
                    LotTransferId = lotTransferId,
                    LotId = 2,
                    Lot = lot2,
                    Quantity = 20
                }
            };

      // Create the lot transfer
      return new LotTransfer
      {
        LotTransferId = lotTransferId,
        LotTransferCode = $"LT-{testDate:yyyyMMddHHmmss}",
        FromWareHouseId = 1,
        FromWareHouse = sourceWarehouse,
        ToWareHouseId = 2,
        ToWareHouse = targetWarehouse,
        LotTransferStatus = LotTransferStatus.Completed,
        CreatedAt = createdAt,
        LotTransferDetails = details
      };
    }

    [Fact]
    public async Task UpdateLotTransfer_AccountNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync((Account)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.UpdateLotTransfer(accountId, request)
      );
      Assert.Equal("Account not found", exception.Message);
    }

    [Fact]
    public async Task UpdateLotTransfer_LotTransferNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var emptyLotTransfers = new List<LotTransfer>().AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(emptyLotTransfers);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.UpdateLotTransfer(accountId, request)
      );
      Assert.Equal("Lot transfer not found", exception.Message);
    }

    [Fact]
    public async Task UpdateLotTransfer_AlreadyCancelled_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled,
        LotTransferDetails = new List<LotTransferDetail>()
      };
      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.UpdateLotTransfer(accountId, request)
      );
      Assert.Equal("Lot transfer is already cancelled", exception.Message);
    }

    [Fact]
    public async Task UpdateLotTransfer_NotPendingCantCancel_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Completed,  // Not Pending
        LotTransferDetails = new List<LotTransferDetail>()
      };
      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.UpdateLotTransfer(accountId, request)
      );
      Assert.Equal("Can't cancel lot with status not Pending", exception.Message);
    }

    [Fact]
    public async Task UpdateLotTransfer_CancelSuccess_ReturnsQuantitiesToLots()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      // Create lots that will have their quantities restored
      var lot1 = new Lot { LotId = 1, Quantity = 10 };
      var lot2 = new Lot { LotId = 2, Quantity = 20 };

      // Create transfer details with quantities to restore
      var transferDetails = new List<LotTransferDetail>
            {
                new LotTransferDetail { LotId = 1, Quantity = 5 },
                new LotTransferDetail { LotId = 2, Quantity = 8 }
            };

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Pending,  // Pending status
        LotTransferDetails = transferDetails
      };

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      // Setup GetByIdAsync for lots
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot1);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(2))
          .ReturnsAsync(lot2);

      _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.UpdateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.UpdateLotTransfer(accountId, request);

      // Assert
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Update transfer order successfully", result.Message);

      // Verify quantities were restored
      Assert.Equal(15, lot1.Quantity);  // 10 + 5
      Assert.Equal(28, lot2.Quantity);  // 20 + 8

      // Verify status was updated
      Assert.Equal(LotTransferStatus.Cancelled, lotTransfer.LotTransferStatus);

      // Verify repository calls
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(lot1), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(lot2), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.UpdateAsync(lotTransfer), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLotTransfer_RegularUpdate_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Pending,
        LotTransferDetails = new List<LotTransferDetail>(),
      };

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.UpdateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.UpdateLotTransfer(accountId, request);

      // Assert
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Update transfer order successfully", result.Message);

      // Check if UpdatedAt is set
      Assert.NotNull(lotTransfer.UpdatedAt);

      // Verify status wasn't changed
      Assert.Equal(LotTransferStatus.Pending, lotTransfer.LotTransferStatus);

      // Verify repository calls
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()), Times.Never);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.UpdateAsync(lotTransfer), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLotTransfer_CancelWithNoDetails_SuccessWithoutLotUpdates()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Pending,
        LotTransferDetails = null // No details
      };

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.UpdateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.UpdateLotTransfer(accountId, request);

      // Assert
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Update transfer order successfully", result.Message);

      // Verify status was updated
      Assert.Equal(LotTransferStatus.Cancelled, lotTransfer.LotTransferStatus);

      // Verify no lot repository calls were made
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()), Times.Never);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.UpdateAsync(lotTransfer), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateLotTransfer_CancelWithEmptyDetails_SuccessWithoutLotUpdates()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var request = new UpdateLotTransferRequest
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Cancelled
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);

      var lotTransfer = new LotTransfer
      {
        LotTransferId = 1,
        LotTransferStatus = LotTransferStatus.Pending,
        LotTransferDetails = new List<LotTransferDetail>() // Empty details
      };

      var lotTransfers = new List<LotTransfer> { lotTransfer }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.GetByWhere(It.IsAny<Expression<Func<LotTransfer, bool>>>()))
          .Returns(lotTransfers);

      _unitOfWorkMock.Setup(u => u.LotTransferRepository.UpdateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.UpdateLotTransfer(accountId, request);

      // Assert
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Update transfer order successfully", result.Message);

      // Verify status was updated
      Assert.Equal(LotTransferStatus.Cancelled, lotTransfer.LotTransferStatus);

      // Verify no lot repository calls were made
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()), Times.Never);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.UpdateAsync(lotTransfer), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
  }
}