using PluginBase;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;

namespace RemoveBackPlugin
{
    public class RemoveBackPlugin : IImageEditing
    {
        // ImageNet normalization constants
        private static readonly float[] Mean = { 0.485f, 0.406f, 0.456f };
        private static readonly float[] Std  = { 0.229f, 0.225f, 0.224f };
        private const int ModelSize = 320;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        private static readonly Lazy<InferenceSession> _session = new Lazy<InferenceSession>(() =>
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Ensure native onnxruntime.dll can be found when loaded via Assembly.LoadFrom
            SetDllDirectory(assemblyDir);

            string modelPath = Path.Combine(assemblyDir, "models", "u2net.onnx");
            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"U2-Net model not found: {modelPath}");

            var options = new SessionOptions();
            options.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            return new InferenceSession(modelPath, options);
        });

        public string GetInfo()
        {
            return "Удаление фона (U2-Net, локальная нейросеть). Автор: Пасилий Вупкин. Версия: 10.0";
        }

        public string GetGUID()
        {
            return "{E16EDEBB-7E81-4856-A9D1-CDB6B996A744}";
        }

        public string GetGUIinfo()
        {
            return "Удалить фон";
        }

        public string GetPluginType()
        {
            return "ImageEditing";
        }

        public string SetSettings()
        {
            return null;
        }

        public Image ProcessImage(Image inputImage, string settings = null)
        {
            int origW = inputImage.Width;
            int origH = inputImage.Height;

            // 1. Preprocess: resize to 320x320, normalize, build NCHW tensor
            float[] inputTensor = Preprocess(inputImage, origW, origH);

            // 2. Run inference
            float[] maskData = RunInference(inputTensor);

            // 3. Postprocess: sigmoid, normalize, resize mask, apply alpha
            Bitmap result = Postprocess(maskData, inputImage, origW, origH);

            return result;
        }

        private float[] Preprocess(Image image, int origW, int origH)
        {
            float[] tensor = new float[1 * 3 * ModelSize * ModelSize];

            using (var resized = new Bitmap(image, ModelSize, ModelSize))
            {
                BitmapData bmpData = resized.LockBits(
                    new Rectangle(0, 0, ModelSize, ModelSize),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                int stride = bmpData.Stride;
                byte[] pixels = new byte[stride * ModelSize];
                Marshal.Copy(bmpData.Scan0, pixels, 0, pixels.Length);
                resized.UnlockBits(bmpData);

                int planeSize = ModelSize * ModelSize;
                for (int y = 0; y < ModelSize; y++)
                {
                    int rowOffset = y * stride;
                    for (int x = 0; x < ModelSize; x++)
                    {
                        int pixelOffset = rowOffset + x * 4; // BGRA
                        float b = pixels[pixelOffset + 0] / 255f;
                        float g = pixels[pixelOffset + 1] / 255f;
                        float r = pixels[pixelOffset + 2] / 255f;

                        int idx = y * ModelSize + x;
                        // NCHW: channel 0=R, 1=G, 2=B
                        tensor[0 * planeSize + idx] = (r - Mean[0]) / Std[0];
                        tensor[1 * planeSize + idx] = (g - Mean[1]) / Std[1];
                        tensor[2 * planeSize + idx] = (b - Mean[2]) / Std[2];
                    }
                }
            }

            return tensor;
        }

        private float[] RunInference(float[] inputTensor)
        {
            var session = _session.Value;

            var tensor = new DenseTensor<float>(inputTensor, new[] { 1, 3, ModelSize, ModelSize });
            string inputName = session.InputMetadata.Keys.First();
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(inputName, tensor)
            };

            using (var results = session.Run(inputs))
            {
                // U2-Net outputs 7 side outputs; use the first one (d0) — highest quality
                var output = results.First();
                var outputTensor = output.AsTensor<float>();

                float[] data = new float[outputTensor.Length];
                int i = 0;
                foreach (var val in outputTensor)
                {
                    data[i++] = val;
                }
                return data;
            }
        }

        private Bitmap Postprocess(float[] maskData, Image originalImage, int origW, int origH)
        {
            // Apply sigmoid activation
            for (int i = 0; i < maskData.Length; i++)
            {
                maskData[i] = Sigmoid(maskData[i]);
            }

            // Min-max normalization to [0, 1]
            float min = maskData[0], max = maskData[0];
            for (int i = 1; i < maskData.Length; i++)
            {
                if (maskData[i] < min) min = maskData[i];
                if (maskData[i] > max) max = maskData[i];
            }

            float range = max - min;
            if (range > 0)
            {
                for (int i = 0; i < maskData.Length; i++)
                {
                    maskData[i] = (maskData[i] - min) / range;
                }
            }

            // Resize mask from 320x320 to original dimensions using bilinear interpolation
            byte[] alphaMask = ResizeMask(maskData, ModelSize, ModelSize, origW, origH);

            // Apply alpha mask to original image
            Bitmap result = new Bitmap(origW, origH, PixelFormat.Format32bppArgb);

            using (var origBmp = new Bitmap(originalImage))
            {
                BitmapData srcData = origBmp.LockBits(
                    new Rectangle(0, 0, origW, origH),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                BitmapData dstData = result.LockBits(
                    new Rectangle(0, 0, origW, origH),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                int byteCount = srcData.Stride * origH;
                byte[] srcPixels = new byte[byteCount];
                byte[] dstPixels = new byte[byteCount];

                Marshal.Copy(srcData.Scan0, srcPixels, 0, byteCount);

                int stride = srcData.Stride;
                for (int y = 0; y < origH; y++)
                {
                    int rowOffset = y * stride;
                    for (int x = 0; x < origW; x++)
                    {
                        int pixelOffset = rowOffset + x * 4;
                        byte alpha = alphaMask[y * origW + x];

                        dstPixels[pixelOffset + 0] = srcPixels[pixelOffset + 0]; // B
                        dstPixels[pixelOffset + 1] = srcPixels[pixelOffset + 1]; // G
                        dstPixels[pixelOffset + 2] = srcPixels[pixelOffset + 2]; // R
                        dstPixels[pixelOffset + 3] = alpha;                       // A
                    }
                }

                Marshal.Copy(dstPixels, 0, dstData.Scan0, byteCount);

                origBmp.UnlockBits(srcData);
                result.UnlockBits(dstData);
            }

            return result;
        }

        private byte[] ResizeMask(float[] mask, int srcW, int srcH, int dstW, int dstH)
        {
            byte[] result = new byte[dstW * dstH];
            float xRatio = (float)srcW / dstW;
            float yRatio = (float)srcH / dstH;

            for (int y = 0; y < dstH; y++)
            {
                float srcY = y * yRatio;
                int y0 = (int)srcY;
                int y1 = Math.Min(y0 + 1, srcH - 1);
                float fy = srcY - y0;

                for (int x = 0; x < dstW; x++)
                {
                    float srcX = x * xRatio;
                    int x0 = (int)srcX;
                    int x1 = Math.Min(x0 + 1, srcW - 1);
                    float fx = srcX - x0;

                    // Bilinear interpolation
                    float v00 = mask[y0 * srcW + x0];
                    float v01 = mask[y0 * srcW + x1];
                    float v10 = mask[y1 * srcW + x0];
                    float v11 = mask[y1 * srcW + x1];

                    float value = v00 * (1 - fx) * (1 - fy)
                                + v01 * fx * (1 - fy)
                                + v10 * (1 - fx) * fy
                                + v11 * fx * fy;

                    result[y * dstW + x] = (byte)(value * 255f + 0.5f);
                }
            }

            return result;
        }

        private static float Sigmoid(float x)
        {
            return 1f / (1f + (float)Math.Exp(-x));
        }
    }
}
