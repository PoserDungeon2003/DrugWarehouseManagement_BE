using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateCategoryRequest
    {
        [Required]
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<CreateSubCategories>? SubCategories { get; set; }
    }

    public class CreateSubCategories
    {
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
