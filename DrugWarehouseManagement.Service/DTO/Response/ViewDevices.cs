using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
  public class ViewDevices
  {
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
    public string DeviceType { get; set; } = null!;
    public string ApiKey { get; set; } = null!;
    public string? ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string Status { get; set; } = null!;
    public Instant CreatedAt { get; set; }
    public Instant? UpdatedAt { get; set; }

  }
}
