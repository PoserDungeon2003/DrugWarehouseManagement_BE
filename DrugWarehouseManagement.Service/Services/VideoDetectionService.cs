using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DrugWarehouseManagement.Service.Services
{
    public class VideoDetectionService
    {
        private readonly string _modelPath;

        public VideoDetectionService(string modelPath)
        {
            _modelPath = modelPath;
        }

        public DetectionResult ProcessVideo(string videoPath)
        {
            if (!File.Exists(videoPath))
            {
                throw new FileNotFoundException("Video file not found", videoPath);
            }

            int totalCount = 0;
            List<Detection> allDetections = new List<Detection>();

            using (VideoCapture capture = new VideoCapture(videoPath))
            {
                Mat frame = new Mat();

                while (capture.Read(frame))
                {
                    Mat resizedFrame = ResizeFrame(frame);
                    var detections = RunYOLO(resizedFrame);

                    if (detections.Any())
                    {
                        totalCount += detections.Count;
                        allDetections.AddRange(detections);
                    }
                }
            }

            return new DetectionResult { TotalCount = totalCount, Detections = allDetections };
        }

        private Mat ResizeFrame(Mat frame)
        {
            Mat resizedFrame = new Mat();
            CvInvoke.Resize(frame, resizedFrame, new Size(640, 640));
            return resizedFrame;
        }

        private List<Detection> RunYOLO(Mat image)
        {
            List<Detection> detections = new List<Detection>();

            using (var session = new InferenceSession(_modelPath))
            {
                var inputTensor = ConvertImageToTensor(image);
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("images", inputTensor) };

                using (var results = session.Run(inputs))
                {
                    foreach (var result in results)
                    {
                        var output = result.AsEnumerable<float>().ToArray();
                        detections = ParseYOLOOutput(output, confidenceThreshold: 0.5f);
                    }
                }
            }

            return detections;
        }

        private DenseTensor<float> ConvertImageToTensor(Mat image)
        {
            Mat floatImage = new Mat();
            image.ConvertTo(floatImage, DepthType.Cv32F);
            float[] imageData = new float[image.Rows * image.Cols * image.NumberOfChannels];
            Marshal.Copy(floatImage.DataPointer, imageData, 0, imageData.Length);

            return new DenseTensor<float>(imageData, new[] { 1, 3, 640, 640 });
        }

        private List<Detection> ParseYOLOOutput(float[] output, float confidenceThreshold)
        {
            List<Detection> detections = new List<Detection>();

            for (int i = 0; i < output.Length; i += 7)
            {
                float confidence = output[i + 2];
                if (confidence >= confidenceThreshold)
                {
                    var bbox = new Rectangle(
                        (int)(output[i + 3] * 640),
                        (int)(output[i + 4] * 640),
                        (int)(output[i + 5] * 640 - output[i + 3] * 640),
                        (int)(output[i + 6] * 640 - output[i + 4] * 640)
                    );

                    detections.Add(new Detection { BoundingBox = bbox, Label = "Product", Confidence = confidence });
                }
            }

            return detections;
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
