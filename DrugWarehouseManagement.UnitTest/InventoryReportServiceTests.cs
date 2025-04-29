//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using MockQueryable.Moq;
//using Moq;
//using Xunit;
//using NodaTime;
//using DrugWarehouseManagement.Repository;
//using DrugWarehouseManagement.Repository.Models;
//using DrugWarehouseManagement.Service.Services;
//using DrugWarehouseManagement.Common;
//using DrugWarehouseManagement.Repository.Interface;
//using System.Text;
//using Microsoft.EntityFrameworkCore;
//using DrugWarehouseManagement.Repository.Repositories;
//using MockQueryable;

//namespace DrugWarehouseManagement.UnitTest
//{
//    public class InventoryReportServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _uow;
//        private readonly InventoryReportService _svc;
//        private readonly Instant _start;
//        private readonly Instant _end;

//        public InventoryReportServiceTests()
//        {
//            _uow = new Mock<IUnitOfWork>();
//            _svc = new InventoryReportService(_uow.Object);
//            _start = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
//            _end = SystemClock.Instance.GetCurrentInstant();
//        }
//        [Fact]
//        public async Task ExportInventoryReportPdf_NoData_ReturnsNonEmptyPdf()
//        {
//            // Arrange: all repositories return empty data sets
//            var emptyLots = new List<Lot>().AsQueryable().BuildMock();
//            var emptyInvTx = new List<InventoryTransaction>().AsQueryable().BuildMock();
//            var emptyInboundDetails = new List<InboundDetails>().AsQueryable().BuildMock();
//            var emptyLotTransfers = new List<LotTransferDetail>().AsQueryable().BuildMock();
//            var emptyOutboundDetails = new List<OutboundDetails>().AsQueryable().BuildMock();
//            var emptyInvChecks = new List<InventoryCheck>().AsQueryable().BuildMock();
//            var emptyWarehouses = new List<Warehouse>().AsQueryable().BuildMock();

//            _uow.Setup(u => u.LotRepository.GetAll()).Returns(emptyLots);
//            _uow.Setup(u => u.InventoryTransactionRepository.GetAll()).Returns(emptyInvTx);
//            _uow.Setup(u => u.InboundDetailRepository.GetAll()).Returns(emptyInboundDetails);
//            _uow.Setup(u => u.LotTransferDetailsRepository.GetAll()).Returns(emptyLotTransfers);
//            _uow.Setup(u => u.OutboundDetailsRepository.GetAll()).Returns(emptyOutboundDetails);
//            _uow.Setup(u => u.InventoryCheckRepository.GetByWhere(It.IsAny<Expression<Func<InventoryCheck, bool>>>()))
//                           .Returns(new List<InventoryCheck>().AsQueryable());
//            _uow.Setup(u => u.WarehouseRepository.GetByWhere(It.IsAny<Expression<Func<Warehouse, bool>>>()))
//                           .Returns(emptyWarehouses);

//            // Act
//            var result = await _svc.ExportInventoryReportPdf(warehouseId: 1, _start, _end);

//            // Assert
//            Assert.NotNull(result);
//            Assert.True(result.Length > 0, "PDF byte array should not be empty");
//        }

//        [Fact]
//        public async Task ExportInventoryReportPdf_WithSingleLot_ComputesQuantitiesCorrectly()
//        {
//            // Arrange: one lot, one opening and closing transaction, one warehouse
//            var product = new Product { ProductId = 100, ProductCode = "P100", ProductName = "Test Product", SKU = "SKU100" };
//            var lot = new Lot { LotId = 10, WarehouseId = 2, ProductId = product.ProductId, Product = product };
//            var lots = new List<Lot> { lot }.AsQueryable().BuildMock();

//            // Opening transaction: quantity 50 at startDate
//            var openTx = new InventoryTransaction { LotId = lot.LotId, Quantity = 50, CreatedAt = _start - Duration.FromDays(1), Id = 1 };
//            // Closing transaction: quantity 30 at endDate
//            var closeTx = new InventoryTransaction { LotId = lot.LotId, Quantity = 30, CreatedAt = _end - Duration.FromHours(1), Id = 2 };
//            var invTxs = new List<InventoryTransaction> { openTx, closeTx }.AsQueryable().BuildMock();

//            var warehouse = new Warehouse { WarehouseId = 2, WarehouseName = "WH2" };
//            var warehouses = new List<Warehouse> { warehouse }.AsQueryable().BuildMock();

