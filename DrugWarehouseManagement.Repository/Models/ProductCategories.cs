using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class ProductCategories
    {
        public int ProductId { get; set; }
        public int CategoriesId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public virtual Categories Categories { get; set; } = null!;
    }
}
