using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Services;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MockQueryable;

namespace DrugWarehouseManagement.UnitTest
{
    public class ReturnOutboundServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ReturnOutboundService _returnOutboundService;

        public ReturnOutboundServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _returnOutboundService = new ReturnOutboundService(_unitOfWorkMock.Object);
        }

        #region CreateReturnOutboundAsync Tests

        [Fact]
        public async Task CreateReturnOutboundAsync_Success()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                OutboundCode = "OUT-123",
                Status = OutboundStatus.Completed,
                AccountId = Guid.NewGuid(),
                OutboundDetails = new List<OutboundDetails>
                {
                    new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Quantity = 10,
                        UnitPrice = 100,
                        Lot = new Lot
                        {
                            LotNumber = "LOT-1",
                            ManufacturingDate = new DateOnly(2023, 1, 1),
                            ExpiryDate = new DateOnly(2024, 1, 1),
                            ProductId = 101,
                            ProviderId = 201
                        }
                    }
                }
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var emptyLotQueryable = new List<Lot>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(emptyLotQueryable);

            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.AddRangeAsync(It.IsAny<IEnumerable<ReturnOutboundDetails>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.OutboundRepository.UpdateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>
                {
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 1,
                        Quantity = 5,
                        Note = "Testing return"
                    }
                }
            };

            // Act
            await _returnOutboundService.CreateReturnOutboundAsync(request);

            // Assert
            _unitOfWorkMock.Verify(u => u.ReturnOutboundDetailsRepository.AddRangeAsync(
                It.Is<IEnumerable<ReturnOutboundDetails>>(details =>
                    details.Count() == 1 &&
                    details.First().OutboundDetailsId == 1 &&
                    details.First().ReturnedQuantity == 5)), Times.Once);

            _unitOfWorkMock.Verify(u => u.OutboundRepository.UpdateAsync(
                It.Is<Outbound>(o => o.Status == OutboundStatus.Returned)), Times.Once);

            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(
                It.IsAny<Inbound>()), Times.Once);

            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.AtLeast(1));
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_MultipleReturnDetails_Success()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                OutboundCode = "OUT-123",
                Status = OutboundStatus.Completed,
                AccountId = Guid.NewGuid(),
                OutboundDetails = new List<OutboundDetails>
                {
                    new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Quantity = 10,
                        UnitPrice = 100,
                        Lot = new Lot
                        {
                            LotNumber = "LOT-1",
                            ManufacturingDate = new DateOnly(2023, 1, 1),
                            ExpiryDate = new DateOnly(2024, 1, 1),
                            ProductId = 101,
                            ProviderId = 201
                        }
                    },
                    new OutboundDetails
                    {
                        OutboundDetailsId = 2,
                        OutboundId = 1,
                        Quantity = 15,
                        UnitPrice = 200,
                        Lot = new Lot
                        {
                            LotNumber = "LOT-2",
                            ManufacturingDate = new DateOnly(2023, 2, 1),
                            ExpiryDate = new DateOnly(2024, 2, 1),
                            ProductId = 102,
                            ProviderId = 201
                        }
                    }
                }
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var emptyLotQueryable = new List<Lot>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(emptyLotQueryable);

            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.AddRangeAsync(It.IsAny<IEnumerable<ReturnOutboundDetails>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.OutboundRepository.UpdateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>
                {
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 1,
                        Quantity = 5,
                        Note = "Testing return 1"
                    },
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 2,
                        Quantity = 7,
                        Note = "Testing return 2"
                    }
                }
            };

            // Act
            await _returnOutboundService.CreateReturnOutboundAsync(request);

            // Assert
            _unitOfWorkMock.Verify(u => u.ReturnOutboundDetailsRepository.AddRangeAsync(
                It.Is<IEnumerable<ReturnOutboundDetails>>(details =>
                    details.Count() == 2)), Times.Once);

            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(
                It.Is<Inbound>(i => i.ProviderId == 201)), Times.Once);
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_MultipleProviders_CreatesMultipleInbounds()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                OutboundCode = "OUT-123",
                Status = OutboundStatus.Completed,
                AccountId = Guid.NewGuid(),
                OutboundDetails = new List<OutboundDetails>
                {
                    new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Quantity = 10,
                        UnitPrice = 100,
                        Lot = new Lot
                        {
                            LotNumber = "LOT-1",
                            ManufacturingDate = new DateOnly(2023, 1, 1),
                            ExpiryDate = new DateOnly(2024, 1, 1),
                            ProductId = 101,
                            ProviderId = 201
                        }
                    },
                    new OutboundDetails
                    {
                        OutboundDetailsId = 2,
                        OutboundId = 1,
                        Quantity = 15,
                        UnitPrice = 200,
                        Lot = new Lot
                        {
                            LotNumber = "LOT-2",
                            ManufacturingDate = new DateOnly(2023, 2, 1),
                            ExpiryDate = new DateOnly(2024, 2, 1),
                            ProductId = 102,
                            ProviderId = 202 // Different provider
                        }
                    }
                }
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var emptyLotQueryable = new List<Lot>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(emptyLotQueryable);

            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.AddRangeAsync(It.IsAny<IEnumerable<ReturnOutboundDetails>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.OutboundRepository.UpdateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.LotRepository.CreateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>
                {
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 1,
                        Quantity = 5,
                        Note = "Testing return 1"
                    },
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 2,
                        Quantity = 7,
                        Note = "Testing return 2"
                    }
                }
            };

            // Act
            await _returnOutboundService.CreateReturnOutboundAsync(request);

            // Assert
            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(It.Is<Inbound>(i => i.ProviderId == 201)), Times.Once);
            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(It.Is<Inbound>(i => i.ProviderId == 202)), Times.Once);
            _unitOfWorkMock.Verify(u => u.InboundRepository.CreateAsync(It.IsAny<Inbound>()), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_OutboundNotFound_ThrowsException()
        {
            // Arrange
            var emptyOutboundQueryable = new List<Outbound>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(emptyOutboundQueryable);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 999,
                Details = new List<ReturnOutboundDetailItem>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _returnOutboundService.CreateReturnOutboundAsync(request));
            Assert.Contains("Đơn xuất 999 không tìm thấy.", exception.Message);
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_OutboundNotCompleted_ThrowsException()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                Status = OutboundStatus.InProgress,
                OutboundDetails = new List<OutboundDetails>()
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>()
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _returnOutboundService.CreateReturnOutboundAsync(request));
            Assert.Contains("Chỉ được trả hàng khi đơn xuất ở trạng thái hoàn thành.", exception.Message);
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_InvalidOutboundDetail_ThrowsException()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                Status = OutboundStatus.Completed,
                OutboundDetails = new List<OutboundDetails>
                {
                    new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Quantity = 10
                    }
                }
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>
                {
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 999, // Non-existent detail
                        Quantity = 5
                    }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _returnOutboundService.CreateReturnOutboundAsync(request));
            Assert.Contains("Không tìm thấy chi tiết xuất kho 999 trong phiếu này.", exception.Message);
        }

        [Fact]
        public async Task CreateReturnOutboundAsync_ReturnQuantityExceedsOutbound_ThrowsException()
        {
            // Arrange
            var outbound = new Outbound
            {
                OutboundId = 1,
                Status = OutboundStatus.Completed,
                OutboundDetails = new List<OutboundDetails>
                {
                    new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Quantity = 10,
                        Lot = new Lot()
                    }
                }
            };

            var outboundQueryable = new List<Outbound> { outbound }.AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(outboundQueryable);

            var request = new CreateReturnOutboundRequest
            {
                OutboundId = 1,
                Details = new List<ReturnOutboundDetailItem>
                {
                    new ReturnOutboundDetailItem
                    {
                        OutboundDetailsId = 1,
                        Quantity = 15 // Exceeds available quantity
                    }
                }
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _returnOutboundService.CreateReturnOutboundAsync(request));
            Assert.Contains("Số lượng trả về 15 lớn hơn số lượng xuất kho còn lại 10", exception.Message);
        }

        #endregion

        #region GetReturnOutboundByOutboundIdAsync Tests

        [Fact]
        public async Task GetReturnOutboundByOutboundIdAsync_ReturnsCorrectData()
        {
            // Arrange
            var outboundDetails = new List<OutboundDetails>
            {
                new OutboundDetails
                {
                    OutboundDetailsId = 1,
                    OutboundId = 1,
                    Outbound = new Outbound { OutboundCode = "OUT-123" }
                }
            };
            var outboundDetailsQueryable = outboundDetails.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.OutboundDetailsRepository.GetAll())
                .Returns(outboundDetailsQueryable);

            var returnDetails = new List<ReturnOutboundDetails>
            {
                new ReturnOutboundDetails
                {
                    ReturnOutboundDetailsId = 1,
                    OutboundDetailsId = 1,
                    ReturnedQuantity = 5,
                    Note = "Test return",
                    OutboundDetails = new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Outbound = new Outbound { OutboundCode = "OUT-123" },
                        Lot = new Lot
                        {
                            Product = new Product { ProductCode = "P-101", ProductName = "Test Product" }
                        }
                    }
                }
            };
            var returnDetailsQueryable = returnDetails.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.GetAll())
                .Returns(returnDetailsQueryable);

            // Act
            var result = await _returnOutboundService.GetReturnOutboundByOutboundIdAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal("OUT-123", result[0].OutboundCode);
            Assert.Equal("P-101", result[0].ProductCode);
            Assert.Equal("Test Product", result[0].ProductName);
            Assert.Equal(5, result[0].ReturnedQuantity);
        }

        [Fact]
        public async Task GetReturnOutboundByOutboundIdAsync_ReturnsEmptyList_WhenNoData()
        {
            // Arrange
            var emptyOutboundDetailsQueryable = new List<OutboundDetails>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.OutboundDetailsRepository.GetAll())
                .Returns(emptyOutboundDetailsQueryable);

            var emptyReturnDetailsQueryable = new List<ReturnOutboundDetails>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.GetAll())
                .Returns(emptyReturnDetailsQueryable);

            // Act
            var result = await _returnOutboundService.GetReturnOutboundByOutboundIdAsync(999);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllReturnOutboundDetailsAsync Tests

        [Fact]
        public async Task GetAllReturnOutboundDetailsAsync_ReturnsCorrectData()
        {
            // Arrange
            var returnDetails = new List<ReturnOutboundDetails>
            {
                new ReturnOutboundDetails
                {
                    ReturnOutboundDetailsId = 1,
                    OutboundDetailsId = 1,
                    ReturnedQuantity = 5,
                    Note = "Test return 1",
                    OutboundDetails = new OutboundDetails
                    {
                        OutboundDetailsId = 1,
                        OutboundId = 1,
                        Outbound = new Outbound { OutboundCode = "OUT-123" },
                        Lot = new Lot
                        {
                            Product = new Product { ProductCode = "P-101", ProductName = "Test Product 1" }
                        }
                    }
                },
                new ReturnOutboundDetails
                {
                    ReturnOutboundDetailsId = 2,
                    OutboundDetailsId = 2,
                    ReturnedQuantity = 3,
                    Note = "Test return 2",
                    OutboundDetails = new OutboundDetails
                    {
                        OutboundDetailsId = 2,
                        OutboundId = 2,
                        Outbound = new Outbound { OutboundCode = "OUT-456" },
                        Lot = new Lot
                        {
                            Product = new Product { ProductCode = "P-202", ProductName = "Test Product 2" }
                        }
                    }
                }
            };
            var returnDetailsQueryable = returnDetails.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.GetAll())
                .Returns(returnDetailsQueryable);

            // Act
            var result = await _returnOutboundService.GetAllReturnOutboundDetailsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllReturnOutboundDetailsAsync_ReturnsEmptyList_WhenNoData()
        {
            // Arrange
            var emptyReturnDetailsQueryable = new List<ReturnOutboundDetails>().AsQueryable().BuildMock();
            _unitOfWorkMock.Setup(u => u.ReturnOutboundDetailsRepository.GetAll())
                .Returns(emptyReturnDetailsQueryable);

            // Act
            var result = await _returnOutboundService.GetAllReturnOutboundDetailsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}