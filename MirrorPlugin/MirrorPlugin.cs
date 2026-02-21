using PluginBase;
using System.Drawing;
using System.Windows.Forms;

namespace MirrorPlugin
{
    public class MirrorPlugin : IImageEditing
    {
        public string GetInfo()
        {
            return "Отзеркаливание изображения. Автор: Зазеркальный шляпник. Версия: 81.18";
        }

        public string GetGUID()
        {
            return "{F3D411A7-E867-4353-A3D9-97BA9D9D3522}";
        }

        public string GetGUIinfo()
        {
            return "Отзеркалить";
        }

        public string GetPluginType()
        {
            return "ImageEditing";
        }

        public string SetSettings()
        {
            string result = null;

            using (Form form = new Form())
            {
                form.Text = "Выберите способ как отзеркалить";
                form.Width = 400;
                form.Height = 150;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                Button horizontalButton = new Button();
                horizontalButton.Text = "По горизонтали";
                horizontalButton.DialogResult = DialogResult.OK;
                horizontalButton.Width = 150;
                horizontalButton.Height = 40;
                horizontalButton.Top = 20;
                horizontalButton.Left = 20;
                horizontalButton.Click += (sender, e) => { result = "horizontal"; form.Close(); };

                Button verticalButton = new Button();
                verticalButton.Text = "По вертикали";
                verticalButton.DialogResult = DialogResult.OK;
                verticalButton.Width = 150;
                verticalButton.Height = 40;
                verticalButton.Top = 20;
                verticalButton.Left = form.Width - verticalButton.Width - 20;
                verticalButton.Click += (sender, e) => { result = "vertical"; form.Close(); };

                form.Controls.Add(horizontalButton);
                form.Controls.Add(verticalButton);

                form.ShowDialog();
            }

            return result;
        }

        public Image ProcessImage(Image inputImage, string settings)
        {
            Bitmap bmp = new Bitmap(inputImage);

            if (settings == "horizontal")
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            else if (settings == "vertical")
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            return (Image)bmp;
        }
    }
}
