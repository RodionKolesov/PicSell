using PluginBase;
using System.Drawing;
namespace RotatePlugin
{
    public class RotatePlugin : IImageEditing
    {
        public string GetInfo()
        {
            return "Поворот изображения по часовой стрелке. Автор: Часовщик. Версия: 12.00";
        }

        public string GetGUID()
        {
            return "{45224D4F-1FAE-4612-8F06-72ECA7DDD027}";
        }

        public string GetGUIinfo()
        {
            return "Повернуть изображение по часовой стрелке";
        }

        public string GetPluginType()
        {
            return "ImageEditing";
        }

        public string SetSettings()
        {
            return null;
        }

        public Image ProcessImage(Image image, string settings=null)
        {
            try
            {
                var rotated = new Bitmap(image);
                rotated.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return rotated;
            }
            finally
            {
                image?.Dispose(); // Освобождаем исходное изображение
            }
        }
    }
}
