using DrugWarehouseManagement.Common;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateInboundReportRequest
    {
        [Required(ErrorMessage = "InboundId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "InboundId must be a positive number.")]
        public int InboundId { get; set; }

        [Required(ErrorMessage = "ProblemDescription is required.")]
        [MaxLength(1000, ErrorMessage = "ProblemDescription must be at most 1000 characters.")]
        public string ProblemDescription { get; set; } = null!;
        public List<string> AssetFiles {  get; set; }
    }
}
