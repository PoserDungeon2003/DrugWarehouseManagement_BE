using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewCategories
    {
        public int CategoriesId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public List<ViewSubCategories>? SubCategories { get; set; }
    }

    public class ViewSubCategories
    {
        public int CategoriesId { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
