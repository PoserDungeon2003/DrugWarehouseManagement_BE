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

        public async Task<FileUploadResponse> FileUpload(string bucketName, IFormFile file, string fileName, string? contentType = null)
        {
            // Make a bucket on the server, if not already present.
            try
            {
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
                    .WithContentType(contentType ?? file.ContentType);

                var response = await _minioClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                _logger.LogInformation("Successfully uploaded " + file.FileName);
                return new FileUploadResponse
                {
                    PutObjectResponse = response,
                    ContentType = contentType ?? file.ContentType,
                    Extension = file.FileName.Split('.').Last()
                };

            }
            catch (System.Exception)
            {
                throw new Exception("File upload failed. Please try again.");
            }
        }

        public async Task<MemoryStream?> GetFileAsync(string bucketName, string objectName)
        {
            try
            {
                var ms = new MemoryStream();
                var args = new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream(async stream =>
                    {
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;
                    });

                await _minioClient.GetObjectAsync(args);
                return ms;
                // // Get object stats to determine size
                // var statArgs = new StatObjectArgs()
                //     .WithBucket(bucketName)
                //     .WithObject(objectName);

                // var stats = await _minioClient.StatObjectAsync(statArgs);
                // var fileSize = stats.Size;

                // // Set up chunk sizes for parallel downloading
                // int chunkSize = 5 * 1024 * 1024; // 5MB chunks
                // int numChunks = (int)Math.Ceiling((double)fileSize / chunkSize);

                // var finalStream = new MemoryStream(new byte[fileSize]);
                // var tasks = new List<Task>();

                // for (int i = 0; i < numChunks; i++)
                // {
                //     int chunkIndex = i;
                //     long startPosition = chunkIndex * chunkSize;
                //     long length = Math.Min(chunkSize, fileSize - startPosition);

                //     tasks.Add(Task.Run(async () =>
                //     {
                //         var chunkStream = new MemoryStream();
                //         var getArgs = new GetObjectArgs()
                //             .WithBucket(bucketName)
                //             .WithObject(objectName)
                //             .WithCallbackStream(async stream =>
                //             {
                //                 await stream.CopyToAsync(chunkStream);
                //                 chunkStream.Position = 0;
                //             })
                //             .WithOffsetAndLength(startPosition, length);  // Using WithOffsetAndLength instead

                //         await _minioClient.GetObjectAsync(getArgs);

                //         // Copy to the correct position in the final stream
                //         lock (finalStream)
                //         {
                //             finalStream.Position = startPosition;
                //             chunkStream.CopyTo(finalStream);
                //         }
                //     }));
                // }

                // await Task.WhenAll(tasks);
                // finalStream.Position = 0;
                // return finalStream;
            }
            catch (Exception)
            {
                throw new Exception("File not found or error occurred while retrieving the file.");
            }

        }
    }
}
