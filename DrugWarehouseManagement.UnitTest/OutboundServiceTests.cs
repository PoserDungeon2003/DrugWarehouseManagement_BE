//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using Moq;
//using DrugWarehouseManagement.Service.Services;
//using DrugWarehouseManagement.Repository;
//using DrugWarehouseManagement.Repository.Models;
//using DrugWarehouseManagement.Service.DTO.Request;
//using DrugWarehouseManagement.Service.DTO.Response;
//using Microsoft.EntityFrameworkCore;
//using DrugWarehouseManagement.Common;

//namespace DrugWarehouseManagement.UnitTest
//{
//    public class OutboundServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly OutboundService _outboundService;

//        public OutboundServiceTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _outboundService = new OutboundService(_mockUnitOfWork.Object);
//        }

//        [Fact]
//        public async Task GetOutboundByIdAsync_ShouldReturnOutboundResponse_WhenOutboundExists()
//        {
//            Arrange
//           var outboundId = 1;
//            var outbound = new Outbound
//            {
//                OutboundId = outboundId,
//                OutboundCode = "OUTB-123",
//                Customer = new Customer { CustomerName = "John Doe" },
//                OutboundDetails = new List<OutboundDetails>
//                {
//                    new OutboundDetails
//                    {
//                        Lot = new Lot
//                        {
//                            Product = new Product { ProductName = "Product A" },
//                            Warehouse = new Warehouse { WarehouseName = "Warehouse A" }
//                        }
//                    }
//                }
//            };

//            var mockDbSet = new Mock<DbSet<Outbound>>();
//            _mockUnitOfWork.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Func<Outbound, bool>>()))
//                .Returns(mockDbSet.Object);
//            _mockUnitOfWork.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Func<Outbound, bool>>())
//                .Include(It.IsAny<Func<Outbound, object>>()))
//                .Returns(mockDbSet.Object);

//            Act
//           var result = await _outboundService.GetOutboundByIdAsync(outboundId);

//            Assert
//            Assert.NotNull(result);
//            Assert.Equal(outboundId, result.OutboundId);
//        }

//        [Fact]
//        public async Task CreateOutbound_ShouldReturnSuccessResponse_WhenValidRequest()
//        {
//            Arrange
//           var accountId = Guid.NewGuid();
//            var request = new CreateOutboundRequest
//            {
//                CustomerId = 1,
//                OutboundDetails = new List<CreateOutboundDetailRequest>
//                {
//                    new CreateOutboundDetailRequest { LotId = 1, Quantity = 10 }
//                }
//            };

//            var customer = new Customer { CustomerId = 1 };
//            var lots = new List<Lot>
//            {
//                new Lot { LotId = 1, Quantity = 20, ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(10)) }
//            };

//            _mockUnitOfWork.Setup(u => u.CustomerRepository.GetAll())
//                .Returns(MockDbSet(new List<Customer> { customer }).Object);
//            _mockUnitOfWork.Setup(u => u.LotRepository.GetByWhere(It.IsAny<Func<Lot, bool>>()))
//                .Returns(MockDbSet(lots).Object);

//            Act
//           var result = await _outboundService.CreateOutbound(accountId, request);

//            Assert
//            Assert.NotNull(result);
//            Assert.Equal(200, result.Code);
//            Assert.Equal("Outbound created successfully", result.Message);
//        }

//        [Fact]
//        public async Task SearchOutboundsAsync_ShouldReturnPaginatedResult_WhenValidRequest()
//        {
//            Arrange
//           var request = new SearchOutboundRequest
//           {
//               Page = 1,
//               PageSize = 10
//           };

//            var outbounds = new List<Outbound>
//            {
//                new Outbound { OutboundId = 1, OutboundCode = "OUTB-123" }
//            };

//            _mockUnitOfWork.Setup(u => u.OutboundRepository.GetAll())
//                .Returns(MockDbSet(outbounds).Object);

//            Act
//           var result = await _outboundService.SearchOutboundsAsync(request);

//            Assert
//            Assert.NotNull(result);
//            Assert.Single(result.Items);
//        }

//        [Fact]
//        public async Task UpdateOutbound_ShouldReturnSuccessResponse_WhenValidRequest()
//        {
//            Arrange
//           var outboundId = 1;
//            var request = new UpdateOutboundRequest
//            {
//                Status = OutboundStatus.Completed
//            };

//            var outbound = new Outbound
//            {
//                OutboundId = outboundId,
//                Status = OutboundStatus.InProgress,
//                OutboundDetails = new List<OutboundDetails>()
//            };

//            _mockUnitOfWork.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Func<Outbound, bool>>()))
//                .Returns(MockDbSet(new List<Outbound> { outbound }).Object);

//            Act
//           var result = await _outboundService.UpdateOutbound(outboundId, request);

//            Assert
//            Assert.NotNull(result);
//            Assert.Equal(200, result.Code);
//            Assert.Equal("Cập nhật đơn xuất thành công", result.Message);
//        }

//        [Fact]
//        public async Task GenerateOutboundInvoicePdfAsync_ShouldReturnPdfBytes_WhenOutboundExists()
//        {
//            Arrange
//           var outboundId = 1;
//            var outbound = new Outbound
//            {
//                OutboundId = outboundId,
//                OutboundCode = "OUTB-123",
//                OutboundDetails = new List<OutboundDetails>
//                {
//                    new OutboundDetails
//                    {
//                        Lot = new Lot
//                        {
//                            Product = new Product { ProductName = "Product A" }
//                        }
//                    }
//                }
//            };

//            _mockUnitOfWork.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Func<Outbound, bool>>()))
//                .Returns(MockDbSet(new List<Outbound> { outbound }).Object);

//            Act
//           var result = await _outboundService.GenerateOutboundInvoicePdfAsync(outboundId);

//            Assert
//            Assert.NotNull(result);
//            Assert.IsType<byte[]>(result);
//        }

//        private Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
//        {
//            var queryable = data.AsQueryable();
//            var mockSet = new Mock<DbSet<T>>();
//            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
//            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
//            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
//            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
//            return mockSet;
//        }
//    }
//}
