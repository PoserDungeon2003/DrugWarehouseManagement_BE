using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InboundRequest : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundRequestId { get; set; }
        public string InboundRequestCode { get; set; } = null!;
        public InboundRequestStatus Status { get; set; } = InboundRequestStatus.WaitingForAccountantApproval;
        public string? Note { get; set; }
        public Guid AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual List<InboundRequestDetails> InboundRequestDetails { get; set; } = null!;

    }
}
