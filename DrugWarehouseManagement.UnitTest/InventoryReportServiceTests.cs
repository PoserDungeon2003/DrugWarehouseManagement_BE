using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MockQueryable.Moq;
using Moq;
using Xunit;
using NodaTime;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Interface;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DrugWarehouseManagement.Repository.Repositories;
using MockQueryable;

namespace DrugWarehouseManagement.UnitTest
{
    public class InventoryReportServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly InventoryReportService _svc;
        private readonly Instant _start;
        private readonly Instant _end;

        public InventoryReportServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _svc = new InventoryReportService(_uow.Object);
            _start = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
            _end = SystemClock.Instance.GetCurrentInstant();
        }
        [Fact]
        public async Task ExportInventoryReportPdf_ShouldReturnPdfBytes()
        {
            // Arrange
            int warehouseId = 1;
            Instant startDate = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
            Instant endDate = SystemClock.Instance.GetCurrentInstant();

            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductCode = "P001", ProductName = "Product 1", SKU = "SKU1" },
                new Product { ProductId = 2, ProductCode = "P002", ProductName = "Product 2", SKU = "SKU2" }
            };

            _uow.Setup(u => u.LotRepository.GetAll())
                .Returns(products.AsQueryable().Select(p => new Lot { Product = p }).AsQueryable());

            _uow.Setup(u => u.InventoryTransactionRepository.GetAll())
                .Returns(new List<InventoryTransaction>
                {
                    new InventoryTransaction { LotId = 1, CreatedAt = startDate.Plus(Duration.FromDays(-1)), Quantity = 10 },
                    new InventoryTransaction { LotId = 2, CreatedAt = endDate.Plus(Duration.FromDays(-1)), Quantity = 20 }
                }.AsQueryable());

            _uow.Setup(u => u.WarehouseRepository.GetByWhere(It.IsAny<System.Linq.Expressions.Expression<Func<Warehouse, bool>>>()))
                .Returns(new List<Warehouse> { new Warehouse { WarehouseId = warehouseId, WarehouseName = "Main Warehouse" } }.AsQueryable());

            // Act
            var result = await _svc.ExportInventoryReportPdf(warehouseId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<byte[]>(result);
        }

        [Fact]
        public async Task ExportStockCardPdf_ShouldReturnPdfBytes()
        {
            int wid = 1, pid = 1;

            // 1) Opening stock transactions
            var lots = new List<Lot> { new Lot { LotId = 50, ProductId = pid, WarehouseId = wid } };
            var invTx = new List<InventoryTransaction>
            {
                new InventoryTransaction { Id=1, Lot=lots[0], CreatedAt=_start.Plus(Duration.FromDays(-1)), Quantity=8 }
            };
            _uow.Setup(u => u.InventoryTransactionRepository.GetAll())
                .Returns(invTx.AsQueryable().BuildMockDbSet().Object);

            // 2) ProductRepository.GetByWhere(...)
            var products = new List<Product> { new Product { ProductId = pid, ProductName = "Prod1", SKU = "U1" } };
            _uow.Setup(u => u.ProductRepository.GetByWhere(It.IsAny<Expression<Func<Product, bool>>>()))
                .Returns((Expression<Func<Product, bool>> pred) =>
                    products.AsQueryable().Where(pred).BuildMockDbSet().Object);

            // 3) InboundDetailRepository
            _uow.Setup(u => u.InboundDetailRepository.GetAll())
                .Returns(new List<InboundDetails>().AsQueryable().BuildMockDbSet().Object);

            // 4) LotTransferDetailsRepository
            _uow.Setup(u => u.LotTransferDetailsRepository.GetAll())
                .Returns(new List<LotTransferDetail>().AsQueryable().BuildMockDbSet().Object);

            // 5) InventoryCheckRepository
            _uow.Setup(u => u.InventoryCheckRepository.GetAll())
                .Returns(new List<InventoryCheck>().AsQueryable().BuildMockDbSet().Object);

            // 6) OutboundDetailsRepository
            _uow.Setup(u => u.OutboundDetailsRepository.GetAll())
                .Returns(new List<OutboundDetails>().AsQueryable().BuildMockDbSet().Object);

            // 7) WarehouseRepository.GetByWhere(...)
            var warehouses = new List<Warehouse> { new Warehouse { WarehouseId = wid, WarehouseName = "Main" } };
            _uow.Setup(u => u.WarehouseRepository.GetByWhere(It.IsAny<Expression<Func<Warehouse, bool>>>()))
                .Returns((Expression<Func<Warehouse, bool>> pred) =>
                    warehouses.AsQueryable().Where(pred).BuildMockDbSet().Object);

            // Act
            var pdf = await _svc.ExportStockCardPdf(wid, pid, _start, _end);

            // Assert
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 100, "Expected non-trivial PDF output");
        }
    }
}
