using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Service.DTO.Request;
public class LotTransferQueryPaging : QueryPaging
{
  public LotTransferStatus? Status { get; set; }
}