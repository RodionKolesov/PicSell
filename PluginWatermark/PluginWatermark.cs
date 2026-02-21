using PluginBase;
using System.Drawing;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;
using static System.Net.Mime.MediaTypeNames;
//using System.Reflection.Emit;
using System.Windows.Forms;

namespace PluginWatermark
{
    public class PluginWatermark : IImageEditing
    {
        public string GetInfo()
        {
            return "Добавление водяного знака. Автор: Водяной Яводяной. Версия: 9.0в";
        }

        public string GetGUID()
        {
            return "{EDF509F2-4C96-4842-B5DE-93A4543073C6}";
        }

        public string GetGUIinfo()
        {
            return "Добавить водяной знак";
        }

        public string GetPluginType()
        {
            return "ImageEditing";
        }

        public string SetSettings()
        {
            using (Form form = new Form())
            {
                form.Text = "Настройка водяного знака";
                form.Width = 400;
                form.Height = 150;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                Label label = new Label() { Left = 10, Top = 10, Text = "Введите текст для водяного знака:", AutoSize = true };
                TextBox textBox = new TextBox() { Left = 10, Top = 35, Width = 360 };
                Button buttonOk = new Button() { Text = "OK", Left = 280, Width = 90, Top = 70, DialogResult = DialogResult.OK };

                form.Controls.Add(label);
                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.AcceptButton = buttonOk;

                if (form.ShowDialog() == DialogResult.OK)
                {
                    return textBox.Text;
                }
                
            }
            return null;
        }

        public Image ProcessImage(Image inputImage, string watermarkText)
        {
            // Создаём новый Bitmap на основе оригинального изображения
            Bitmap bitmapWithWatermark = new Bitmap(inputImage);

            using (Graphics graphics = Graphics.FromImage(bitmapWithWatermark))
            {
                // Улучшение качества отрисовки
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // Настройка шрифта и кисти
                Font font = new Font("Arial", 12, FontStyle.Bold, GraphicsUnit.Pixel); // Уменьшенный размер шрифта
                Brush brush = new SolidBrush(Color.FromArgb(128, 255, 255, 255)); // Полупрозрачный белый

                // Сохраняем состояние
                var state = graphics.Save();

                // Перенос начала координат в центр изображения
                graphics.TranslateTransform(bitmapWithWatermark.Width / 2, bitmapWithWatermark.Height / 2);
                graphics.RotateTransform(-45); // Поворот на 45 градусов (по часовой стрелке — отрицательное значение)
                graphics.TranslateTransform(-bitmapWithWatermark.Width / 2, -bitmapWithWatermark.Height / 2);

                // Измеряем размер текста
                SizeF textSize = graphics.MeasureString(watermarkText, font);

                // Шаг между водяными знаками (можно настроить)
                int stepX = (int)textSize.Width + 40;
                int stepY = (int)textSize.Height + 50;

                // Рисуем водяной знак по всему изображению
                for (int y = -bitmapWithWatermark.Height; y < bitmapWithWatermark.Height * 2; y += stepY)
                {
                    for (int x = -bitmapWithWatermark.Width; x < bitmapWithWatermark.Width * 2; x += stepX)
                    {
                        graphics.DrawString(watermarkText, font, brush, new PointF(x, y));
                    }
                }

                // Восстанавливаем состояние графики
                graphics.Restore(state);
            }

            return bitmapWithWatermark;
        }

    }
}
