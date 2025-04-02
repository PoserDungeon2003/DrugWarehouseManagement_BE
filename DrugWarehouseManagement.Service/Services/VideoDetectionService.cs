using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OpenCvSharp;
using System.Drawing;
using System.Collections.Generic;

namespace DrugWarehouseManagement.Service.Services
{
    public class VideoDetectionService
    {
        private readonly string _modelPath;
        private const int InputSize = 640;
        private const float ConfidenceThreshold = 0.1f; // Giảm để tăng khả năng phát hiện
        private const float IouThreshold = 0.6f; // Tăng để giữ lại các phát hiện gần nhau

        public VideoDetectionService(string modelPath)
        {
            _modelPath = modelPath;
        }

        public DetectionResult ProcessVideo(Stream videoStream)
        {
            if (videoStream == null || videoStream.Length == 0)
            {
                throw new ArgumentException("Video stream is null or empty.");
            }

            int totalCount = 0;
            List<Detection> allDetections = new List<Detection>();
            List<Detection> uniqueDetections = new List<Detection>(); // Để theo dõi các đối tượng duy nhất

            string tempFilePath = Path.GetTempFileName() + ".mp4";
            try
            {
                // Lưu video stream vào file tạm
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    videoStream.CopyTo(fileStream);
                }

                using (var capture = new VideoCapture(tempFilePath))
                {
                    if (!capture.IsOpened())
                    {
                        throw new Exception("Unable to open video stream.");
                    }

                    using (Mat frame = new Mat())
                    {
                        int frameCount = 0;
                        int maxFrames = (int)capture.FrameCount;
                        double fps = capture.Fps;
                        int frameInterval = (int)(fps / 10); // Xử lý 10 frame mỗi giây (giảm số frame được xử lý)

                        while (frameCount < maxFrames && capture.Read(frame))
                        {
                            if (frame.Empty())
                            {
                                frameCount++;
                                continue; // Bỏ qua frame rỗng
                            }

                            // Chỉ xử lý frame theo khoảng cách frameInterval
                            if (frameInterval == 0 || frameCount % frameInterval == 0)
                            {
                                Console.WriteLine($"Processing frame {frameCount + 1}/{maxFrames}");

                                using (Mat resizedFrame = ResizeFrame(frame))
                                {
                                    var detections = RunYOLO(resizedFrame);
                                    if (detections.Any())
                                    {
                                        totalCount += detections.Count;

                                        foreach (var detection in detections)
                                        {
                                            // Chuyển đổi bounding box từ kích thước resized (640x640) về kích thước gốc
                                            float scaleX = (float)frame.Width / InputSize;
                                            float scaleY = (float)frame.Height / InputSize;
                                            var bbox = new Rectangle(
                                                (int)(detection.BoundingBox.X * scaleX),
                                                (int)(detection.BoundingBox.Y * scaleY),
                                                (int)(detection.BoundingBox.Width * scaleX),
                                                (int)(detection.BoundingBox.Height * scaleY)
                                            );

                                            // Đảm bảo bbox nằm trong giới hạn của frame
                                            int x = Math.Max(0, bbox.X);
                                            int y = Math.Max(0, bbox.Y);
                                            int width = Math.Min(bbox.Width, frame.Width - x);
                                            int height = Math.Min(bbox.Height, frame.Height - y);
                                            if (width <= 0 || height <= 0)
                                            {
                                                Console.WriteLine($"Invalid bounding box size for detection in frame {frameCount}: width={width}, height={height}. Skipping...");
                                                continue;
                                            }
                                            bbox = new Rectangle(x, y, width, height);
                                            detection.BoundingBox = bbox; // Cập nhật bounding box đã điều chỉnh

                                            // Kiểm tra xem detection có phải là đối tượng mới không
                                            bool isNewDetection = true;
                                            foreach (var uniqueDetection in uniqueDetections)
                                            {
                                                if (ComputeIoU(detection.BoundingBox, uniqueDetection.BoundingBox) > 0.5)
                                                {
                                                    isNewDetection = false;
                                                    break;
                                                }
                                            }

                                            if (isNewDetection)
                                            {
                                                uniqueDetections.Add(detection);
                                            }

                                            allDetections.Add(detection);
                                        }
                                    }
                                }
                            }

                            frameCount++;
                            capture.Set(VideoCaptureProperties.PosFrames, frameCount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing video: {ex.Message}");
                throw;
            }
            finally
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }

            return new DetectionResult { TotalCount = uniqueDetections.Count, Detections = allDetections };
        }

        private Mat ResizeFrame(Mat frame)
        {
            using (Mat resizedFrame = new Mat())
            {
                Cv2.Resize(frame, resizedFrame, new OpenCvSharp.Size(InputSize, InputSize), interpolation: InterpolationFlags.Linear);
                return resizedFrame.Clone();
            }
        }

        private List<Detection> RunYOLO(Mat image)
        {
            using (var session = new InferenceSession(_modelPath))
            {
                var inputTensor = ConvertImageToTensor(image);
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

                using (var results = session.Run(inputs))
                {
                    var outputTensor = results.First().AsTensor<float>();
                    return ParseYOLOv8Output(outputTensor);
                }
            }
        }

        private DenseTensor<float> ConvertImageToTensor(Mat image)
        {
            using (Mat rgbImage = new Mat())
            {
                Cv2.CvtColor(image, rgbImage, ColorConversionCodes.BGR2RGB);
                using (Mat floatImage = new Mat())
                {
                    rgbImage.ConvertTo(floatImage, MatType.CV_32F, 1.0 / 255.0);

                    float[] imageData = new float[3 * InputSize * InputSize];
                    int index = 0;

                    for (int c = 0; c < 3; c++)
                    {
                        for (int h = 0; h < InputSize; h++)
                        {
                            for (int w = 0; w < InputSize; w++)
                            {
                                imageData[index++] = floatImage.At<Vec3f>(h, w)[c];
                            }
                        }
                    }

                    return new DenseTensor<float>(imageData, new[] { 1, 3, InputSize, InputSize });
                }
            }
        }

        private List<Detection> ParseYOLOv8Output(Tensor<float> output)
        {
            List<Detection> detections = new List<Detection>();
            int numProposals = output.Dimensions[2];
            int numClasses = output.Dimensions[1] - 5;

            string[] classLabels = new string[] { "person", "car" };
            int[] targetClassIndices = new int[] { 0, 2 };

            Console.WriteLine($"Output dimensions: {output.Dimensions[0]} x {output.Dimensions[1]} x {output.Dimensions[2]}");
            Console.WriteLine($"numClasses: {numClasses}, numProposals: {numProposals}");

            for (int i = 0; i < numProposals; i++)
            {
                float confidence = output[0, 4, i];
                Console.WriteLine($"Proposal {i}: confidence = {confidence}");

                if (confidence >= ConfidenceThreshold)
                {
                    float xCenter = output[0, 0, i];
                    float yCenter = output[0, 1, i];
                    float width = output[0, 2, i];
                    float height = output[0, 3, i];

                    int x = (int)(xCenter - width / 2);
                    int y = (int)(yCenter - height / 2);
                    int w = (int)width;
                    int h = (int)height;

                    float maxClassScore = 0;
                    int maxClassIndex = 0;
                    for (int c = 0; c < numClasses; c++)
                    {
                        float classScore = output[0, 5 + c, i];
                        if (classScore > maxClassScore)
                        {
                            maxClassScore = classScore;
                            maxClassIndex = c;
                        }
                    }

                    Console.WriteLine($"Proposal {i}: maxClassIndex = {maxClassIndex}, maxClassScore = {maxClassScore}");

                    int labelIndex = Array.IndexOf(targetClassIndices, maxClassIndex);
                    if (labelIndex != -1)
                    {
                        var bbox = new Rectangle(x, y, w, h);
                        detections.Add(new Detection
                        {
                            BoundingBox = bbox,
                            Label = classLabels[labelIndex],
                            Confidence = confidence
                        });
                        Console.WriteLine($"Detected {classLabels[labelIndex]} with confidence {confidence} at bbox: {bbox}");
                    }
                    else
                    {
                        Console.WriteLine($"Detected class index {maxClassIndex} is not 'person' or 'car'. Skipping...");
                    }
                }
            }

            var nmsDetections = ApplyNMS(detections);
            Console.WriteLine($"After NMS: {nmsDetections.Count} detections remaining.");
            return nmsDetections;
        }

        private List<Detection> ApplyNMS(List<Detection> detections)
        {
            if (!detections.Any()) return detections;

            var sortedDetections = detections.OrderByDescending(d => d.Confidence).ToList();
            List<Detection> result = new List<Detection>();

            while (sortedDetections.Any())
            {
                var best = sortedDetections[0];
                result.Add(best);
                sortedDetections.RemoveAt(0);

                sortedDetections = sortedDetections
                    .Where(d => ComputeIoU(best.BoundingBox, d.BoundingBox) < IouThreshold)
                    .ToList();
            }

            return result;
        }

        private float ComputeIoU(Rectangle box1, Rectangle box2)
        {
            float x1 = Math.Max(box1.X, box2.X);
            float y1 = Math.Max(box1.Y, box2.Y);
            float x2 = Math.Min(box1.Right, box2.Right);
            float y2 = Math.Min(box1.Bottom, box2.Bottom);

            float intersection = Math.Max(0, x2 - x1) * Math.Max(0, y2 - y1);
            float union = box1.Width * box1.Height + box2.Width * box2.Height - intersection;

            return intersection / union;
        }
    }

    public class Detection
    {
        public Rectangle BoundingBox { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
    }

    public class DetectionResult
    {
        public int TotalCount { get; set; }
        public List<Detection> Detections { get; set; }
    }
}