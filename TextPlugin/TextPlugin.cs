using PluginBase;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace TextPlugin
{
    public class TextPlugin : IImageEditing
    {
        public string GetInfo()
        {
            return "Добавление надписи на фото. Автор: Нестор Вордовский. Версия: 31.0.1";
        }

        public string GetGUID()
        {
            return "{ACB9CAE0-6AD0-40EB-A07E-43E3B2CA5BB6}";
        }

        public string GetGUIinfo()
        {
            return "Добавить надпись";
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
                form.Text = "Настройки текста";
                form.Width = 400;
                form.Height = 350;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                // Шрифт
                Label fontLabel = new Label() { Text = "Шрифт:", Left = 10, Top = 10, Width = 80 };
                ComboBox fontCombo = new ComboBox() { Left = 100, Top = 10, Width = 200 };
                fontCombo.Items.AddRange(FontFamily.Families.Select(f => f.Name).ToArray());
                fontCombo.SelectedIndex = fontCombo.Items.IndexOf("Arial");

                // Размер
                Label sizeLabel = new Label() { Text = "Размер:", Left = 10, Top = 40, Width = 80 };
                NumericUpDown sizeNumeric = new NumericUpDown() { Left = 100, Top = 40, Width = 50, Minimum = 6, Maximum = 72, Value = 12 };

                // Цвет
                Label colorLabel = new Label() { Text = "Цвет:", Left = 10, Top = 70, Width = 80 };
                Button colorButton = new Button() { Text = "Выбрать цвет", Left = 100, Top = 70, Width = 100 };
                ColorDialog colorDialog = new ColorDialog() { Color = Color.Black };
                colorButton.Click += (s, e) => { if (colorDialog.ShowDialog() == DialogResult.OK) colorButton.BackColor = colorDialog.Color; };

                // Расположение
                Label positionLabel = new Label() { Text = "Расположение:", Left = 10, Top = 100, Width = 80 };
                ComboBox positionCombo = new ComboBox() { Left = 100, Top = 100, Width = 100 };
                positionCombo.Items.AddRange(new string[] { "Верх", "Низ", "Лево", "Право", "Центр" });
                positionCombo.SelectedIndex = 0;

                // Текст
                Label textLabel = new Label() { Text = "Текст:", Left = 10, Top = 130, Width = 80 };
                TextBox textBox = new TextBox() { Left = 100, Top = 130, Width = 200 };

                // Начертание
                CheckBox boldBox = new CheckBox() { Text = "Жирный", Left = 100, Top = 160 };
                CheckBox italicBox = new CheckBox() { Text = "Курсив", Left = 100, Top = 190 };
                CheckBox underlineBox = new CheckBox() { Text = "Подчёркнутый", Left = 100, Top = 210 };

                // Кнопки
                Button okButton = new Button() { Text = "OK", Left = 100, Top = 240, Width = 80, DialogResult = DialogResult.OK };
                Button cancelButton = new Button() { Text = "Отмена", Left = 200, Top = 240, Width = 80, DialogResult = DialogResult.Cancel };
                form.AcceptButton = okButton;
                form.CancelButton = cancelButton;

                // Добавление на форму
                form.Controls.AddRange(new Control[] {
                        fontLabel, fontCombo,
                        sizeLabel, sizeNumeric,
                        colorLabel, colorButton,
                        positionLabel, positionCombo,
                        textLabel, textBox,
                        boldBox, italicBox, underlineBox,
                        okButton, cancelButton
                    });

                if (form.ShowDialog() == DialogResult.OK)
                {
                    string color = colorButton.BackColor.ToArgb().ToString();
                    string fontStyle = $"{(boldBox.Checked ? "B" : "")}{(italicBox.Checked ? "I" : "")}{(underlineBox.Checked ? "U" : "")}";
                    result = $"{fontCombo.SelectedItem}&{sizeNumeric.Value}&{color}&{positionCombo.SelectedItem}&{textBox.Text}&{fontStyle}";
                }
            }

            return result;
        }


        public Image ProcessImage(Image inputImage, string settings)
        {
            if (string.IsNullOrEmpty(settings)) return inputImage;

            string[] parts = settings.Split('&');
            if (parts.Length != 6) return inputImage;

            string fontName = parts[0];
            float fontSize = float.TryParse(parts[1], out float size) ? size : 12;
            Color color = Color.FromArgb(int.Parse(parts[2]));
            string position = parts[3];
            string text = parts[4];
            string style = parts[5];

            FontStyle fontStyle = FontStyle.Regular;
            if (style.Contains("B")) fontStyle |= FontStyle.Bold;
            if (style.Contains("I")) fontStyle |= FontStyle.Italic;
            if (style.Contains("U")) fontStyle |= FontStyle.Underline;

            Bitmap bitmap = new Bitmap(inputImage);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (Font font = new Font(fontName, fontSize, fontStyle))
                using (Brush brush = new SolidBrush(color))
                {
                    SizeF textSize = g.MeasureString(text, font);

                    float x = 0, y = 0;
                    switch (position)
                    {
                        case "Верх":
                            x = (bitmap.Width - textSize.Width) / 2;
                            y = 10;
                            break;
                        case "Низ":
                            x = (bitmap.Width - textSize.Width) / 2;
                            y = bitmap.Height - textSize.Height - 10;
                            break;
                        case "Лево":
                            x = 10;
                            y = (bitmap.Height - textSize.Height) / 2;
                            break;
                        case "Право":
                            x = bitmap.Width - textSize.Width - 10;
                            y = (bitmap.Height - textSize.Height) / 2;
                            break;
                        case "Центр":
                            x = (bitmap.Width - textSize.Width) / 2;
                            y = (bitmap.Height - textSize.Height) / 2;
                            break;
                    }

                    g.DrawString(text, font, brush, new PointF(x, y));
                }
            }

            return bitmap;
        }

    }
}
