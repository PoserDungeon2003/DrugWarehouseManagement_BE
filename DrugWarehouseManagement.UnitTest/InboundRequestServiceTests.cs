using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using NodaTime;
using DrugWarehouseManagement.Service.DTO.Response;
using NodaTime.Text;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Service.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DrugWarehouseManagement.UnitTest
{
    public class InboundRequestServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMinioService> _minioServiceMock;
        private readonly Mock<ILogger<InboundRequestService>> _loggerMock;
        private readonly InboundRequestService _inboundRequestService;
        private readonly Mock<INotificationService> _notificationServiceMock;
        public InboundRequestServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _minioServiceMock = new Mock<IMinioService>();
            _loggerMock = new Mock<ILogger<InboundRequestService>>();
            _notificationServiceMock = new Mock<INotificationService>();
            _inboundRequestService = new InboundRequestService(_unitOfWorkMock.Object, _minioServiceMock.Object, _loggerMock.Object, _notificationServiceMock.Object);
        }

        // [Fact]
        // public async Task CreateInboundRequest_AccountNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new CreateInboundOrderRequest();
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync((Account)null);

        //     // Act
        //     var response = await _inboundRequestService.CreateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Equal("Account not found", response.Message);
        // }

        // [Fact]
        // public async Task CreateInboundRequest_WithDetailsAndImages_CreatesSuccessfully()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var account = new Account { Id = accountId };
        //     var fileMock = new Mock<IFormFile>();
        //     fileMock.Setup(f => f.Length).Returns(100); // Ensure Length > 0
        //     fileMock.Setup(f => f.FileName).Returns("test.jpg"); // Required for FileUploadResponse
        //     var request = new CreateInboundOrderRequest
        //     {
        //         InboundRequestDetails = new List<InboundOrderDetailRequest>
        // {
        //     new InboundOrderDetailRequest
        //     {
        //         ProductId = 1,
        //         Quantity = 1,
        //         UnitPrice = 100,
        //         TotalPrice = 100
        //     }
        // },
        //         Images = new List<IFormFile> { fileMock.Object }
        //     };

        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(account);
        //     _minioServiceMock.Setup(m => m.FileUpload(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>(), null))
        //         .ReturnsAsync(new FileUploadResponse { Extension = ".jpg" });
        //     _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.CreateAsync(It.IsAny<InboundRequest>()))
        //         .Returns(Task.CompletedTask);
        //     _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
        //         .Returns(Task.CompletedTask);
        //     //_notificationServiceMock.Setup(ns => ns.NotifyRoleAsync("Accountant", It.IsAny<string>()))
        //     //    .Returns(Task.CompletedTask);

        //     // Act
        //     var response = await _inboundRequestService.CreateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(200, response.Code);
        //     Assert.Equal("Inbound Request record created successfully", response.Message);
        //     _unitOfWorkMock.Verify(uow => uow.InboundRequestRepository.CreateAsync(It.Is<InboundRequest>(ir => ir.Price == 100 && ir.Assets.Count == 1)), Times.Once);
        //     _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        //     //_notificationServiceMock.Verify(ns => 
        //     //    ns.NotifyRoleAsync("Accountant", It.Is<string>(msg => msg.Contains("New Inbound Request"))), Times.Once);
        // }

        // [Fact]
        // public async Task CreateInboundRequest_ImageUploadFails_ReturnsError()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var account = new Account { Id = accountId };
        //     var fileMock = new Mock<IFormFile>();
        //     fileMock.Setup(f => f.Length).Returns(100);
        //     fileMock.Setup(f => f.FileName).Returns("test.jpg");
        //     var request = new CreateInboundOrderRequest
        //     {
        //         Images = new List<IFormFile> { fileMock.Object },
        //         InboundRequestDetails = null
        //     };

        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(account);
        //     _minioServiceMock.Setup(m => m.FileUpload(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>(), null))
        //         .ThrowsAsync(new Exception("Upload failed"));

        //     // Act
        //     var response = await _inboundRequestService.CreateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(500, response.Code);
        //     Assert.StartsWith("Error uploading files: Upload failed", response.Message);
        //     _unitOfWorkMock.Verify(uow => uow.InboundRequestRepository.CreateAsync(It.IsAny<InboundRequest>()), Times.Never);
        // }

        [Fact]
        public async Task GetInboundRequestById_NotFound_ThrowsException()
        {
            // Arrange
            var inboundRequestId = 1;
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequest, bool>>>()))
                .Returns(new List<InboundRequest>().AsQueryable().BuildMock());

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _inboundRequestService.GetInboundRequestById(inboundRequestId));
        }

        [Fact]
        public async Task GetInboundRequestById_Found_ReturnsMappedResult()
        {
            // Arrange
            var inboundRequestId = 1;
            var createdAt = SystemClock.Instance.GetCurrentInstant();
            var inboundRequest = new InboundRequest
            {
                InboundRequestId = inboundRequestId,
                InboundRequestCode = "IRC123",
                CreatedAt = createdAt,
                InboundRequestDetails = new List<InboundRequestDetails>
        {
            new InboundRequestDetails { Product = new Product() }
        }
            };
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequest, bool>>>()))
                .Returns(new List<InboundRequest> { inboundRequest }.AsQueryable().BuildMock());

            // Act
            var result = await _inboundRequestService.GetInboundRequestById(inboundRequestId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdAt.ToString("dd/MM/yyyy HH:mm", null), result.CreateDate);
        }

        [Fact]
        public async Task GetInboundRequestsPaginatedAsync_WithFilters_ReturnsPaginatedResult()
        {
            // Arrange
            var pattern = InstantPattern.ExtendedIso;
            var instant = SystemClock.Instance.GetCurrentInstant();
            var requests = new List<InboundRequest>
        {
            new InboundRequest
            {
                InboundRequestId = 1,
                InboundRequestCode = "REQ001",
                Status = InboundRequestStatus.WaitingForDirectorApproval,
                CreatedAt = instant,
                Assets = new List<Asset>(),
                InboundRequestDetails = new List<InboundRequestDetails> { new InboundRequestDetails { Product = new Product() } }
            },
            new InboundRequest
            {
                InboundRequestId = 2,
                InboundRequestCode = "REQ002",
                Status = InboundRequestStatus.Completed,
                CreatedAt = instant.Plus(Duration.FromDays(1)),
                Assets = new List<Asset>(),
                InboundRequestDetails = new List<InboundRequestDetails> { new InboundRequestDetails { Product = new Product() } }
            }
        };

            // Use MockQueryable to create a mock IQueryable that supports async operations
            var mockQueryable = requests.AsQueryable().BuildMockDbSet();

            var inboundRequestRepositoryMock = new Mock<IInboundRequestRepository>();
            inboundRequestRepositoryMock.Setup(r => r.GetAll()).Returns(mockQueryable.Object);
            _unitOfWorkMock.Setup(u => u.InboundRequestRepository).Returns(inboundRequestRepositoryMock.Object);

            var request = new InboundRequestQueryPaging
            {
                Page = 1,
                PageSize = 1,
                Search = "REQ001",
                InboundRequestStatus = InboundRequestStatus.WaitingForDirectorApproval,
                DateFrom = pattern.Format(instant.Minus(Duration.FromDays(1))),
                DateTo = pattern.Format(instant.Plus(Duration.FromDays(1)))
            };

            // Act
            var result = await _inboundRequestService.GetInboundRequestsPaginatedAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.CurrentPage);
            Assert.Equal(1, result.PageSize);

            var item = result.Items.First();
            Assert.Equal("REQ001", item.InboundRequestCode);
            Assert.Equal(InboundRequestStatus.WaitingForDirectorApproval.ToString(), item.Status);

        }

        // [Fact]
        // public async Task UpdateInboundRequest_AccountNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderRequest { InboundOrderId = 1 };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync((Account)null);

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Equal("Account not found", response.Message);
        // }

        // [Fact]
        // public async Task UpdateInboundRequest_InboundNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderRequest { InboundOrderId = 1 };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(new Account { Id = accountId });
        //     _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequest, bool>>>()))
        //         .Returns(new List<InboundRequest>().AsQueryable().BuildMock());

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Equal("Inbound Request not found", response.Message);
        // }

        // [Fact]
        // public async Task UpdateInboundRequest_CompletedStatus_ReturnsCannotUpdate()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderRequest { InboundOrderId = 1 };
        //     var inboundRequest = new InboundRequest { InboundRequestId = 1, Status = InboundRequestStatus.Completed };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(new Account { Id = accountId });
        //     _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequest, bool>>>()))
        //         .Returns(new List<InboundRequest> { inboundRequest }.AsQueryable().BuildMock());

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequest(accountId, request);

        //     // Assert
        //     Assert.Equal(200, response.Code);
        //     Assert.Equal("Inbound Request is Completed or Cancelled that can not update", response.Message);
        // }

        public async Task UpdateInboundRequest_ValidRequest_UpdatesSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var inboundRequestId = 1;
            var account = new Account { Id = accountId };
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(100);
            fileMock.Setup(f => f.FileName).Returns("test.jpg");

            var request = new UpdateInboundOrderRequest
            {
                InboundOrderId = inboundRequestId,
                InboundRequestDetails = new List<InboundOrderDetailRequest>
        {
            new InboundOrderDetailRequest
            {
                ProductId = 1,
                Quantity = 1,
                UnitPrice = 100,
                TotalPrice = 100
            }
        },
                Images = new List<IFormFile> { fileMock.Object }
            };

            var existingInboundRequest = new InboundRequest
            {
                InboundRequestId = inboundRequestId,
                Status = InboundRequestStatus.WaitingForDirectorApproval,
                Assets = new List<Asset>(),
                InboundRequestDetails = new List<InboundRequestDetails>()
            };

            // Mock GetByWhere to return a Task with the filtered result directly
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(account);
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequest, bool>>>()))
                .Returns<Expression<Func<InboundRequest, bool>>>(predicate =>
                {
                    // Simulate filtering by applying the predicate to the in-memory list
                    var compiledPredicate = predicate.Compile();
                    var filteredList = new List<InboundRequest> { existingInboundRequest }
                        .Where(compiledPredicate)
                        .AsQueryable();
                    return filteredList;
                });
            _unitOfWorkMock.Setup(uow => uow.InboundRequestDetailsRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequestDetails, bool>>>()))
                .Returns(new List<InboundRequestDetails>().AsQueryable());
            _unitOfWorkMock.Setup(uow => uow.InboundRequestDetailsRepository.DeleteRangeAsync(It.IsAny<IEnumerable<InboundRequestDetails>>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.InboundRequestAssetsRepository.GetByWhere(It.IsAny<Expression<Func<InboundRequestAssets, bool>>>()))
                .Returns(new List<InboundRequestAssets>().AsQueryable());
            _unitOfWorkMock.Setup(uow => uow.InboundRequestAssetsRepository.DeleteRangeAsync(It.IsAny<IEnumerable<InboundRequestAssets>>()))
                .Returns(Task.CompletedTask);
            _minioServiceMock.Setup(m => m.FileUpload(It.IsAny<string>(), It.IsAny<IFormFile>(), It.IsAny<string>(), null))
                .ReturnsAsync(new FileUploadResponse { Extension = ".jpg" });
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.UpdateAsync(It.IsAny<InboundRequest>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _inboundRequestService.UpdateInboundRequest(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Inbound Request updated successfully", response.Message);
            _unitOfWorkMock.Verify(uow => uow.InboundRequestRepository.UpdateAsync(It.Is<InboundRequest>(ir => ir.Price == 100 && ir.Assets.Count == 1)), Times.Once());
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once());
        }

        // [Fact]
        // public async Task UpdateInboundRequestStatus_AccountNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderStatusRequest { InboundId = 1 };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync((Account)null);

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequestStatus(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Equal("Account not found", response.Message);
        // }

        // [Fact]
        // public async Task UpdateInboundRequestStatus_InboundNotFound_ReturnsNotFound()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderStatusRequest { InboundId = 1 };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(new Account { Id = accountId });
        //     _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByIdAsync(request.InboundId))
        //         .ReturnsAsync((InboundRequest)null);

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequestStatus(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Equal("Inbound Request not found", response.Message);
        // }

        // [Fact]
        // public async Task UpdateInboundRequestStatus_InvalidStatus_ReturnsInvalidStatus()
        // {
        //     // Arrange
        //     var accountId = Guid.NewGuid();
        //     var request = new UpdateInboundOrderStatusRequest
        //     {
        //         InboundId = 1,
        //         InboundOrderStatus = (InboundRequestStatus)999 // Invalid status
        //     };
        //     var inboundRequest = new InboundRequest { InboundRequestId = 1 };
        //     _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
        //         .ReturnsAsync(new Account { Id = accountId });
        //     _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByIdAsync(request.InboundId))
        //         .ReturnsAsync(inboundRequest);

        //     // Act
        //     var response = await _inboundRequestService.UpdateInboundRequestStatus(accountId, request);

        //     // Assert
        //     Assert.Equal(404, response.Code);
        //     Assert.Contains("Invalid inbound request status", response.Message);
        // }

        [Fact]
        public async Task UpdateInboundRequestStatus_ValidStatus_UpdatesSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new UpdateInboundOrderStatusRequest
            {
                InboundId = 1,
                InboundOrderStatus = InboundRequestStatus.InProgress
            };
            var inboundRequest = new InboundRequest { InboundRequestId = 1, Status = InboundRequestStatus.WaitingForAccountantApproval };
            _unitOfWorkMock.Setup(uow => uow.AccountRepository.GetByIdAsync(accountId))
                .ReturnsAsync(new Account { Id = accountId });
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.GetByIdAsync(request.InboundId))
                .ReturnsAsync(inboundRequest);
            _unitOfWorkMock.Setup(uow => uow.InboundRequestRepository.UpdateAsync(It.IsAny<InboundRequest>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            var response = await _inboundRequestService.UpdateInboundRequestStatus(accountId, request);

            // Assert
            Assert.Equal(200, response.Code);
            Assert.Equal("Inbound Request updated status successfully", response.Message);
            Assert.Equal(InboundRequestStatus.InProgress, inboundRequest.Status);
            _unitOfWorkMock.Verify(uow => uow.InboundRequestRepository.UpdateAsync(It.IsAny<InboundRequest>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        }
    }

    public class TestNotificationService
{
    public List<(string Role, string Message)> Notifications { get; } = new List<(string, string)>();

    public Task NotifyRoleAsync(string role, string message)
    {
        Notifications.Add((role, message));
        return Task.CompletedTask;
    }
}
}
