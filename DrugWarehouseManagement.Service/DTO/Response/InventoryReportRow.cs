using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
   public class InventoryReportRow
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }           // Đơn vị tính
        public int Beginning { get; set; }        // Đầu kỳ
        public int BuyQty { get; set; }           // Mua
        public int TransferInQty { get; set; }    // Chuyển (nhập)
        public int ReturnInQty { get; set; }      // Trả về (nhập)
        public int SellQty { get; set; }          // Bán
        public int TransferOutQty { get; set; }   // Chuyển (xuất)
        public int ReturnOutQty { get; set; }     // Trả về (xuất)
        public int Remain { get; set; }           // Tồn

        public int LostQty { get; set; }         // Tổng số mất đã phát hiện
        public int FoundQty { get; set; }        // Tổng số đã tìm thấy lại
        public int NetLostQty => LostQty - FoundQty;   // Còn mất
        public int SampleExportQty { get; set; }
    }
}
