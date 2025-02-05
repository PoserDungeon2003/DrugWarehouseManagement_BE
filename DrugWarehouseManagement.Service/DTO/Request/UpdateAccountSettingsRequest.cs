using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateAccountSettingsRequest
    {
        [RegularExpression(@"^[a-zA-Z]{2}$", ErrorMessage = "Preferred language must be exactly 2 alphabetic characters")]
        public string PreferredLanguage { get; set; } = "vi";
    }
}
