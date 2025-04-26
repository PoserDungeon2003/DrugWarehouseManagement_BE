using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Repository.Models;
using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using DrugWarehouseManagement.Service.Services;
using DrugWarehouseManagement.Service.Wrapper.Interface;
using Google.Authenticator;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MockQueryable;
using Moq;
using NodaTime;
using System.Linq.Expressions;

namespace DrugWarehouseManagement.UnitTest
{
  public class DeviceServiceTests
  {
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeviceService _deviceService;

    public DeviceServiceTests()
    {
      _unitOfWorkMock = new Mock<IUnitOfWork>();
      _deviceService = new DeviceService(_unitOfWorkMock.Object);
    }

    #region DeleteDevice Tests

    [Fact]
    public async Task DeleteDevice_AccountNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var deviceId = 1;

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync((Account)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.DeleteDevice(accountId, deviceId)
      );
      Assert.Equal("Account not found", exception.Message);
    }

    [Fact]
    public async Task DeleteDevice_DeviceNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var deviceId = 1;
      var account = new Account { Id = accountId };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByIdAsync(deviceId))
          .ReturnsAsync((Device)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.DeleteDevice(accountId, deviceId)
      );
      Assert.Equal("Device not found", exception.Message);
    }

    [Fact]
    public async Task DeleteDevice_DeviceAlreadyInactive_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var deviceId = 1;
      var account = new Account { Id = accountId };
      var device = new Device
      {
        DeviceId = deviceId,
        Status = DeviceStatus.Inactive
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByIdAsync(deviceId))
          .ReturnsAsync(device);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.DeleteDevice(accountId, deviceId)
      );
      Assert.Equal("Device is already inactive", exception.Message);
    }

    [Fact]
    public async Task DeleteDevice_ValidRequest_ReturnsSuccessResponse()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var deviceId = 1;
      var account = new Account { Id = accountId };
      var device = new Device
      {
        DeviceId = deviceId,
        Status = DeviceStatus.Active
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByIdAsync(deviceId))
          .ReturnsAsync(device);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.UpdateAsync(It.IsAny<Device>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _deviceService.DeleteDevice(accountId, deviceId);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Device deleted successfully", result.Message);
      Assert.Equal(DeviceStatus.Inactive, device.Status);
      _unitOfWorkMock.Verify(u => u.DeviceRepository.UpdateAsync(device), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region GetDevices Tests

    [Fact]
    public async Task GetDevices_ValidRequest_ReturnsPaginatedResult()
    {
      // Arrange
      var queryPaging = new QueryPaging
      {
        Page = 1,
        PageSize = 10,
        Search = "test"
      };

      var devices = new List<Device>
            {
                new Device
                {
                    DeviceId = 1,
                    DeviceName = "Test Device 1",
                    DeviceType = "Sensor",
                    CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                    Account = new Account { UserName = "user1" }
                },
                new Device
                {
                    DeviceId = 2,
                    DeviceName = "Test Device 2",
                    DeviceType = "Controller",
                    CreatedAt = SystemClock.Instance.GetCurrentInstant(),
                    Account = new Account { UserName = "user2" }
                }
            }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetAll())
          .Returns(devices);

      // Act
      var result = await _deviceService.GetDevices(queryPaging);

      // Assert
      Assert.Equal(2, result.Items.Count);
      Assert.Equal(1, result.CurrentPage);
      Assert.Equal(10, result.PageSize);
      Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task GetDevices_InvalidDateFormat_ThrowsException()
    {
      // Arrange
      var queryPaging = new QueryPaging
      {
        Page = 1,
        PageSize = 10,
        Search = "",
        DateFrom = "invalid-date-format"
      };

      var devices = new List<Device>().AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetAll())
          .Returns(devices);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.GetDevices(queryPaging)
      );
      Assert.Equal("DateFrom is invalid ISO format", exception.Message);
    }

    #endregion

    #region Ping Tests

    [Fact]
    public async Task Ping_DeviceNotFound_ThrowsException()
    {
      // Arrange
      string apiKey = "non-existent-key";
      var mockDevices = new List<Device>().AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.Ping(apiKey)
      );
      Assert.Equal("Device not found", exception.Message);
    }

    [Fact]
    public async Task Ping_ValidApiKey_ReturnsSuccessResponse()
    {
      // Arrange
      string apiKey = "valid-api-key";
      var device = new Device { ApiKey = apiKey };
      var mockDevices = new List<Device> { device }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);

      // Act
      var result = await _deviceService.Ping(apiKey);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Device is online", result.Message);
    }

    #endregion

    #region RegisterDevice Tests

    [Fact]
    public async Task RegisterDevice_ValidRequest_ReturnsSuccessWithApiKey()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new RegisterDeviceRequest
      {
        DeviceName = "Test Device",
        DeviceType = "Sensor"
      };

      // Capture the created device to verify its properties later
      Device capturedDevice = null;

      _unitOfWorkMock.Setup(u => u.DeviceRepository.CreateAsync(It.IsAny<Device>()))
          .Callback<Device>(d => capturedDevice = d)
          .Returns(Task.CompletedTask);

      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _deviceService.RegisterDevice(accountId, request);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Device registered successfully", result.Message);
      Assert.NotNull(result.Result);

      // Convert the result to Dictionary to safely access the ApiKey
      var resultDict = result.Result.GetType()
          .GetProperties()
          .ToDictionary(p => p.Name, p => p.GetValue(result.Result));

      Assert.True(resultDict.ContainsKey("ApiKey"));
      Assert.NotNull(resultDict["ApiKey"]);

      // Also verify the captured device has the ApiKey set
      Assert.NotNull(capturedDevice);
      Assert.NotNull(capturedDevice.ApiKey);
      Assert.Equal(accountId, capturedDevice.AccountId);

      _unitOfWorkMock.Verify(u => u.DeviceRepository.CreateAsync(It.IsAny<Device>()), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
    
    #endregion

    #region UpdateDevice Tests

    [Fact]
    public async Task UpdateDevice_AccountNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new UpdateDeviceRequest { DeviceId = 1 };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync((Account)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.UpdateDevice(accountId, request)
      );
      Assert.Equal("Account not found", exception.Message);
    }

    [Fact]
    public async Task UpdateDevice_DeviceNotFound_ThrowsException()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new UpdateDeviceRequest { DeviceId = 1 };
      var account = new Account { Id = accountId };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByIdAsync(request.DeviceId))
          .ReturnsAsync((Device)null);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.UpdateDevice(accountId, request)
      );
      Assert.Equal("Device not found", exception.Message);
    }

    [Fact]
    public async Task UpdateDevice_ValidRequest_UpdatesDeviceSuccessfully()
    {
      // Arrange
      var accountId = Guid.NewGuid();
      var request = new UpdateDeviceRequest
      {
        DeviceId = 1,
        DeviceName = "Updated Device",
        DeviceType = "Updated Type"
      };
      var account = new Account { Id = accountId };
      var device = new Device
      {
        DeviceId = 1,
        DeviceName = "Original Device",
        DeviceType = "Original Type"
      };

      _unitOfWorkMock.Setup(u => u.AccountRepository.GetByIdAsync(accountId))
          .ReturnsAsync(account);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByIdAsync(request.DeviceId))
          .ReturnsAsync(device);
      _unitOfWorkMock.Setup(u => u.DeviceRepository.UpdateAsync(It.IsAny<Device>()))
          .Returns(Task.CompletedTask);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _deviceService.UpdateDevice(accountId, request);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Device updated successfully", result.Message);
      Assert.Equal("Updated Device", device.DeviceName);
      Assert.Equal("Updated Type", device.DeviceType);
      Assert.NotNull(device.UpdatedAt);
      _unitOfWorkMock.Verify(u => u.DeviceRepository.UpdateAsync(device), Times.Once);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region UpdateTrackingNumber Tests

    [Fact]
    public async Task UpdateTrackingNumber_DeviceNotFound_ThrowsException()
    {
      // Arrange
      string apiKey = "non-existent-key";
      var request = new UpdateTrackingNumberRequest
      {
        OutboundCode = "OB001",
        TrackingNumber = "TRK123"
      };
      var mockDevices = new List<Device>().AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.UpdateTrackingNumber(apiKey, request)
      );
      Assert.Equal("Device not found", exception.Message);
    }

    [Fact]
    public async Task UpdateTrackingNumber_DeviceExpired_ThrowsException()
    {
      // Arrange
      string apiKey = "valid-key";
      var request = new UpdateTrackingNumberRequest
      {
        OutboundCode = "OB001",
        TrackingNumber = "TRK123"
      };
      var device = new Device
      {
        ApiKey = apiKey,
        Status = DeviceStatus.Active,
        IsRevoked = false,
        ExpiryDate = DateTime.UtcNow.AddDays(-1) // Expired
      };
      var mockDevices = new List<Device> { device }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.UpdateTrackingNumber(apiKey, request)
      );
      Assert.Equal("Device is expired", exception.Message);
    }

    [Fact]
    public async Task UpdateTrackingNumber_OutboundNotFound_ThrowsException()
    {
      // Arrange
      string apiKey = "valid-key";
      var request = new UpdateTrackingNumberRequest
      {
        OutboundCode = "OB001",
        TrackingNumber = "TRK123"
      };
      var device = new Device
      {
        ApiKey = apiKey,
        Status = DeviceStatus.Active,
        IsRevoked = false,
        ExpiryDate = null
      };
      var mockDevices = new List<Device> { device }.AsQueryable().BuildMock();
      var mockOutbounds = new List<Outbound>().AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);
      _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
          .Returns(mockOutbounds);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Exception>(
          () => _deviceService.UpdateTrackingNumber(apiKey, request)
      );
      Assert.Equal("Outbound not found", exception.Message);
    }

    [Fact]
    public async Task UpdateTrackingNumber_ValidRequest_UpdatesTrackingNumberSuccessfully()
    {
      // Arrange
      string apiKey = "valid-key";
      var request = new UpdateTrackingNumberRequest
      {
        OutboundCode = "OB001",
        TrackingNumber = "TRK123"
      };
      var device = new Device
      {
        ApiKey = apiKey,
        Status = DeviceStatus.Active,
        IsRevoked = false,
        ExpiryDate = null
      };
      var outbound = new Outbound { OutboundCode = "OB001" };

      var mockDevices = new List<Device> { device }.AsQueryable().BuildMock();
      var mockOutbounds = new List<Outbound> { outbound }.AsQueryable().BuildMock();

      _unitOfWorkMock.Setup(u => u.DeviceRepository.GetByWhere(It.IsAny<Expression<Func<Device, bool>>>()))
          .Returns(mockDevices);
      _unitOfWorkMock.Setup(u => u.OutboundRepository.GetByWhere(It.IsAny<Expression<Func<Outbound, bool>>>()))
          .Returns(mockOutbounds);
      _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
          .Returns(Task.CompletedTask);

      // Act
      var result = await _deviceService.UpdateTrackingNumber(apiKey, request);

      // Assert
      Assert.Equal(200, result.Code);
      Assert.Equal("Tracking number updated successfully", result.Message);
      Assert.Equal("TRK123", outbound.TrackingNumber);
      _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    #endregion
  }
}