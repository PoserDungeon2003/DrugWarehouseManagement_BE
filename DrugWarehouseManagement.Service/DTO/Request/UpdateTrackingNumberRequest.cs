﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateTrackingNumberRequest
    {
        [Required]
        public string TrackingNumber { get; set; } = null!;
        [Required]
        public string OutboundCode { get; set; } = null!;
    }
}
