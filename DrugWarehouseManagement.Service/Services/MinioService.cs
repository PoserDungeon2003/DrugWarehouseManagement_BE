using DrugWarehouseManagement.Service.DTO.Response;
using DrugWarehouseManagement.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<IMinioService> _logger;

        public MinioService(IMinioClient minioClient, ILogger<IMinioService> logger)
        {
            _minioClient = minioClient;
            _logger = logger;
        }

        public async Task<FileUploadResponse> FileUpload(string bucketName, IFormFile file, string fileName)
        {
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
                // Upload a file to bucket.
                using var stream = file.OpenReadStream();
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithObjectSize(file.Length)
                    .WithStreamData(stream)
                    .WithContentType(file.ContentType);

                var response = await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                _logger.LogInformation("Successfully uploaded " + file.FileName);
                return new FileUploadResponse
                {
                    PutObjectResponse = response,
                    ContentType = file.ContentType,
                    Extension = file.FileName.Split('.').Last()
                };
            }
            catch (MinioException e)
            {
                _logger.LogError("File Upload Error: {0}", e.Message);
                return null;
            }
        }

        public async Task<MemoryStream?> GetFileAsync(string bucketName, string objectName)
        {
            var memoryStream = new MemoryStream();

            try
            {
                // Download file from MinIO and copy to memory stream
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(async stream => await stream.CopyToAsync(memoryStream));

                await _minioClient.GetObjectAsync(args);

                // Reset stream position before returning
                memoryStream.Position = 0;
                return memoryStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving file from MinIO: {ex.Message}");
                return null;
            }
        }
    }
}
