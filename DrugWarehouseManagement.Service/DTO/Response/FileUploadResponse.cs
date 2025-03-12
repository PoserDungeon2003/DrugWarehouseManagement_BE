using Microsoft.AspNetCore.Http;
using Minio.DataModel.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class FileUploadResponse
    {
        public PutObjectResponse? PutObjectResponse { get; set; }
        public string ContentType { get; set; } = null!;
        public string Extension { get; set; } = null!;
    }
}
