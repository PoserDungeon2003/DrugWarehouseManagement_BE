using DrugWarehouseManagement.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateInboundReportRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "InboundReportId must be greater than 0.")]
        public int InboundReportId { get; set; }

        [Required(ErrorMessage = "ProblemDescription is required.")]
        [MaxLength(1000, ErrorMessage = "ProblemDescription must be at most 1000 characters.")]
        public string ProblemDescription { get; set; } = null!;
        public InboundReportStatus InboundReportStatus { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
