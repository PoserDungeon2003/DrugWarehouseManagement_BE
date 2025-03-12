using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Categories : TimeStamp
    {
        [Key]
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ParentCategoryId { get; set; }
        public string Description { get; set; } = null!;
        public CategoriesStatus Status { get; set; } = CategoriesStatus.Active;

        public Categories? ParentCategory { get; set; }
        public List<Categories> SubCategories { get; set; } = null!;
        public List<Product> Products { get; set; } = null!;
        public List<Asset> Assets { get; set; } = null!;
    }
}
