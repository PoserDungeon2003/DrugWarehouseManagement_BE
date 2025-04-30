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
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Không tìm thấy tài khoản", exception.Message);
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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync((Warehouse)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Không tìm thấy kho hàng", exception.Message);
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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Kho hàng không hoạt động", exception.Message);
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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync((Lot)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Không tìm thấy lô hàng", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_ZeroQuantity_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, WarehouseId = 1 };

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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Số lượng không hợp lệ", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_InsufficientQuantity_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, WarehouseId = 1, Quantity = 5 };

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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Số lượng lô hàng không đủ để chuyển", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_TransferToSameWarehouse_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var warehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var lot = new Lot { LotId = 1, WarehouseId = 1, Quantity = 20 };

      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 1, // Same as source
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 }
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(warehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(lot);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _lotTransferService.CreateLotTransfer(accountId, request)
      );
      Assert.Equal("Không thể chuyển lô hàng đến kho hiện tại", exception.Message);
    }

    [Fact]
    public async Task CreateLotTransfer_UpdateExistingLotInDestination_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };

      var sourceLot = new Lot
      {
        LotId = 1,
        WarehouseId = 1,
        Quantity = 20,
        LotNumber = "LOT001",
        ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
        ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
        ProductId = 101
      };

      var destLot = new Lot
      {
        LotId = 2,
        WarehouseId = 2,
        Quantity = 5,
        LotNumber = "LOT001", // Same as source lot
        ExpiryDate = sourceLot.ExpiryDate,
        ManufacturingDate = sourceLot.ManufacturingDate,
        ProductId = 101
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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // Setup matching lot in destination warehouse
      var destLots = new List<Lot> { destLot }.AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(destLots);

      _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _lotTransferService.CreateLotTransfer(accountId, request);

      // Assert
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Tạo phiếu chuyển kho thành công", result.Message);

      // Verify source lot quantity was reduced
      Assert.Equal(10, sourceLot.Quantity);

      // Verify destination lot quantity was increased
      Assert.Equal(15, destLot.Quantity);

      // Verify no new lot was created
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()), Times.Never);

      // Verify both lots were updated and transfer was created
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(sourceLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(destLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLotTransfer_CreateNewLotInDestination_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };

      var sourceLot = new Lot
      {
        LotId = 1,
        WarehouseId = 1,
        Quantity = 20,
        LotNumber = "LOT001",
        ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
        ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
        ProductId = 101,
        ProviderId = 201
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
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // Setup no matching lot in destination warehouse
      var emptyLots = new List<Lot>().AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(emptyLots);

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
      Assert.Equal((int)HttpStatusCode.OK, result.Code);
      Assert.Equal("Tạo phiếu chuyển kho thành công", result.Message);

      // Verify source lot quantity was reduced
      Assert.Equal(10, sourceLot.Quantity);

      // Verify a new lot was created with correct properties
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

      // Verify repository calls
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(sourceLot), Times.Once);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(It.IsAny<LotTransfer>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateLotTransfer_GroupDuplicateLotEntries_Success()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var account = new Account { Id = accountId };
      var fromWarehouse = new Warehouse { WarehouseId = 1, Status = WarehouseStatus.Active };
      var toWarehouse = new Warehouse { WarehouseId = 2, Status = WarehouseStatus.Active };

      var sourceLot = new Lot
      {
        LotId = 1,
        WarehouseId = 1,
        Quantity = 50,
        LotNumber = "LOT001",
        ProductId = 101
      };

      var request = new LotTransferRequest
      {
        FromWareHouseId = 1,
        ToWareHouseId = 2,
        LotTransferDetails = new List<LotTransferDetailRequest>
                {
                    new LotTransferDetailRequest { LotId = 1, Quantity = 10 },
                    new LotTransferDetailRequest { LotId = 1, Quantity = 15 } // Duplicate lot entry
                }
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(1))
          .ReturnsAsync(fromWarehouse);
      _unitOfWorkMock.Setup(u => u.WarehouseRepository.GetByIdAsync(2))
          .ReturnsAsync(toWarehouse);
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(1))
          .ReturnsAsync(sourceLot);

      // Setup no matching lot in destination warehouse
      var emptyLots = new List<Lot>().AsQueryable().BuildMock();
      _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
          .Returns(emptyLots);

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
      Assert.Equal((int)HttpStatusCode.OK, result.Code);

      // Verify source lot quantity was reduced by combined amount (10+15=25)
      Assert.Equal(25, sourceLot.Quantity);

      // Verify a new lot was created with combined quantity
      _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(
          It.Is<Lot>(l => l.Quantity == 25)
      ), Times.Once);

      // Verify transfer was created with one combined detail
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.CreateAsync(
          It.Is<LotTransfer>(lt =>
              lt.LotTransferDetails.Count == 1 &&
              lt.LotTransferDetails[0].Quantity == 25)
      ), Times.Once);
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
      Assert.Equal("Không tìm thấy tài khoản", exception.Message);
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
      Assert.Equal("Không tìm thấy phiếu chuyển kho", exception.Message);
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
      Assert.Equal("Phiếu chuyển kho đã bị huỷ trước đó", exception.Message);
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
      Assert.Equal("Không thể huỷ phiếu chuyển kho đã được duyệt", exception.Message);
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
      Assert.Equal("Cập nhật phiếu chuyển kho thành công", result.Message);

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
      Assert.Equal("Cập nhật phiếu chuyển kho thành công", result.Message);

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
      Assert.Equal("Cập nhật phiếu chuyển kho thành công", result.Message);

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
      Assert.Equal("Cập nhật phiếu chuyển kho thành công", result.Message);

      // Verify status was updated
      Assert.Equal(LotTransferStatus.Cancelled, lotTransfer.LotTransferStatus);

      // Verify no lot repository calls were made
      _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()), Times.Never);
      _unitOfWorkMock.Verify(u => u.LotTransferRepository.UpdateAsync(lotTransfer), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
  }
}