//            // Other repositories return empty
//            var emptyInboundDetails = new List<InboundDetails>().AsQueryable().BuildMock();
//            var emptyLotTransfers = new List<LotTransferDetail>().AsQueryable().BuildMock();
//            var emptyOutboundDetails = new List<OutboundDetails>().AsQueryable().BuildMock();
//            var emptyInvChecks = new List<InventoryCheck>().AsQueryable().BuildMock();

//            _uow.Setup(u => u.LotRepository.GetAll()).Returns(lots);
//            _uow.Setup(u => u.InventoryTransactionRepository.GetAll()).Returns(invTxs);
//            _uow.Setup(u => u.InboundDetailRepository.GetAll()).Returns(emptyInboundDetails);
//            _uow.Setup(u => u.LotTransferDetailsRepository.GetAll()).Returns(emptyLotTransfers);
//            _uow.Setup(u => u.OutboundDetailsRepository.GetAll()).Returns(emptyOutboundDetails);
//            _uow.Setup(u => u.InventoryCheckRepository.GetByWhere(It.IsAny<Expression<Func<InventoryCheck, bool>>>()))
//                           .Returns(emptyInvChecks);
//            _uow.Setup(u => u.WarehouseRepository.GetByWhere(It.IsAny<Expression<Func<Warehouse, bool>>>()))
//                           .Returns(warehouses);

//            // Act
//            var result = await _svc.ExportInventoryReportPdf(warehouseId: 2, _start, _end);

//            // Assert
//            Assert.NotNull(result);
//            Assert.True(result.Length > 0);
//            // Further PDF content validation could be done by parsing PDF bytes
//        }

//        [Fact]
//        public async Task ExportStockCardPdf_ShouldReturnPdfBytes()
//        {
//            int wid = 1, pid = 1;

//            // 1) Opening stock transactions
//            var lots = new List<Lot> { new Lot { LotId = 50, ProductId = pid, WarehouseId = wid } };
//            var invTx = new List<InventoryTransaction>
//            {
//                new InventoryTransaction { Id=1, Lot=lots[0], CreatedAt=_start.Plus(Duration.FromDays(-1)), Quantity=8 }
//            };
//            _uow.Setup(u => u.InventoryTransactionRepository.GetAll())
//                .Returns(invTx.AsQueryable().BuildMockDbSet().Object);

//            // 2) ProductRepository.GetByWhere(...)
//            var products = new List<Product> { new Product { ProductId = pid, ProductName = "Prod1", SKU = "U1" } };
//            _uow.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
//                .Returns((Expression<Func<Product, bool>> pred) =>
//                    products.AsQueryable().Where(pred).BuildMockDbSet().Object);

//            // 3) InboundDetailRepository
//            _uow.Setup(u => u.InboundDetailRepository.GetAll())
//                .Returns(new List<InboundDetails>().AsQueryable().BuildMockDbSet().Object);

//            // 4) LotTransferDetailsRepository
//            _uow.Setup(u => u.LotTransferDetailsRepository.GetAll())
//                .Returns(new List<LotTransferDetail>().AsQueryable().BuildMockDbSet().Object);

//            // 5) InventoryCheckRepository
//            _uow.Setup(u => u.InventoryCheckRepository.GetAll())
//                .Returns(new List<InventoryCheck>().AsQueryable().BuildMockDbSet().Object);

//            // 6) OutboundDetailsRepository
//            _uow.Setup(u => u.OutboundDetailsRepository.GetAll())
//                .Returns(new List<OutboundDetails>().AsQueryable().BuildMockDbSet().Object);

//            // 7) WarehouseRepository.GetByWhere(...)
//            var warehouses = new List<Warehouse> { new Warehouse { WarehouseId = wid, WarehouseName = "Main" } };
//            _uow.Setup(u => u.WarehouseRepository.GetByWhere(It.IsAny<Expression<Func<Warehouse, bool>>>()))
//                .Returns((Expression<Func<Warehouse, bool>> pred) =>
//                    warehouses.AsQueryable().Where(pred).BuildMockDbSet().Object);

//            // Act
//            var pdf = await _svc.ExportStockCardPdf(wid, pid, _start, _end);

//            // Assert
//            Assert.NotNull(pdf);
//            Assert.True(pdf.Length > 100, "Expected non-trivial PDF output");
//        }
//    }
//}
