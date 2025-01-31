using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateAccountSettingsRequest
    {
        [RegularExpression(@"^[a-zA-Z]{2}$", ErrorMessage = "Preferred language must be exactly 2 alphabetic characters")]
        public string PreferredLanguage { get; set; } = "vi";
    }
}
