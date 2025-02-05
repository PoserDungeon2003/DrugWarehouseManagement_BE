using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Services;
using Moq;
using System.Linq.Expressions;
using System.Net;

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
        public async Task CreateOutbound_Success()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest
            {
                CustomerName = "Test Customer",
                OutboundDetails = new List<CreateOutboundDetailRequest> { new CreateOutboundDetailRequest { LotId = 1, Quantity = 5, UnitPrice = 10, UnitType = "Box" } }
            };

            var lots = new List<Lot> { new Lot { LotId = 1, LotNumber = "LOT123", Quantity = 10, ExpiryDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1)), ProductId = 1 } };

            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(lots.AsQueryable());

            _unitOfWorkMock.Setup(u => u.OutboundRepository.CreateAsync(It.IsAny<Outbound>()))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _outboundService.CreateOutbound(accountId, request);

            // Assert
            Assert.Equal((int)HttpStatusCode.OK, response.Code);
            Assert.Equal("Outbound created successfully", response.Message);
        }

        [Fact]
        public async Task CreateOutbound_LotIdNotFound()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest
            {
                CustomerName = "Test Customer",
                OutboundDetails = new List<CreateOutboundDetailRequest> { new CreateOutboundDetailRequest { LotId = 1, Quantity = 5, UnitPrice = 10, UnitType = "Box" } }
            };
            var lots = new List<Lot>();
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(lots.AsQueryable());
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _outboundService.CreateOutbound(accountId, request));
            Assert.Equal("One or some LotId not found.", exception.Message);
        }

        [Fact]
        public async Task CreateOutbound_InsufficientQuantity()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest
            {
                CustomerName = "Test Customer",
                OutboundDetails = new List<CreateOutboundDetailRequest> { new CreateOutboundDetailRequest { LotId = 1, Quantity = 15, UnitPrice = 10, UnitType = "Box" } }
            };

            var lots = new List<Lot> { new Lot { LotId = 1, LotNumber = "LOT123", Quantity = 10, ExpiryDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1)), ProductId = 1 } };
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(lots.AsQueryable());
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _outboundService.CreateOutbound(accountId, request));
            Assert.Equal("Số lượng trong lô LOT123 không đủ. Hiện có: 10, yêu cầu: 15", exception.Message);
        }

        [Fact]
        public async Task CreateOutbound_LotNumberMissing()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateOutboundRequest
            {
                CustomerName = "Test Customer",
                OutboundDetails = new List<CreateOutboundDetailRequest> { new CreateOutboundDetailRequest { LotId = 1, Quantity = 5, UnitPrice = 10, UnitType = "Box" } }
            };

            var lots = new List<Lot> { new Lot { LotId = 1, LotNumber = null, Quantity = 10, ExpiryDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow.AddYears(1)), ProductId = 1 } };
            _unitOfWorkMock.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Expression<Func<Lot, bool>>>()))
                .Returns(lots.AsQueryable());
            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _outboundService.CreateOutbound(accountId, request));
            Assert.Equal("LotNumber is missing for LotId 1.", exception.Message);
        }


        [Fact]
        public async Task SearchOutboundsAsync_SearchByOutboundId_Success()
        {
            // Arrange
            var queryPaging = new QueryPaging { Page = 1, PageSize = 10, Search = "1" };
            var outbounds = new List<Outbound>
            {
                new Outbound { OutboundId = 1, OutboundCode = "OUTB-1234", OutboundDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow) }
            };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetAll())
                .Returns(outbounds.AsQueryable());

            // Act
            var result = await _outboundService.SearchOutboundsAsync(queryPaging);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().OutboundId);
        }

        [Fact]
        public async Task SearchOutboundsAsync_SearchByOutboundCode_Success()
        {
            // Arrange
            var queryPaging = new QueryPaging { Page = 1, PageSize = 10, Search = "OUTB-1234" };
            var outbounds = new List<Outbound>
            {
                new Outbound { OutboundId = 1, OutboundCode = "OUTB-1234", OutboundDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow) }
            };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetAll())
                .Returns(outbounds.AsQueryable());

            // Act
            var result = await _outboundService.SearchOutboundsAsync(queryPaging);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("OUTB-1234", result.Items.First().OutboundCode);
        }

        [Fact]
        public async Task SearchOutboundsAsync_Pagination_Success()
        {
            // Arrange
            var queryPaging = new QueryPaging { Page = 1, PageSize = 1 };
            var outbounds = new List<Outbound>
            {
                new Outbound { OutboundId = 1, OutboundCode = "OUTB-1234", OutboundDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow) },
                new Outbound { OutboundId = 2, OutboundCode = "OUTB-5678", OutboundDate = NodaTime.Instant.FromDateTimeUtc(DateTime.UtcNow.AddDays(-1)) }
            };

            _unitOfWorkMock.Setup(u => u.OutboundRepository.GetAll())
                .Returns(outbounds.AsQueryable());

            // Act
            var result = await _outboundService.SearchOutboundsAsync(queryPaging);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().OutboundId);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(1, result.PageSize);
            Assert.Equal(1, result.CurrentPage);
        }
    }
}
