namespace DrugWarehouseManagement.Service.DTO.Request;

public class CategoriesQueryPaging : QueryPaging
{
  public bool? IsMainCategory { get; set; }
}