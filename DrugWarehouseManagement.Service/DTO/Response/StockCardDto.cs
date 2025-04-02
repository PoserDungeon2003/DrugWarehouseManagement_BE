using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class StockCardDto
    {
        public DateTime TransactionDate { get; set; }
        public string DocumentNumber { get; set; } = "";
        public string PartnerName { get; set; } = "";
        public string Note { get; set; } = "";
        public int QuantityIn { get; set; }
        public int QuantityOut { get; set; }
        public int BeginningBalance { get; set; } 
        public int EndingBalance { get; set; }    
    }
    public class StockCardLine
    {
        public DateTime Date { get; set; }
        public string DocumentNumber { get; set; } = "";
        public string PartnerName { get; set; } = "";
        public string Note { get; set; } = "";
        public int InQty { get; set; }
        public int OutQty { get; set; }
    }
}
