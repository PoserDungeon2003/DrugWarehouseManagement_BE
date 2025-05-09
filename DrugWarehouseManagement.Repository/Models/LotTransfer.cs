﻿using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class LotTransfer : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LotTransferId { get; set; }
        public string LotTransferCode { get; set; } = null!;
        public LotTransferStatus LotTransferStatus { get; set; } = LotTransferStatus.Pending;
        public int FromWareHouseId { get; set; }
        public int ToWareHouseId { get; set; }
        public Guid AccountId { get; set; }
        public int? AssetId { get; set; }

        [ForeignKey("FromWareHouseId")]
        public virtual Warehouse FromWareHouse { get; set; } = null!;
        [ForeignKey("ToWareHouseId")]
        public virtual Warehouse ToWareHouse { get; set; } = null!;
        public virtual List<LotTransferDetail> LotTransferDetails { get; set; } = null!;
        public virtual Asset? Asset { get; set; }
        public virtual Account Account { get; set; } = null!;
    }
}
