using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Security.Claims;

namespace DrugWarehouseManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboundController : ControllerBase
    {
        private readonly IOutboundService _outboundService;

        public OutboundController(IOutboundService outboundService)
        {
            _outboundService = outboundService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOutbound([FromBody] CreateOutboundRequest request)
        {
            try
            {
                var accountId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var response = await _outboundService.CreateOutbound(accountId, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateOutbound(int id, [FromBody] UpdateOutboundRequest request)
        {
            try
            {
                var response = await _outboundService.UpdateOutbound(id, request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message,
                });
            }
        }

        /// <summary>
        /// Searches outbounds by OutboundId or OutboundCode using pagination.
        /// Example: GET api/outbounds/search?page=1&pageSize=10&search=OUTB-1234
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchOutbounds([FromQuery] QueryPaging queryPaging)
        {
            var result = await _outboundService.SearchOutboundsAsync(queryPaging);
            return Ok(result);
        }

        [HttpGet("export/{id}")]
        public async Task<IActionResult> ExportOutboundInvoice(int id)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                var outbound = await _outboundService.GetOutboundByIdWithDetailsAsync(id);
                if (outbound == null)
                {
                    return NotFound(new BaseResponse
                    {
                        Code = 404,
                        Message = "Outbound not found."
                    });
                }
                // Create file excel in memory
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("PhieuGiaoNhan");
                // Cấu hình font, cỡ chữ chung
                worksheet.Cells.Style.Font.Name = "Times New Roman";
                worksheet.Cells.Style.Font.Size = 11;

                // Phần header công ty, khách hàng
                worksheet.Cells["A1"].Value = "CÔNG TY TNHH DƯỢC PHẨM TRUNG HẠNH";
                worksheet.Cells["A2"].Value = "Đ/c: 2/35 Châu Hưng, P.6, Quận Tân Bình, Tp.Hồ Chí Minh";
                worksheet.Cells["A3"].Value = "ĐT: 0993 129 300";

                // Tiêu đề phiếu
                worksheet.Cells["A5:H5"].Merge = true;
                worksheet.Cells["A5"].Value = "PHIẾU GIAO NHẬN / PHIẾU BÁN HÀNG";
                worksheet.Cells["A5"].Style.Font.Bold = true;
                worksheet.Cells["A5"].Style.Font.Size = 16;
                worksheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Thông tin khách hàng
                worksheet.Cells["A7"].Value = "Tên khách hàng:";
                worksheet.Cells["B7"].Value = outbound.Customer.CustomerName;
                worksheet.Cells["A8"].Value = "Địa chỉ:";
                worksheet.Cells["B8"].Value = outbound.Customer.Address;
                worksheet.Cells["A9"].Value = "SĐT:";
                worksheet.Cells["B9"].Value = outbound.Customer.PhoneNumber;

                // Thông tin phiếu
                worksheet.Cells["E7"].Value = "Mã phiếu:";
                worksheet.Cells["F7"].Value = outbound.OutboundCode;
                worksheet.Cells["E8"].Value = "Ngày:";
                worksheet.Cells["F8"].Value = outbound.OutboundDate?.ToDateTimeUtc().ToString("dd/MM/yyyy");

                // Tiêu đề bảng chi tiết
                int startRow = 11;
                worksheet.Cells[startRow, 1].Value = "STT";
                worksheet.Cells[startRow, 2].Value = "Tên hàng";
                worksheet.Cells[startRow, 3].Value = "Số lô";
                worksheet.Cells[startRow, 4].Value = "Hạn dùng";
                worksheet.Cells[startRow, 5].Value = "ĐVT";
                worksheet.Cells[startRow, 6].Value = "Số lượng";
                worksheet.Cells[startRow, 7].Value = "Đơn giá";
                worksheet.Cells[startRow, 8].Value = "Thành tiền";

                // Canh giữa, in đậm
                using (var range = worksheet.Cells[startRow, 1, startRow, 8])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Font.Bold = true;
                }

                // Đổ dữ liệu OutboundDetails
                int currentRow = startRow + 1;
                int stt = 1;
                foreach (var detail in outbound.OutboundDetails)
                {
                    worksheet.Cells[currentRow, 1].Value = stt; // STT
                    worksheet.Cells[currentRow, 2].Value = detail.Product?.ProductName ?? "N/A";
                    worksheet.Cells[currentRow, 3].Value = detail.LotNumber;
                    worksheet.Cells[currentRow, 4].Value = detail.ExpiryDate.ToDateTimeUtc().ToString("dd/MM/yyyy");
                    worksheet.Cells[currentRow, 5].Value = detail.UnitType;
                    worksheet.Cells[currentRow, 6].Value = detail.Quantity;
                    worksheet.Cells[currentRow, 7].Value = detail.UnitPrice;
                    worksheet.Cells[currentRow, 8].Value = detail.TotalPrice;

                    // Định dạng canh phải cho số
                    worksheet.Cells[currentRow, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    stt++;
                    currentRow++;
                }

                // Tính tổng
                worksheet.Cells[currentRow, 7].Value = "Tổng cộng:";
                worksheet.Cells[currentRow, 7].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // =SUM(H12 : H{currentRow-1})
                worksheet.Cells[currentRow, 8].Formula = $"SUM(H{(startRow + 1)}:H{(currentRow - 1)})";
                worksheet.Cells[currentRow, 8].Style.Font.Bold = true;
                worksheet.Cells[currentRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                // Auto-fit
                worksheet.Cells[1, 1, currentRow, 8].AutoFitColumns();

                // Lưu file vào MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                // Tạo tên file
                var fileName = $"PhieuGiaoNhan_{outbound.OutboundCode}.xlsx";

                // Trả về file
                return File(stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new BaseResponse
                {
                    Code = 400,
                    Message = ex.Message
                });
            }
        }
    }
}
