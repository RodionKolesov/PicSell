using System;
using System.Drawing;
using PluginBase;

namespace ContrastPlugin
{
    public class ContrastPlugin : IImageEditing
    {
        public string GetInfo()
        {
            return "Увеличение контраста у изображения. Автор: Василий Пупкин. Версия: 0.9b";
        }

        public string GetGUID()
        {
            return "{6AB59D92-B561-4C61-A54E-4CFADBEF8635}"; 
        }

        public string GetGUIinfo()
        {
            return "Увеличить контраст";
        }

        public string GetPluginType()
        {
            return "ImageEditing";
        }

        public string SetSettings()
        {
            return null;
        }

        private int Truncate(int value)
        {
            return Math.Min(255, Math.Max(0, value));
        }

        public Image ProcessImage(Image image, string settings=null)
        {
            if (image != null)
            {
                Bitmap original = new Bitmap(image);
                Bitmap contrast = new Bitmap(original.Width, original.Height);

                float contrastLevel = 1.2f;

                for (int y = 0; y < original.Height; y++)
                {
                    for (int x = 0; x < original.Width; x++)
                    {
                        Color p = original.GetPixel(x, y);

                        if (p.A == 0) continue; // Пропускаем полностью прозрачные пиксели

                        int r = Truncate((int)((p.R - 128) * contrastLevel + 128));
                        int g = Truncate((int)((p.G - 128) * contrastLevel + 128));
                        int b = Truncate((int)((p.B - 128) * contrastLevel + 128));

                        contrast.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }

                image = contrast;
            }
            return image;
        }


    }
}

