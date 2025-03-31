using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using MockQueryable;

namespace DrugWarehouseManagement.UnitTest
{
    public class AuditLogsServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly IAuditLogsService _auditLogsService;

        public AuditLogsServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _auditLogsService = new AuditLogsService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ViewLogsAsync_ReturnsPaginatedLogs()
        {
            // Arrange
            var logs = new List<AuditLogs>
            {
                new AuditLogs { Action = "create", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user1" } },
                new AuditLogs { Action = "delete", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user2" } }
            }.AsQueryable().BuildMock();

            var queryPaging = new QueryPaging { Page = 1, PageSize = 10, Search = "create" };

            _unitOfWorkMock.Setup(uow => uow.AuditLogsRepository.GetAll())
                .Returns(logs);

            // Act
            var result = await _auditLogsService.ViewLogsAsync(queryPaging);

            // Assert
            Assert.Single(result.Items);
            Assert.Equal("create", result.Items.First().Action);
        }

        [Fact]
        public async Task ViewLogsAsync_EmptySearch_ReturnsAllLogs()
        {
            // Arrange
            var logs = new List<AuditLogs>
            {
                new AuditLogs { Action = "create", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user1" } },
                new AuditLogs { Action = "delete", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user2" } }
            }.AsQueryable().BuildMock();

            var queryPaging = new QueryPaging { Page = 1, PageSize = 10, Search = "" };

            _unitOfWorkMock.Setup(uow => uow.AuditLogsRepository.GetAll())
                .Returns(logs);

            // Act
            var result = await _auditLogsService.ViewLogsAsync(queryPaging);

            // Assert
            Assert.Equal(2, result.Items.Count);
        }

        [Fact]
        public async Task ViewLogsAsync_NoMatchingLogs_ReturnsEmptyResult()
        {
            // Arrange
            var logs = new List<AuditLogs>
            {
                new AuditLogs { Action = "create", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user1" } },
                new AuditLogs { Action = "delete", Date = SystemClock.Instance.GetCurrentInstant(), Account = new Account { Id = Guid.NewGuid(), UserName = "user2" } }
            }.AsQueryable().BuildMock();

            var queryPaging = new QueryPaging { Page = 1, PageSize = 10, Search = "update" };

            _unitOfWorkMock.Setup(uow => uow.AuditLogsRepository.GetAll())
                .Returns(logs);

            // Act
            var result = await _auditLogsService.ViewLogsAsync(queryPaging);

            // Assert
            Assert.Empty(result.Items);
        }
    }
}
