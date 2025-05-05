using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Services;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using MockQueryable;
using Moq;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Service.Interface;

namespace DrugWarehouseManagement.UnitTest
{
    public class InboundServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<InboundService>> _loggerMock;
        private readonly InboundService _inboundService;
        private readonly Instant _instant;
        private readonly InstantPattern _pattern;
        private readonly Mock<INotificationService> _notificationServiceMock;

        public InboundServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<InboundService>>();
            _notificationServiceMock = new Mock<INotificationService>();
            _inboundService = new InboundService(_unitOfWorkMock.Object, _notificationServiceMock.Object);
            _instant = SystemClock.Instance.GetCurrentInstant();
            _pattern = InstantPattern.ExtendedIso;

        }

        [Fact]
        public async Task CreateInbound_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateInboundRequest
            {
                ProviderId = 1,
                WarehouseId = 1,
                ProviderOrderCode = "PO12345"
            };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _inboundService.CreateInbound(accountId, request));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task CreateInbound_DuplicateProviderOrderCode_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new CreateInboundRequest
            {
                ProviderId = 1,
                WarehouseId = 1,
                ProviderOrderCode = "PO12345"
            };

            var existingInbound = new Inbound { ProviderOrderCode = "PO12345" };
            var existingInbounds = new List<Inbound> { existingInbound }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Inbound, bool>>>()))
                .Returns(existingInbounds);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _inboundService.CreateInbound(accountId, request));
            Assert.Equal("Mã đơn hàng đã tồn tại trong hệ thống", exception.Message);
        }

        [Fact]
        public async Task CreateInbound_Success_ReturnsCorrectResponse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new CreateInboundRequest
            {
                ProviderId = 1,
                WarehouseId = 1,
                ProviderOrderCode = "PO12345"
            };

            var emptyInbounds = new List<Inbound>().AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Inbound, bool>>>()))
                .Returns(emptyInbounds);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.PushNotificationToRole(It.IsAny<string>(), It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.CreateInbound(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);
            Assert.Equal("Đơn nhập đã được tạo thành công", result.Message);

            // Verify repository and notification calls
            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _notificationServiceMock.Verify(n => n.PushNotificationToRole(
                It.Is<string>(s => s == "Inventory Manager"),
                It.Is<Notification>(note =>
                    note.Title == "Đơn nhập mới" &&
                    note.Type == NotificationType.ByRole &&
                    note.Role == "Inventory Manager")),
                Times.Once);
        }

        [Fact]
        public async Task CreateInbound_SetsCorrectProperties()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new CreateInboundRequest
            {
                ProviderId = 1,
                WarehouseId = 1,
                ProviderOrderCode = "PO12345",
                Note = "Test note"
            };

            var emptyInbounds = new List<Inbound>().AsQueryable().BuildMock();
            Inbound capturedInbound = null;

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Inbound, bool>>>()))
                .Returns(emptyInbounds);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Callback<Inbound>(i => capturedInbound = i)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.PushNotificationToRole(It.IsAny<string>(), It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.CreateInbound(accountId, request);

            // Assert
            Assert.NotNull(capturedInbound);
            Assert.Equal(accountId, capturedInbound.AccountId);
            Assert.Equal(1, capturedInbound.ProviderId);
            Assert.Equal(1, capturedInbound.WarehouseId);
            Assert.Equal("PO12345", capturedInbound.ProviderOrderCode);
            Assert.Equal("Test note", capturedInbound.Note);
            Assert.Equal(InboundStatus.Pending, capturedInbound.Status);
            Assert.NotNull(capturedInbound.InboundCode);
            Assert.True(capturedInbound.InboundCode.StartsWith("IC")); // Verify code format
            Assert.NotNull(capturedInbound.InboundDate);
            Assert.NotNull(capturedInbound.UpdatedAt);
        }

        [Fact]
        public async Task CreateInbound_AdaptsRequestToEntity()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new CreateInboundRequest
            {
                ProviderId = 1,
                WarehouseId = 1,
                ProviderOrderCode = "PO12345",
                InboundDetails = new List<InboundDetailRequest>
                {
                    new InboundDetailRequest
                    {
                        ProductId = 101,
                        Quantity = 10,
                        LotNumber = "LOT101"
                    }
                }
            };

            var emptyInbounds = new List<Inbound>().AsQueryable().BuildMock();
            Inbound capturedInbound = null;

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Inbound, bool>>>()))
                .Returns(emptyInbounds);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Callback<Inbound>(i => capturedInbound = i)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.PushNotificationToRole(It.IsAny<string>(), It.IsAny<Notification>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.CreateInbound(accountId, request);

            // Assert
            Assert.NotNull(capturedInbound);
            Assert.Equal(1, capturedInbound.ProviderId);
            Assert.Equal(1, capturedInbound.WarehouseId);

            // Mapster should have mapped the InboundDetails collection
            // If you're testing with real Mapster (not mocked), you'd verify the details collection here
            // However, this depends on your actual Mapster configuration
        }

        [Fact]
        public async Task UpdateInboundStatus_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = InboundStatus.Completed
            };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _inboundService.UpdateInboundStatus(accountId, request));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateInboundStatus_InboundNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = InboundStatus.Completed
            };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync((Inbound)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _inboundService.UpdateInboundStatus(accountId, request));
            Assert.Equal("Đơn nhập không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateInboundStatus_ToCompleted_WithPendingReport_UpdatesReportAndCreatesLots()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                WarehouseId = 1,
                ProviderId = 1,
                Status = InboundStatus.Pending
            };

            var inboundReport = new InboundReport
            {
                InboundReportId = 1,
                InboundId = 1,
                Status = InboundReportStatus.Pending,
                ReportDate = SystemClock.Instance.GetCurrentInstant()
            };

            var inboundDetails = new List<InboundDetails>
            {
                new InboundDetails
                {
                    InboundDetailsId = 1,
                    InboundId = 1,
                    ProductId = 101,
                    LotNumber = "LOT001",
                    ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                    ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                    Quantity = 10
                }
            };

            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = InboundStatus.Completed
            };

            // Setup mocks
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // Setup pending report
            var pendingReports = new List<InboundReport> { inboundReport }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.InboundReportRepository.GetByWhere(It.IsAny<Expression<Func<InboundReport, bool>>>()))
                .Returns(pendingReports);

            // Setup inbound details
            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId))
                .ReturnsAsync(inboundDetails);

            // No existing lot for this product+lot combination
            var emptyLots = new List<Lot>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(emptyLots);

            // Setup other repository methods
            _unitOfWorkMock.Setup(u => u.InboundReportRepository.UpdateAsync(It.IsAny<InboundReport>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.UpdateAsync(It.IsAny<InboundDetails>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);
            Assert.Equal("Inbound updated status successfully", result.Message);

            // Verify inbound status update
            Assert.Equal(InboundStatus.Completed, inbound.Status);

            // Verify report update
            _unitOfWorkMock.Verify(u => u.InboundReportRepository.UpdateAsync(It.Is<InboundReport>(
                r => r.Status == InboundReportStatus.Completed &&
                     r.ProblemDescription == "Đơn không có hàng lỗi")), Times.Once);

            // Verify lot creation
            _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(It.Is<Lot>(
                l => l.ProductId == 101 &&
                     l.LotNumber == "LOT001" &&
                     l.Quantity == 10 &&
                     l.WarehouseId == inbound.WarehouseId &&
                     l.ProviderId == inbound.ProviderId)), Times.Once);

            // Verify save changes
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdateInboundStatus_ToCompleted_ExistingLot_UpdatesQuantity()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                WarehouseId = 1,
                ProviderId = 1,
                Status = InboundStatus.Pending
            };

            var inboundDetails = new List<InboundDetails>
            {
                new InboundDetails
                {
                    InboundDetailsId = 1,
                    InboundId = 1,
                    ProductId = 101,
                    LotNumber = "LOT001",
                    ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
                    ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
                    Quantity = 10
                }
            };

            var existingLot = new Lot
            {
                LotId = 1,
                ProductId = 101,
                LotNumber = "LOT001",
                ManufacturingDate = inboundDetails[0].ManufacturingDate,
                ExpiryDate = inboundDetails[0].ExpiryDate ?? new DateOnly(2024, 1, 1),
                Quantity = 5,  // Initial quantity
                WarehouseId = 1
            };

            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = InboundStatus.Completed
            };

            // Setup mocks
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // No pending report
            var emptyReports = new List<InboundReport>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.InboundReportRepository.GetByWhere(It.IsAny<Expression<Func<InboundReport, bool>>>()))
                .Returns(emptyReports);

            // Setup inbound details
            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId))
                .ReturnsAsync(inboundDetails);

            // Setup existing lot
            var existingLots = new List<Lot> { existingLot }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(existingLots);

            // Setup other repository methods
            _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.UpdateAsync(It.IsAny<InboundDetails>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);

            // Verify inbound status update
            Assert.Equal(InboundStatus.Completed, inbound.Status);

            // Verify lot quantity update
            Assert.Equal(15, existingLot.Quantity);  // 5 (initial) + 10 (added)

            // Verify repository calls
            _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(existingLot), Times.Once);
            _unitOfWorkMock.Verify(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // [Fact]
        // public async Task UpdateInboundStatus_ToCancelled_FromCompleted_UpdatesLotQuantities()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var account = new Account { Id = accountId };
        //     var inbound = new Inbound
        //     {
        //         InboundId = 1,
        //         WarehouseId = 1,
        //         ProviderId = 1,
        //         Status = InboundStatus.Completed  // Already completed
        //     };

        //     var inboundDetails = new List<InboundDetails>
        //     {
        //         new InboundDetails
        //         {
        //             InboundDetailsId = 1,
        //             InboundId = 1,
        //             ProductId = 101,
        //             LotNumber = "LOT001",
        //             ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
        //             ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
        //             Quantity = 10
        //         }
        //     };

        //     var existingLot = new Lot
        //     {
        //         LotId = 1,
        //         ProductId = 101,
        //         LotNumber = "LOT001",
        //         ManufacturingDate = inboundDetails[0].ManufacturingDate,
        //         ExpiryDate = inboundDetails[0].ExpiryDate ?? new DateOnly(2024, 1, 1),
        //         Quantity = 15,  // Current quantity
        //         WarehouseId = 1
        //     };

        //     var request = new UpdateInboundStatusRequest
        //     {
        //         InboundId = 1,
        //         InboundStatus = InboundStatus.Cancelled
        //     };

        //     // Setup mocks
        //     _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(account);
        //     _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
        //         .ReturnsAsync(inbound);

        //     // Setup inbound details
        //     _unitOfWorkMock.Setup(u => u.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId))
        //         .ReturnsAsync(inboundDetails);

        //     // Setup existing lot
        //     var existingLots = new List<Lot> { existingLot }.AsQueryable().BuildMock();
        //     _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
        //         .Returns(existingLots);

        //     // Setup other repository methods
        //     _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
        //         .Returns(Task.CompletedTask);
        //     _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
        //         .Returns(Task.CompletedTask);
        //     _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
        //         .Returns(Task.CompletedTask);

        //     // Act
        //     var result = await _inboundService.UpdateInboundStatus(accountId, request);

        //     // Assert
        //     Assert.Equal(200, result.Code);

        //     // Verify inbound status update
        //     Assert.Equal(InboundStatus.Cancelled, inbound.Status);

        //     // Verify repository calls
        //     _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(existingLot), Times.Once);
        //     _unitOfWorkMock.Verify(u => u.LotRepository.DeleteAsync(It.IsAny<Lot>()), Times.Never);
        //     _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        // }

    //     [Fact]
    //     public async Task UpdateInboundStatus_ToCancelled_FromCompleted_DeletesLotWhenQuantityZero()
    //     {
    //         // Arrange
    //         var accountId = Guid.NewGuid();
    //         var account = new Account { Id = accountId };
    //         var inbound = new Inbound
    //         {
    //             InboundId = 1,
    //             WarehouseId = 1,
    //             ProviderId = 1,
    //             Status = InboundStatus.Completed  // Already completed
    //         };

    //         var inboundDetails = new List<InboundDetails>
    // {
    //     new InboundDetails
    //     {
    //         InboundDetailsId = 1,
    //         InboundId = 1,
    //         ProductId = 101,
    //         LotNumber = "LOT001",
    //         ManufacturingDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)),
    //         ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(1)),
    //         Quantity = 10
    //     }
    // };

    //         var existingLot = new Lot
    //         {
    //             LotId = 1,
    //             ProductId = 101,
    //             LotNumber = "LOT001",
    //             ManufacturingDate = inboundDetails[0].ManufacturingDate,
    //             ExpiryDate = inboundDetails[0].ExpiryDate ?? new DateOnly(2024, 1, 1),
    //             Quantity = 10,  // Same as inbound detail quantity - will become zero
    //             WarehouseId = 1
    //         };

    //         var request = new UpdateInboundStatusRequest
    //         {
    //             InboundId = 1,
    //             InboundStatus = InboundStatus.Cancelled
    //         };

    //         // Setup mocks
    //         _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
    //             .ReturnsAsync(account);
    //         _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
    //             .ReturnsAsync(inbound);

    //         // Setup inbound details
    //         _unitOfWorkMock.Setup(u => u.InboundDetailRepository.GetAllByInboundIdAsync(inbound.InboundId))
    //             .ReturnsAsync(inboundDetails);

    //         // Setup existing lot with specific mock callback to track operations
    //         var existingLots = new List<Lot> { existingLot }.AsQueryable().BuildMock();
    //         _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
    //             .Returns(existingLots);

    //         _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
    //             .Returns(Task.CompletedTask);
    //         _unitOfWorkMock.Setup(u => u.LotRepository.DeleteAsync(It.IsAny<Lot>()))
    //             .Returns(Task.CompletedTask);
    //         _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
    //             .Returns(Task.CompletedTask);

    //         // Act
    //         var result = await _inboundService.UpdateInboundStatus(accountId, request);

    //         // Assert
    //         Assert.Equal(200, result.Code);
    //         Assert.Equal(InboundStatus.Cancelled, inbound.Status);

    //         // Verify repository calls for deletion
    //         _unitOfWorkMock.Verify(u => u.LotRepository.DeleteAsync(existingLot), Times.Once);
    //         _unitOfWorkMock.Verify(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()), Times.Never);
    //         _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    //     }
        
        [Fact]
        public async Task UpdateInboundStatus_ToProcessing_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                WarehouseId = 1,
                ProviderId = 1,
                Status = InboundStatus.Pending
            };

            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = InboundStatus.InProgress
            };

            // Setup mocks
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // Setup other repository methods
            _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);

            // Verify inbound status update
            Assert.Equal(InboundStatus.InProgress, inbound.Status);

            // Verify repository calls
            _unitOfWorkMock.Verify(u => u.InboundRepository.UpdateAsync(inbound), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateInbound_AccountNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundRequest { InboundId = 1 };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _inboundService.UpdateInbound(accountId, request));
            Assert.Equal("Tài khoản không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateInbound_InboundNotFound_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var request = new UpdateInboundRequest { InboundId = 1 };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync((Inbound)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _inboundService.UpdateInbound(accountId, request));
            Assert.Equal("Đơn nhập không tồn tại", exception.Message);
        }

        [Fact]
        public async Task UpdateInbound_AlreadyCompleted_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                Status = InboundStatus.Completed
            };
            var request = new UpdateInboundRequest { InboundId = 1 };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _inboundService.UpdateInbound(accountId, request));
            Assert.Equal("Đơn nhập đã hoàn thành và không thể cập nhật", exception.Message);
        }

        [Fact]
        public async Task UpdateInbound_WithoutDetails_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                Status = InboundStatus.Pending,
                WarehouseId = 5,
                ProviderOrderCode = "OLD-CODE"
            };
            var request = new UpdateInboundRequest
            {
                InboundId = 1,
                WarehouseId = 10,
                ProviderOrderCode = "NEW-CODE",
                InboundDetails = null // No details to update
            };

            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);
            _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);
            Assert.Equal("Cập nhật đơn nhập thành công", result.Message);

            // Verify inbound was updated with new values
            Assert.Equal(10, inbound.WarehouseId);
            Assert.Equal("NEW-CODE", inbound.ProviderOrderCode);
            Assert.Equal(accountId, inbound.AccountId);
            Assert.NotNull(inbound.UpdatedAt);

            // Verify repository calls
            _unitOfWorkMock.Verify(u => u.InboundRepository.UpdateAsync(inbound), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);

            // Verify no detail deletion was attempted
            _unitOfWorkMock.Verify(u => u.InboundDetailRepository.DeleteAsync(It.IsAny<InboundDetails>()), Times.Never);
        }

        [Fact]
        public async Task UpdateInbound_WithDetails_DeletesAndCreatesDetails()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId };
            var inbound = new Inbound
            {
                InboundId = 1,
                Status = InboundStatus.Pending
            };

            var request = new UpdateInboundRequest
            {
                InboundId = 1,
                InboundDetails = new List<InboundDetailRequest>
                {
                    new InboundDetailRequest
                    {
                        ProductId = 101,
                        Quantity = 10,
                        LotNumber = "LOT-NEW-001"
                    }
                }
            };

            // Existing details that should be deleted
            var existingDetails = new List<InboundDetails>
            {
                new InboundDetails { InboundDetailsId = 10, InboundId = 1 }
            };

            // Setup repository mocks
            _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);

            _unitOfWorkMock.Setup(u => u.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // THIS IS THE PROBLEMATIC MOCK - Fix it by mocking IQueryable correctly
            var detailsQueryableMock = existingDetails.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.GetByWhere(It.IsAny<Expression<Func<InboundDetails, bool>>>()))
                .Returns(detailsQueryableMock);

            _unitOfWorkMock.Setup(u => u.InboundDetailRepository.DeleteAsync(It.IsAny<InboundDetails>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(200, result.Code);
            Assert.Equal("Cập nhật đơn nhập thành công", result.Message);

            // Verify existing details were deleted
            _unitOfWorkMock.Verify(u => u.InboundDetailRepository.DeleteAsync(existingDetails[0]), Times.Once);

            // Verify inbound was updated and changes saved
            _unitOfWorkMock.Verify(u => u.InboundRepository.UpdateAsync(inbound), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetInboundById_NotFound_ReturnsEmptyViewInbound()
        {
            // Arrange
            var inboundId = 1;
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByWhere(It.IsAny<Expression<Func<Inbound, bool>>>()))
                .Returns(new List<Inbound>().AsQueryable().BuildMock());

            // Act
            var result = await _inboundService.GetInboundById(inboundId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewInbound>(result);
            Assert.Equal(0, result.InboundId); // Default value for int is 0
        }

        [Fact]
        public async Task GetInboundById_Found_ReturnsMappedResult()
        {
            // Arrange
            var inboundId = 1;
            var inboundDate = _instant;
            var inbound = new Inbound
            {
                InboundId = inboundId,
                InboundCode = "IC123",
                InboundDate = inboundDate,
                InboundDetails = new List<InboundDetails>
        {
            new InboundDetails
            {
                Product = new Product { ProductName = "Test Product" }
            }
        },
                Provider = new Provider { ProviderName = "Test Provider" },
                Account = new Account { },
                Warehouse = new Warehouse { }
            };

            var inboundReport = new InboundReport
            {
                InboundId = inboundId,
                ReportDate = SystemClock.Instance.GetCurrentInstant(),
                ProblemDescription = "Test Problem"
            };

            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByWhere(It.IsAny<Expression<Func<Inbound, bool>>>()))
                .Returns(new List<Inbound> { inbound }.AsQueryable().BuildMock());

            _unitOfWorkMock.Setup(uow => uow.InboundReportRepository.GetByWhere(It.IsAny<Expression<Func<InboundReport, bool>>>()))
                .Returns(new List<InboundReport> { inboundReport }.AsQueryable().BuildMock());

            // Act
            var result = await _inboundService.GetInboundById(inboundId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inboundId, result.InboundId);
            Assert.NotNull(result.Report);
            Assert.Equal(inboundReport.ProblemDescription, result.Report.ProblemDescription);
        }


        //    [Fact]
        //    public async Task GetInboundsPaginatedAsync_WithFilters_ReturnsPaginatedResult()
        //    {
        //        // Arrange
        //        var pattern = InstantPattern.ExtendedIso;
        //        var instant = _instant;
        //        var inbounds = new List<Inbound>
        //{
        //    new Inbound
        //    {
        //        InboundId = 1,
        //        InboundCode = "IC001",
        //        Status = InboundStatus.Pending,
        //        InboundDate = instant,
        //        InboundDetails = new List<InboundDetails>
        //        {
        //            new InboundDetails
        //            {
        //                Product = new Product { ProductName = "Test Product" }
        //            }
        //        },
        //        Provider = new Provider { ProviderName = "Test Provider" },
        //        Account = new Account { },
        //        Warehouse = new Warehouse { }
        //    }
        //};

        //        var mockQueryable = inbounds.AsQueryable().BuildMockDbSet();
        //        var inboundRepositoryMock = new Mock<IInboundRepository>();
        //        inboundRepositoryMock.Setup(r => r.GetAll()).Returns(mockQueryable.Object);
        //        _unitOfWorkMock.Setup(u => u.InboundRepository).Returns(inboundRepositoryMock.Object);

        //        var request = new InboundtQueryPaging
        //        {
        //            Page = 1,
        //            PageSize = 1,
        //            Search = "IC001",
        //            InboundStatus = InboundStatus.Pending,
        //            DateFrom = pattern.Format(instant.Minus(Duration.FromDays(1))),
        //            DateTo = pattern.Format(instant.Plus(Duration.FromDays(1)))
        //        };

        //        // Act
        //        var result = await _inboundService.GetInboundsPaginatedAsync(request);

        //        // Assert
        //        Assert.NotNull(result);
        //        Assert.Single(result.Items);
        //        Assert.Equal(1, result.TotalCount);
        //        Assert.Equal(1, result.CurrentPage);
        //        Assert.Equal(1, result.PageSize);

        //        var item = result.Items.First();
        //        Assert.Equal(1, item.InboundId);
        //        Assert.Equal("IC001", item.InboundCode);
        //    }


        [Fact]
        public async Task GenerateInboundPdfAsync_InboundNotFound_ThrowsException()
        {
            // Arrange
            var inboundId = 1;
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByWhere(It.IsAny<Expression<Func<Inbound, bool>>>()))
                .Returns(new List<Inbound>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inboundService.GenerateInboundPdfAsync(inboundId));
        }

        [Fact]
        public async Task GenerateInboundPdfAsync_ValidInbound_GeneratesPdfSuccessfully()
        {
            // Arrange
            var inboundId = 1;
            var inboundDate = _instant;
            var inbound = new Inbound
            {
                InboundId = inboundId,
                InboundCode = "IC123",
                InboundDate = inboundDate,
                InboundDetails = new List<InboundDetails>
                {
                    new InboundDetails
                    {
                        Product = new Product { ProductName = "Test Product" },
                        LotNumber = "LOT123",
                        ExpiryDate = DateOnly.FromDateTime(inboundDate.ToDateTimeUtc().ToLocalTime()),
                        Quantity = 10,
                        UnitPrice = 100
                    }
                },
                Provider = new Provider { ProviderName = "Test Provider", Address = "Test Address", PhoneNumber = "123456789" },
                Warehouse = new Warehouse()
            };

            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByWhere(It.IsAny<Expression<Func<Inbound, bool>>>()))
                .Returns(new List<Inbound> { inbound }.AsQueryable().BuildMock());

            // Act
            var pdfBytes = await _inboundService.GenerateInboundPdfAsync(inboundId);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);
        }
    }
}
