using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Xunit;
using NodaTime;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using MockQueryable;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq.Protected;

namespace DrugWarehouseManagement.UnitTest
{
    public class OutboundServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OutboundService _outboundService;

        public OutboundServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _outboundService = new OutboundService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task CreateOutboundAsync_CustomerNotExists_ThrowsException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest { CustomerId = 1, OutboundDetails = new List<CreateOutboundDetailRequest>() };

            _unitOfWorkMock.Setup(u => u.CustomerRepository.GetAll())
                .Returns(new List<Customer>().AsQueryable().BuildMock());

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _outboundService.CreateOutbound(accountId, request));
            Assert.Equal("Khách hàng chưa tồn tại. Vui lòng tạo Customer trước khi xuất hàng.", ex.Message);
        }

        [Fact]
        public async Task CreateOutbound_ShouldCreateOutbound_WhenValidRequest()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest
            {
                CustomerId = 1,
                OutboundDetails = new List<CreateOutboundDetailRequest>
        {
            new CreateOutboundDetailRequest
            {
                LotId = 1,
                Quantity = 10,
                UsePricingFormula = false
            }
        }
            };

            // 1) Customer exists
            var customer = new Customer { CustomerId = 1, IsLoyal = false };
            var custSet = new List<Customer> { customer }
                .AsQueryable()
                .BuildMockDbSet();
            _unitOfWorkMock
                .Setup(u => u.CustomerRepository.GetAll())
                .Returns(custSet.Object);

            // 2) Exactly one Lot, valid
            var lot = new Lot
            {
                LotId = 1,
                Quantity = 20,
                ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)),
                WarehouseId = 1,
                LotNumber = "L1",
                ProductId = 100
            };
            var lotSet = new List<Lot> { lot }
                .AsQueryable()
                .BuildMockDbSet();
            _unitOfWorkMock
                .Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(lotSet.Object);

            // 3) InboundDetail exists (to compute unit price)
            var inboundDetail = new InboundDetails
            {
                LotNumber = "L1",
                ProductId = 100,
                UnitPrice = 50m
            };
            var inboundSet = new List<InboundDetails> { inboundDetail }
                .AsQueryable()
                .BuildMockDbSet();
            _unitOfWorkMock
                .Setup(u => u.InboundDetailRepository.GetByWhere(It.IsAny<Expression<Func<InboundDetails, bool>>>()))
                .Returns(inboundSet.Object);

            // 4) OutboundRepository.GetAll() for loyalty count: return zero outbounds
            var emptyOutboundSet = new List<Outbound>()
                .AsQueryable()
                .BuildMockDbSet();
            _unitOfWorkMock
                .Setup(u => u.OutboundRepository.GetAll())
                .Returns(emptyOutboundSet.Object);

            // 5) CustomerRepository.GetAll() again inside UpdateCustomerLoyalty → same custSet
            //    (already setup in step 1)

            // 6) Create & Update & Save
            _unitOfWorkMock
                .Setup(u => u.OutboundRepository.CreateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.CustomerRepository.UpdateAsync(It.IsAny<Customer>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _outboundService.CreateOutbound(accountId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Code);
            Assert.Equal("Tạo đơn xuất thành công", result.Message);

            // Verify outbound created
            _unitOfWorkMock.Verify(u =>
                u.OutboundRepository.CreateAsync(It.IsAny<Outbound>()), Times.Once);

            // Verify loyalty update attempted (count=0 so no UpdateAsync on customer)
            // but SaveChangesAsync will be called once after Create
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }



        [Fact]
        public async Task GetOutboundByIdAsync_WhenExists_ReturnsResponse()
        {
            // Arrange
            var outboundId = 1;
            var outbound = new Outbound { OutboundId = outboundId, OutboundCode = "CODE1" };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(new List<Outbound> { outbound }.AsQueryable().BuildMock());

            // Act
            var result = await _outboundService.GetOutboundByIdAsync(outboundId);

            // Assert
           Assert.Equal(outboundId, result.OutboundId);
        }

        [Fact]
        public async Task UpdateOutboundAsync_StatusTransitionToCompleted_AdjustsLotQuantity()
        {
            // Arrange
            var outboundId = 2;
            var detail = new OutboundDetails { LotId = 5, Quantity = 3 };
            var outbound = new Outbound { OutboundId = outboundId, Status = OutboundStatus.InProgress, OutboundDetails = new List<OutboundDetails> { detail } };
            var lot = new Lot { LotId = 5, Quantity = 10 };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(new List<Outbound> { outbound }.AsQueryable().BuildMock());
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByIdAsync(detail.LotId))
                .ReturnsAsync(lot);
            _unitOfWorkMock.Setup(u => u.LotRepository.UpdateAsync(It.IsAny<Lot>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.OutboundRepository.UpdateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var request = new UpdateOutboundRequest { Status = OutboundStatus.Completed };

            // Act
            var response = await _outboundService.UpdateOutbound(outboundId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal(7, lot.Quantity);
        }

        [Fact]
        public async Task GenerateOutboundInvoicePdfAsync_WithValidId_ReturnsPdfBytes()
        {
            // Arrange
            var outboundId = 3;
            var outbound = new Outbound { OutboundId = outboundId, ReceiverName = "RCV", OutboundDetails = new List<OutboundDetails> { new OutboundDetails { Quantity = 2, UnitPrice = 20, Lot = new Lot { Product = new Product { ProductName = "P" } } } } };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
                .Returns(new List<Outbound> { outbound }.AsQueryable().BuildMock());

            // Act
            var pdf = await _outboundService.GenerateOutboundInvoicePdfAsync(outboundId);

            Assert.NotNull(pdf);
            Assert.IsType<byte[]>(pdf);
        }
    }
}
