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

namespace DrugWarehouseManagement.UnitTest
{
    public class InboundServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<InboundService>> _loggerMock;
        private readonly InboundService _inboundService;
        private readonly Instant _instant;
        private readonly InstantPattern _pattern;

        public InboundServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<InboundService>>();
            _inboundService = new InboundService(_unitOfWorkMock.Object);
            _instant = SystemClock.Instance.GetCurrentInstant();
            _pattern = InstantPattern.ExtendedIso;

        }

        [Fact]
        public async Task CreateInbound_AccountNotFound_ReturnsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateInboundRequest();
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act
            var response = await _inboundService.CreateInbound(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Account not found", response.Message);
        }

        [Fact]
        public async Task CreateInbound_ValidRequest_CreatesSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateInboundRequest
            {
                ProviderOrderCode = "POC123",
                Note = "Test note",
                ProviderId = 1, // Required, must be > 0
                WarehouseId = 1,
                InboundRequestId = null, // Optional
                InboundDetails = new List<InboundDetailRequest>
                {
                    new InboundDetailRequest
                    {
                        LotNumber = "LOT123", // Required
                        ProductId = 1, // Required, must be > 0
                        ManufacturingDate = DateOnly.FromDateTime(new DateTime(2025, 1, 1)),
                        ExpiryDate = DateOnly.FromDateTime(new DateTime(2026, 1, 1)),
                        Quantity = 10, // Required, must be >= 1
                        UnitPrice = 100.50m, // Required, must be > 0
                        TotalPrice = 1005.00m // Required, must be > 0
                    }
                }
            };
            var account = new Account { Id = accountId };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _inboundService.CreateInbound(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Inbound record created successfully", response.Message);

            // Verify the Inbound object passed to CreateAsync
            string expectedDateDigits = _instant.ToString("MMdd", null); // "0415" for 2025-04-15
            _unitOfWorkMock.Verify(uow => uow.InboundRepository.CreateAsync(It.Is<Inbound>(i => i.ProviderOrderCode == "POC123" && i.InboundDetails.Count == 1)), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task UpdateInboundStatus_AccountNotFound_ReturnsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundStatusRequest { InboundId = 1 };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act
            var response = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Account not found", response.Message);
        }

        [Fact]
        public async Task UpdateInboundStatus_InboundNotFound_ReturnsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundStatusRequest { InboundId = 1 };
            var account = new Account { Id = accountId };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync((Inbound)null);

            // Act
            var response = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Inbound not found", response.Message);
        }

        [Fact]
        public async Task UpdateInboundStatus_InvalidStatus_ReturnsInvalidStatus()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundStatusRequest
            {
                InboundId = 1,
                InboundStatus = (InboundStatus)999 // Invalid status
            };
            var account = new Account { Id = accountId };
            var inbound = new Inbound { InboundId = 1 };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // Act
            var response = await _inboundService.UpdateInboundStatus(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Invalid inbound status {Pending, InProgess, Completed, Cancelled}", response.Message);
        }

        //[Fact]
        //public async Task UpdateInboundStatus_StatusToCompleted_UpdatesLotsAndInboundSuccessfully()
        //{
        //    // Arrange
        //    var accountId = Guid.NewGuid();
        //    var request = new UpdateInboundStatusRequest
        //    {
        //        InboundId = 1,
        //        InboundStatus = InboundStatus.Completed
        //    };
        //    var account = new Account { Id = accountId };
        //    var inbound = new Inbound
        //    {
        //        InboundId = 1,
        //        Status = InboundStatus.Pending,
        //        WarehouseId = 1,
        //        ProviderId = 1
        //    };
        //    var InboundDetails = new List<InboundDetails>
        //    {
        //        new InboundDetails
        //        {
        //            InboundId = 1,
        //            LotNumber = "LOT123",
        //            ManufacturingDate = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime()),
        //            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).ToLocalTime()),
        //            ProductId = 1,
        //            Quantity = 10
        //        }
        //    };
        //    var existingLot = new Lot
        //    {
        //        LotNumber = "LOT123",
        //        ManufacturingDate = InboundDetails[0].ManufacturingDate,
        //        ExpiryDate = InboundDetails[0].ExpiryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).ToLocalTime()),
        //        ProductId = 1,
        //        Quantity = 20
        //    };
        //    var allLotsForProduct = new List<Lot> { existingLot };

        //    _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //        .ReturnsAsync(account);
        //    _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
        //        .ReturnsAsync(inbound);
        //    _unitOfWorkMock.Setup(uow => uow.InboundDetailRepository.GetByWhere(It.IsAny<Expression<Func<InboundDetails, bool>>>()))
        //        .Returns(InboundDetails.AsQueryable());
        //    _unitOfWorkMock.Setup(uow => uow.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
        //        .Returns(new List<Lot> { existingLot }.AsQueryable());
        //    _unitOfWorkMock.Setup(uow => uow.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
        //        .Returns(new List<Lot> { existingLot }.AsQueryable());
        //    _unitOfWorkMock.Setup(uow => uow.LotRepository.UpdateAsync(It.IsAny<Lot>()))
        //        .Returns(Task.CompletedTask);
        //    _unitOfWorkMock.Setup(uow => uow.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
        //        .Returns(Task.CompletedTask);
        //    _unitOfWorkMock.Setup(uow => uow.InboundDetailRepository.UpdateAsync(It.IsAny<InboundDetails>()))
        //        .Returns(Task.CompletedTask);
        //    _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    var response = await _inboundService.UpdateInboundStatus(accountId, request);

        //    // Assert
        //    Assert.Equal(200, response.Code);
        //    Assert.Equal("Inbound updated status successfully", response.Message);
        //    Assert.Equal(InboundStatus.Completed, inbound.Status);
        //    Assert.Equal(30, InboundDetails[0].OpeningStock); // 20 (existing) + 10 (new)
        //    _unitOfWorkMock.Verify(uow => uow.LotRepository.UpdateAsync(It.Is<Lot>(l => l.Quantity == 30)), Times.Once());
        //    _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
        //}

    //    [Fact]
    //    public async Task UpdateInboundStatus_StatusToCancelled_RemovesLotsAndUpdatesSuccessfully()
    //    {
    //        // Arrange
    //        var accountId = Guid.NewGuid();
    //        var request = new UpdateInboundStatusRequest
    //        {
    //            InboundId = 1,
    //            InboundStatus = InboundStatus.Cancelled
    //        };
    //        var account = new Account { Id = accountId };
    //        var inbound = new Inbound
    //        {
    //            InboundId = 1,
    //            Status = InboundStatus.Completed,
    //            WarehouseId = 1,
    //            ProviderId = 1
    //        };
    //        var inboundDetails = new List<InboundDetails>
    //{
    //    new InboundDetails
    //    {
    //        InboundId = 1,
    //        LotNumber = "LOT123",
    //        ManufacturingDate = DateOnly.FromDateTime(DateTime.UtcNow.ToLocalTime()),
    //        ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).ToLocalTime()),
    //        ProductId = 1,
    //        Quantity = 10
    //    }
    //};
    //        var existingLot = new Lot
    //        {
    //            LotNumber = "LOT123",
    //            ManufacturingDate = inboundDetails[0].ManufacturingDate,
    //            ExpiryDate = inboundDetails[0].ExpiryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1).ToLocalTime()),
    //            ProductId = 1,
    //            Quantity = 10 // Matches the quantity in inboundDetails
    //        };

    //        _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
    //            .ReturnsAsync(account);
    //        _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
    //            .ReturnsAsync(inbound);
    //        _unitOfWorkMock.Setup(uow => uow.InboundDetailRepository.GetAllByInboundIdAsync(request.InboundId))
    //            .ReturnsAsync(inboundDetails);
    //        _unitOfWorkMock.Setup(uow => uow.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
    //            .Returns(new List<Lot> { existingLot }.AsQueryable());
    //        _unitOfWorkMock.Setup(uow => uow.LotRepository.DeleteAsync(It.IsAny<Lot>()))
    //            .Callback<Lot>(lot => lot.Quantity = 0) // Simulate quantity reduction
    //            .Returns(Task.CompletedTask);
    //        _unitOfWorkMock.Setup(uow => uow.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
    //            .Returns(Task.CompletedTask);
    //        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
    //            .Returns(Task.CompletedTask);

    //        // Act
    //        var response = await _inboundService.UpdateInboundStatus(accountId, request);

    //        // Assert
    //        Assert.Equal(200, response.Code);
    //        Assert.Equal("Inbound updated status successfully", response.Message);
    //        Assert.Equal(InboundStatus.Cancelled, inbound.Status);

    //        // Verify that DeleteAsync was called for the lot with Quantity = 0
    //        _unitOfWorkMock.Verify(uow => uow.LotRepository.DeleteAsync(It.Is<Lot>(l => l.LotNumber == "LOT123" && l.Quantity == 0)), Times.Once());
    //        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
    //    }



        [Fact]
        public async Task UpdateInbound_AccountNotFound_ReturnsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundRequest { InboundId = 1 };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync((Account)null);

            // Act
            var response = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Account not found", response.Message);
        }

        [Fact]
        public async Task UpdateInbound_InboundNotFound_ReturnsNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundRequest { InboundId = 1 };
            var account = new Account { Id = accountId };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync((Inbound)null);

            // Act
            var response = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(404, response.Code);
            Assert.Equal("Inbound not found", response.Message);
        }

        [Fact]
        public async Task UpdateInbound_CompletedStatus_ReturnsCannotUpdate()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundRequest { InboundId = 1 };
            var account = new Account { Id = accountId };
            var inbound = new Inbound { InboundId = 1, Status = InboundStatus.Completed };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);

            // Act
            var response = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Inbound is completed and can't be update", response.Message);
        }

        [Fact]
        public async Task UpdateInbound_ValidRequest_UpdatesSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundRequest
            {
                InboundId = 1,
                InboundDetails = new List<InboundDetailRequest>
                {
                    new InboundDetailRequest { ProductId = 1, Quantity = 5 }
                }
            };
            var account = new Account { Id = accountId };
            var inbound = new Inbound { InboundId = 1, Status = InboundStatus.Pending };
            var existingDetails = new List<InboundDetails>
            {
                new InboundDetails { InboundId = 1, InboundDetailsId = 1 }
            };

            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inbound);
            _unitOfWorkMock.Setup(uow => uow.InboundDetailRepository.GetByWhere(It.IsAny<Expression<Func<InboundDetails, bool>>>()))
                .Returns(existingDetails.AsQueryable());
            _unitOfWorkMock.Setup(uow => uow.InboundDetailRepository.DeleteAsync(It.IsAny<InboundDetails>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.InboundRepository.UpdateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _inboundService.UpdateInbound(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Inbound updated successfully", response.Message);
            _unitOfWorkMock.Verify(uow => uow.InboundDetailRepository.DeleteAsync(It.IsAny<InboundDetails>()), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.InboundRepository.UpdateAsync(It.Is<Inbound>(i => i.AccountId == accountId)), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
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
