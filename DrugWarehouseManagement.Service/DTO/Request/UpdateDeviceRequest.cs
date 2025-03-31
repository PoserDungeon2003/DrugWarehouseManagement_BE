using System.ComponentModel.DataAnnotations;
using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Service.DTO.Request;

public class UpdateDeviceRequest
{
  [Required]
  public int DeviceId { get; set; }
  public string? DeviceName { get; set; }
  public string? DeviceType { get; set; }
  public string? SerialNumber { get; set; }
  public DateTime? ExpiryDate { get; set; }
  public DeviceStatus? Status { get; set; }
  public bool? IsRevoked { get; set; }
}