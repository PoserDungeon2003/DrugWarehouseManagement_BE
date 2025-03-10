using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Service.DTO.Response;
using Microsoft.AspNetCore.Http;
using Minio.DataModel.Response;

namespace DrugWarehouseManagement.Service.Interface
{
    public interface IMinioService
    {
        public Task<FileUploadResponse> FileUpload(string bucketName, IFormFile file, string fileName);
        public Task<MemoryStream?> GetFileAsync(string bucketName, string objectName);
    }
}
