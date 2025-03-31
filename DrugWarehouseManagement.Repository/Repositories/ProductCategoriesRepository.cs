using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Repository.Interface;
using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Repository.Repositories
{
    public class ProductCategoriesRepository : GenericRepository<ProductCategories>, IProductCategoriesRepository
    {
        public ProductCategoriesRepository(DrugWarehouseContext context) : base(context)
        {
        }
    }
}
