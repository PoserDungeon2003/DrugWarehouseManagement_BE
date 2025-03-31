using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string MadeFrom { get; set; } = null!;
        //public int ProviderId { get; set; }
        public List<ProductCategoriesResponse>? Categories { get; set; }
    }

    public class ProductCategoriesResponse
    {
        public int CategoriesId { get; set; }
        public string CategoryName { get; set; } = null!;
    }
}
