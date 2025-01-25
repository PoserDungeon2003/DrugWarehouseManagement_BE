using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateAccountSettingsRequest
    {
        public string PreferredLanguage { get; set; } = "vi";
        public bool IsTwoFactorEnabled { get; set; } = false;
    }
}
