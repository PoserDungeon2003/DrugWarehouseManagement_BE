using DrugWarehouseManagement.Service.DTO.Request;
using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class FirebaseService : IFirebaseService
    {
        public async Task<BaseResponse> SendNotificationAsync(FirebaseSendNotificationRequest request)
        {
            if (string.IsNullOrEmpty(request.DeviceToken))
                throw new ArgumentNullException(nameof(request.DeviceToken));

            var message = new Message
            {
                Token = request.DeviceToken,
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                },
                Data = request.Data,
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            return new BaseResponse
            {
                Code = (int)HttpStatusCode.OK,
                Message = "Notification sent successfully",
                Details = response,
            };

        }
    }
}
