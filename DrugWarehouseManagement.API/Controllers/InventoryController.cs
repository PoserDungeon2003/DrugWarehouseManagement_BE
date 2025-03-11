using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }
        [HttpGet("test-inventory")]
        public async Task<IActionResult> TestInventory()
        {
            await _inventoryService.NotifyLowStockAndExpiryAsync();
            return Ok("Done");
        }
    }
}
