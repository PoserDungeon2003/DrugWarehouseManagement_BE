using DrugWarehouseManagement.Common.Enums;
using DrugWarehouseManagement.Repository.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewAccount
    {
        public Guid AccountId { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public Instant? LastLogin { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string Status { get; set; } = null!;
        public AccountSettings? AccountSettings { get; set; }
    }
}
