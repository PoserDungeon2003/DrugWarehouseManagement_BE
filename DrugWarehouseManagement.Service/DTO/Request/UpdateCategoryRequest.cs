using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateCategoryRequest
    {
        [Required]
        public int CategoriesId { get; set; }
        public string? CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? Description { get; set; }
    }
}
