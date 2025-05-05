using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        public InventoryService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<List<LotAlert>> CheckLowStockAndExpiryAsync()
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var lots = await _unitOfWork.LotRepository
                .GetAll()
                .ToListAsync();

            var alerts = new List<LotAlert>();

            foreach (var lot in lots)
            {
                // 1) Kiểm tra Low Stock (< 30)
                if (lot.Quantity < 30)
                {
                    alerts.Add(new LotAlert
                    {
                        LotId = lot.LotId,
                        LotNumber = lot.LotNumber,
                        AlertType = "Gần hết hàng",
                        Message = $"sắp hết hàng ({lot.Quantity})."
                    });
                }
                // 2) Kiểm tra Expiry
                // 2.1) Còn dưới 12 tháng
                var dt = lot.ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var expiryDateInstant = Instant.FromDateTimeUtc(dt);                  
                var timeLeft = expiryDateInstant - now;
                var twelveMonths = Duration.FromDays(365);

                if (timeLeft < twelveMonths)
                {
                    alerts.Add(new LotAlert
                    {
                        LotId = lot.LotId,
                        LotNumber = lot.LotNumber,
                        AlertType = "HSD còn 12 tháng",
                        Message = "sẽ hết hạn trong 12 tháng."
                    });
                }
                // 2.2) Đã sử dụng 60% thời gian
                // Tính total shelf life = ExpiryDate - ManufactureDate
                // Tính used life = now - ManufactureDate
                // Tính phần trăm used = used life / total shelf life
                if (lot.ManufacturingDate != default)
                {
                    var manufactureDt = lot.ManufacturingDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                    var manufactureDateInstant = Instant.FromDateTimeUtc(manufactureDt);
                    var totalShelfLife = expiryDateInstant - manufactureDateInstant;
                    var usedLife = now - manufactureDateInstant;

                    // Chỉ check nếu totalShelfLife > 0
                    if (totalShelfLife > Duration.Zero)
                    {
                        var percentUsed = usedLife / totalShelfLife; // = 0.xxxx
                        if (percentUsed >= 0.6) // 60%
                        {
                            alerts.Add(new LotAlert
                            {
                                LotId = lot.LotId,
                                LotNumber = lot.LotNumber,
                                AlertType = "Quá 60% HSD",
                                Message = "đã quá 60% thời hạn sử dụng."
                            });
                        }
                    }
                }
            }
            return alerts;
        }

        public async Task NotifyLowStockAndExpiryAsync()
        {
            var alerts = await CheckLowStockAndExpiryAsync();
            if (!alerts.Any()) return; 
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Danh sách cảnh báo lô hàng:");
            foreach (var alert in alerts)
            {
                messageBuilder.AppendLine(
         $"- Lô: {alert.LotNumber}, " +
         $"Lí do: {alert.AlertType}, " +
         $"{alert.Message}"
     );
            }
    
            string botToken = _configuration["Telegram:BotToken"];
            string chatId = _configuration["Telegram:ChatId"];

            await Utils.SendTelegramMessageAsync(botToken, chatId, messageBuilder.ToString());
        }
    }
}