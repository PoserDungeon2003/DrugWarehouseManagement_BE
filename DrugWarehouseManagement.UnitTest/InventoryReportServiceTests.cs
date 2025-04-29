using System.Linq.Expressions;
using MockQueryable.Moq;
using Moq;
using NodaTime;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace DrugWarehouseManagement.UnitTest
{
    public class InventoryReportServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;
        private readonly InventoryReportService _svc;
        private readonly Instant _start;
        private readonly Instant _end;
        private readonly int _warehouseId = 1;

        public InventoryReportServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
            _svc = new InventoryReportService(_uow.Object);
            _start = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
            _end = SystemClock.Instance.GetCurrentInstant();
        }
    //    [Fact]
    //    public async Task ExportInventoryReportPdf_ShouldReturnPdfBytes()
    //    {
    //        // Act
    //        var result = await _svc.ExportInventoryReportPdf(_warehouseId, _start, _end);

    //        // Assert
    //        Assert.NotNull(result);
    //        Assert.True(result.Length > 0);
    //        // PDF files start with %PDF
    //        Assert.Equal(0x25, result[0]); // '%'
    //        Assert.Equal(0x50, result[1]); // 'P'
    //        Assert.Equal(0x44, result[2]); // 'D'
    //        Assert.Equal(0x46, result[3]); // 'F'
    //    }

    //    [Fact]
    //    public async Task ExportInventoryReportPdf_WithEmptyWarehouse_ShouldReturnValidPdf()
    //    {
    //        // Arrange
    //        var emptyMockUnitOfWork = SetupEmptyWarehouseMock();
    //        var service = new InventoryReportService(emptyMockUnitOfWork.Object);

    //        // Act
    //        var result = await service.ExportInventoryReportPdf(_warehouseId, _start, _end);

    //        // Assert
    //        Assert.NotNull(result);
    //        Assert.True(result.Length > 0);
    //    }

    //    private Mock<IUnitOfWork> SetupMockUnitOfWork()
    //    {
    //        var mockUnitOfWork = new Mock<IUnitOfWork>();

    //        // Setup repositories
    //        SetupLotRepository(mockUnitOfWork);
    //        SetupInventoryTransactionRepository(mockUnitOfWork);
    //        SetupInboundDetailRepository(mockUnitOfWork);
    //        SetupLotTransferDetailsRepository(mockUnitOfWork);
    //        SetupOutboundDetailsRepository(mockUnitOfWork);
    //        SetupInventoryCheckRepository(mockUnitOfWork);
    //        SetupWarehouseRepository(mockUnitOfWork);

    //        return mockUnitOfWork;
    //    }

    //    private Mock<IUnitOfWork> SetupEmptyWarehouseMock()
    //    {
    //        var mockUnitOfWork = new Mock<IUnitOfWork>();

    //        // Mock empty lot repository (no products)
    //        var mockLotRepo = new Mock<ILotRepository>();
    //        var emptyDbSet = CreateMockDbSet(new List<Lot>());
    //        mockLotRepo.Setup(r => r.GetAll()).Returns(emptyDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.LotRepository).Returns(mockLotRepo.Object);

    //        // Mock other necessary repositories with empty data
    //        SetupEmptyRepositories(mockUnitOfWork);
    //        SetupWarehouseRepository(mockUnitOfWork);

    //        return mockUnitOfWork;
    //    }

    //    private void SetupLotRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var products = new List<Product>
    //        {
    //            new Product { ProductId = 1, ProductCode = "P001", ProductName = "Product 1", SKU = "Box" },
    //            new Product { ProductId = 2, ProductCode = "P002", ProductName = "Product 2", SKU = "Bottle" }
    //        };

    //        var lots = new List<Lot>
    //        {
    //            new Lot { LotId = 1, ProductId = 1, WarehouseId = _warehouseId, Product = products[0] },
    //            new Lot { LotId = 2, ProductId = 2, WarehouseId = _warehouseId, Product = products[1] }
    //        };

    //        var mockLotRepo = new Mock<ILotRepository>();
    //        var mockLotDbSet = CreateMockDbSet(lots);
    //        mockLotRepo.Setup(r => r.GetAll()).Returns(mockLotDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.LotRepository).Returns(mockLotRepo.Object);
    //    }

    //    private void SetupInventoryTransactionRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var transactions = new List<InventoryTransaction>
    //        {
    //            new InventoryTransaction
    //            {
    //                Id = 1, LotId = 1, Quantity = 100,
    //                CreatedAt = _start.Minus(Duration.FromDays(1)),
    //                Lot = new Lot { LotId = 1, WarehouseId = _warehouseId, ProductId = 1 }
    //            },
    //            new InventoryTransaction
    //            {
    //                Id = 2, LotId = 2, Quantity = 150,
    //                CreatedAt = _start.Minus(Duration.FromDays(1)),
    //                Lot = new Lot { LotId = 2, WarehouseId = _warehouseId, ProductId = 2 }
    //            },
    //            new InventoryTransaction
    //            {
    //                Id = 3, LotId = 1, Quantity = 80,
    //                CreatedAt = _end,
    //                Lot = new Lot { LotId = 1, WarehouseId = _warehouseId, ProductId = 1 }
    //            },
    //            new InventoryTransaction
    //            {
    //                Id = 4, LotId = 2, Quantity = 130,
    //                CreatedAt = _end,
    //                Lot = new Lot { LotId = 2, WarehouseId = _warehouseId, ProductId = 2 }
    //            }
    //        };

    //        var mockRepo = new Mock<IInventoryTransactionRepository>();
    //        var mockDbSet = CreateMockDbSet(transactions);
    //        mockRepo.Setup(r => r.GetAll()).Returns(mockDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.InventoryTransactionRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupInboundDetailRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var inbounds = new List<Inbound>
    //        {
    //            new Inbound {
    //                InboundId = 1, WarehouseId = _warehouseId, Status = InboundStatus.Completed,
    //                InboundDate = _start.Plus(Duration.FromDays(5)), InboundRequestId = 1
    //            }
    //        };

    //        var inboundDetails = new List<InboundDetails>
    //        {
    //            new InboundDetails {
    //                InboundDetailsId = 1, InboundId = 1, ProductId = 1,
    //                Quantity = 20, Inbound = inbounds[0]
    //            }
    //        };

    //        var mockRepo = new Mock<IInboundDetailRepository>();
    //        var mockDbSet = CreateMockDbSet(inboundDetails);
    //        mockRepo.Setup(r => r.GetAll()).Returns(mockDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.InboundDetailRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupLotTransferDetailsRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var lotTransfers = new List<LotTransfer>
    //        {
    //            new LotTransfer {
    //                LotTransferId = 1, FromWareHouseId = 2, ToWareHouseId = _warehouseId,
    //                CreatedAt = _start.Plus(Duration.FromDays(10)),
    //                LotTransferStatus = LotTransferStatus.Completed,
    //                FromWareHouse = new Warehouse { WarehouseId = 2 },
    //                ToWareHouse = new Warehouse { WarehouseId = _warehouseId }
    //            },
    //            new LotTransfer {
    //                LotTransferId = 2, FromWareHouseId = _warehouseId, ToWareHouseId = 3,
    //                CreatedAt = _start.Plus(Duration.FromDays(15)),
    //                LotTransferStatus = LotTransferStatus.Completed,
    //                FromWareHouse = new Warehouse { WarehouseId = _warehouseId },
    //                ToWareHouse = new Warehouse { WarehouseId = 3 }
    //            },
    //            new LotTransfer {
    //                LotTransferId = 3, FromWareHouseId = 6, ToWareHouseId = _warehouseId,
    //                CreatedAt = _start.Plus(Duration.FromDays(20)),
    //                LotTransferStatus = LotTransferStatus.Completed,
    //                FromWareHouse = new Warehouse { WarehouseId = 6 },
    //                ToWareHouse = new Warehouse { WarehouseId = _warehouseId }
    //            }
    //        };

    //        var lotTransferDetails = new List<LotTransferDetail>
    //        {
    //            new LotTransferDetail {
    //                LotTransferDetailId = 1, LotTransferId = 1, LotId = 1,
    //                Quantity = 10, LotTransfer = lotTransfers[0],
    //                Lot = new Lot { LotId = 1, ProductId = 1 }
    //            },
    //            new LotTransferDetail{
    //                LotTransferDetailId = 2, LotTransferId = 2, LotId = 1,
    //                Quantity = 5, LotTransfer = lotTransfers[1],
    //                Lot = new Lot { LotId = 1, ProductId = 1 }
    //            },
    //            new LotTransferDetail {
    //                LotTransferDetailId = 3, LotTransferId = 3, LotId = 1,
    //                Quantity = 15, LotTransfer = lotTransfers[2],
    //                Lot = new Lot { LotId = 1, ProductId = 1 }
    //            }
    //        };

    //        var mockRepo = new Mock<ILotTransferDetailRepository>();
    //        var mockDbSet = CreateMockDbSet(lotTransferDetails);
    //        mockRepo.Setup(r => r.GetAll()).Returns(mockDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.LotTransferDetailsRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupOutboundDetailsRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var outbounds = new List<Outbound>
    //        {
    //            new Outbound {
    //                OutboundId = 1, OutboundDate = _start.Plus(Duration.FromDays(12)),
    //                Status = OutboundStatus.Completed,
    //                Customer = new Customer { CustomerId = 1, CustomerName = "Customer A" }
    //            },
    //            new Outbound {
    //                OutboundId = 2, OutboundDate = _start.Plus(Duration.FromDays(25)),
    //                Status = OutboundStatus.Completed,
    //                Customer = new Customer { CustomerId = 2, CustomerName = "Customer B" }
    //            }
    //        };

    //        var outboundDetails = new List<OutboundDetails>
    //        {
    //            new OutboundDetails {
    //                OutboundDetailsId = 1, OutboundId = 1, LotId = 1, Quantity = 25,
    //                TotalPrice = 2500, Outbound = outbounds[0],
    //                Lot = new Lot { LotId = 1, ProductId = 1, WarehouseId = _warehouseId }
    //            },
    //            new OutboundDetails {
    //                OutboundDetailsId = 2, OutboundId = 2, LotId = 2, Quantity = 10,
    //                TotalPrice = 0, Outbound = outbounds[1], // Sample export (TotalPrice = 0)
    //                Lot = new Lot { LotId = 2, ProductId = 2, WarehouseId = _warehouseId }
    //            }
    //        };

    //        var mockRepo = new Mock<IOutboundDetailsRepository>();
    //        var mockDbSet = CreateMockDbSet(outboundDetails);
    //        mockRepo.Setup(r => r.GetAll()).Returns(mockDbSet.Object);
    //        mockUnitOfWork.Setup(u => u.OutboundDetailsRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupInventoryCheckRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var inventoryChecks = new List<InventoryCheck>
    //{
    //    new InventoryCheck {
    //        InventoryCheckId = 1, WarehouseId = _warehouseId,
    //        CheckDate = _start.Plus(Duration.FromDays(18)),
    //        InventoryCheckDetails = new List<InventoryCheckDetail>(),
    //        Warehouse = new Warehouse { WarehouseId = _warehouseId }
    //    }
    //};

    //        var inventoryCheckDetails = new List<InventoryCheckDetail>
    //{
    //    new InventoryCheckDetail {
    //        InventoryCheckDetailId = 1, InventoryCheckId = 1, LotId = 1,
    //        Status = InventoryCheckStatus.Damaged, CheckQuantity = 5,
    //        Lot = new Lot { LotId = 1, ProductId = 1 }
    //    },
    //    new InventoryCheckDetail {
    //        InventoryCheckDetailId = 2, InventoryCheckId = 1, LotId = 2,
    //        Status = InventoryCheckStatus.Lost, CheckQuantity = 3,
    //        Lot = new Lot { LotId = 2, ProductId = 2 }
    //    }
    //};

    //        // Add details to inventory check
    //        inventoryChecks[0].InventoryCheckDetails = inventoryCheckDetails;

    //        var mockRepo = new Mock<IInventoryCheckRepository>();

    //        // Create a queryable mock that will be used for the repository
    //        var queryableInventoryChecks = inventoryChecks.AsQueryable();

    //        // Setup the GetByWhere method to return a mock that properly handles Include/ThenInclude
    //        mockRepo.Setup(r => r.GetByWhere(It.IsAny<Expression<Func<InventoryCheck, bool>>>()))
    //            .Returns((Expression<Func<InventoryCheck, bool>> predicate) => {
    //                // Create mock for the query result (filtered by the predicate)
    //                var mockIncludableQueryable = new Mock<IIncludableQueryable<InventoryCheck, object>>();

    //                // Setup Include to return an IIncludableQueryable
    //                mockIncludableQueryable
    //                    .Setup(m => m.Include(It.IsAny<string>()))
    //                    .Returns(mockIncludableQueryable.Object);

    //                // Setup ThenInclude to return an IIncludableQueryable
    //                mockIncludableQueryable
    //                    .Setup(m => m.ThenInclude(It.IsAny<Expression<Func<object, object>>>()))
    //                    .Returns(mockIncludableQueryable.Object);

    //                // Setup the LINQ methods needed by the service
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.Provider).Returns(queryableInventoryChecks.Provider);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.Expression).Returns(queryableInventoryChecks.Expression);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.ElementType).Returns(queryableInventoryChecks.ElementType);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.GetEnumerator()).Returns(() => inventoryChecks.GetEnumerator());

    //                return mockIncludableQueryable.Object;
    //            });

    //        mockUnitOfWork.Setup(u => u.InventoryCheckRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupWarehouseRepository(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var warehouse = new Warehouse
    //        {
    //            WarehouseId = _warehouseId,
    //            WarehouseName = "Test Warehouse",
    //            DocumentNumber = "WH001"
    //        };

    //        var mockDbSet = CreateMockDbSet(new List<Warehouse> { warehouse });
    //        var mockRepo = new Mock<IWarehouseRepository>();

    //        mockRepo.Setup(r => r.GetByWhere(It.IsAny<Expression<Func<Warehouse, bool>>>()))
    //            .Returns(mockDbSet.Object);

    //        mockUnitOfWork.Setup(u => u.WarehouseRepository).Returns(mockRepo.Object);
    //    }

    //    private void SetupEmptyRepositories(Mock<IUnitOfWork> mockUnitOfWork)
    //    {
    //        var mockInventoryCheckRepo = new Mock<IInventoryCheckRepository>();
    //        mockInventoryCheckRepo.Setup(r => r.GetByWhere(It.IsAny<Expression<Func<InventoryCheck, bool>>>()))
    //            .Returns((Expression<Func<InventoryCheck, bool>> predicate) => {
    //                var emptyQueryable = new List<InventoryCheck>().AsQueryable();
    //                var mockIncludableQueryable = new Mock<IIncludableQueryable<InventoryCheck, object>>();

    //                mockIncludableQueryable
    //                    .Setup(m => m.Include(It.IsAny<string>()))
    //                    .Returns(mockIncludableQueryable.Object);

    //                mockIncludableQueryable
    //                    .Setup(m => m.ThenInclude(It.IsAny<Expression<Func<object, object>>>()))
    //                    .Returns(mockIncludableQueryable.Object);

    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.Provider).Returns(emptyQueryable.Provider);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.Expression).Returns(emptyQueryable.Expression);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.ElementType).Returns(emptyQueryable.ElementType);
    //                mockIncludableQueryable.As<IQueryable<InventoryCheck>>()
    //                    .Setup(m => m.GetEnumerator()).Returns(() => emptyQueryable.GetEnumerator());

    //                return mockIncludableQueryable.Object;
    //            });
    //        mockUnitOfWork.Setup(u => u.InventoryCheckRepository).Returns(mockInventoryCheckRepo.Object);
    //    }

    //    private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    //    {
    //        var queryable = data.AsQueryable();
    //        var mockDbSet = new Mock<DbSet<T>>();

    //        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
    //        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
    //        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
    //        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

    //        // Setup Include method to return itself
    //        mockDbSet.Setup(m => m.Include(It.IsAny<string>())).Returns(mockDbSet.Object);

    //        // Setup async operations
    //        mockDbSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>()))
    //            .ReturnsAsync(data);

    //        return mockDbSet;
    //    }

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
