using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace PicSell
{
    public partial class BannerGeneratorForm : Form
    {
        private readonly int _imageId;
        private Image _sourceImage;
        private readonly List<Image> _variants = new List<Image>();
        private readonly List<string> _variantNames = new List<string>();
        private readonly List<Image> _customBgImages = new List<Image>();
        private readonly List<Panel> _cards = new List<Panel>();

        // ─── Конструктор ─────────────────────────────────────────────────
        public BannerGeneratorForm(int imageId)
        {
            InitializeComponent();
            _imageId = imageId;

            DarkTheme.Apply(this);
            ApplyTheme();

            Load += (s, e) => LoadSource();
        }

        // ─── Тема ────────────────────────────────────────────────────────
        private void ApplyTheme()
        {
            BackColor = DarkTheme.MainBg;
            headerPanel.BackColor = DarkTheme.PanelBg;
            titleLabel.ForeColor = DarkTheme.Text;
            sourceLabel.BackColor = DarkTheme.PanelBg;
            sourceLabel.ForeColor = DarkTheme.DimText;
            variantsLabel.BackColor = DarkTheme.PanelBg;
            variantsLabel.ForeColor = DarkTheme.DimText;
            flowPanel.BackColor = DarkTheme.DarkBg;
            sourcePictureBox.BackColor = DarkTheme.DarkBg;
            mainSplit.Panel1.BackColor = DarkTheme.PanelBg;
            mainSplit.Panel2.BackColor = DarkTheme.DarkBg;
            productNameBox.BackColor = DarkTheme.InputBg;
            productNameBox.ForeColor = DarkTheme.Text;
            productNameBox.BorderStyle = BorderStyle.FixedSingle;
            DarkTheme.StyleButton(addBgButton);
            addBgButton.BackColor = DarkTheme.Accent;
            addBgButton.ForeColor = Color.White;
            addBgButton.FlatAppearance.BorderSize = 0;
            addBgButton.UseVisualStyleBackColor = false;
        }

        // ─── Загрузка исходника ──────────────────────────────────────────
        private void LoadSource()
        {
            try
            {
                _sourceImage = MainForm.Instance.LoadImageFromDB(_imageId);
                if (_sourceImage != null)
                    sourcePictureBox.Image = _sourceImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки фото: " + ex.Message);
            }
        }

        // ─── Загрузка пользовательских фонов ─────────────────────────────
        private void addBgButton_Click(object sender, EventArgs e)
        {
            if (_sourceImage == null)
            {
                MessageBox.Show("Сначала загрузите фото товара.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Выберите фоны";
                dlg.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() != DialogResult.OK) return;

                foreach (var path in dlg.FileNames)
                {
                    try
                    {
                        var bg = Image.FromFile(path);
                        _customBgImages.Add(bg);
                        var composite = CreateOnBackground(_sourceImage, bg);
                        _variants.Add(composite);
                        var name = Path.GetFileNameWithoutExtension(path);
                        _variantNames.Add(name);
                        AddVariantCard(_variants.Count - 1, composite, name);
                    }
                    catch { }
                }

                variantsLabel.Text = _variants.Count == 0
                    ? "  Фоны  (нажми для применения)"
                    : $"  Фоны — {_variants.Count} шт.  (нажми для применения)";
            }
        }

        // ─── Товар поверх фона ───────────────────────────────────────────
        private Image CreateOnBackground(Image product, Image background)
        {
            int W = background.Width, H = background.Height;
            var bmp = new Bitmap(W, H);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Фон — полностью
                g.DrawImage(background, 0, 0, W, H);

                // Товар — по центру с отступами 10%
                int pad = (int)(Math.Min(W, H) * 0.10f);
                DrawImageFit(g, product, pad, pad, W - pad * 2, H - pad * 2);
            }
            return bmp;
        }

        private static void DrawImageFit(Graphics g, Image img, int x, int y, int w, int h)
        {
            float srcRatio = (float)img.Width / img.Height;
            float dstRatio = (float)w / h;
            int drawW, drawH, drawX, drawY;
            if (srcRatio > dstRatio)
            { drawW = w; drawH = (int)(w / srcRatio); drawX = x; drawY = y + (h - drawH) / 2; }
            else
            { drawH = h; drawW = (int)(h * srcRatio); drawY = y; drawX = x + (w - drawW) / 2; }
            g.DrawImage(img, drawX, drawY, drawW, drawH);
        }

        // ─── Карточка варианта ───────────────────────────────────────────
        private void AddVariantCard(int index, Image img, string name)
        {
            const int W = 188, H = 220;

            var card = new Panel
            {
                Size = new Size(W, H),
                Margin = new Padding(6),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(38, 38, 40),
                Tag = index
            };

            var pb = new PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(28, 28, 30),
                Cursor = Cursors.Hand
            };

            var lbl = new Label
            {
                Text = name,
                Dock = DockStyle.Bottom,
                Height = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 180, 255),
                BackColor = Color.FromArgb(32, 32, 34)
            };

            // Кнопка удаления — крестик в правом верхнем углу
            var deleteBtn = new Button
            {
                Text = "×",
                Size = new Size(20, 20),
                Location = new Point(W - 22, 2),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.FromArgb(60, 60, 65),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            deleteBtn.FlatAppearance.BorderSize = 0;
            deleteBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 60, 60);

            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(55, 55, 65);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(38, 38, 40);
            pb.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(55, 55, 65);
            pb.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(38, 38, 40);

            EventHandler applyClick = (s, e) => ApplyVariant((int)card.Tag);
            card.Click += applyClick;
            pb.Click += applyClick;
            lbl.Click += applyClick;

            deleteBtn.Click += (s, e) => RemoveCard(card);

            card.Controls.Add(pb);
            card.Controls.Add(lbl);
            card.Controls.Add(deleteBtn); // добавляем последним — рендерится поверх pb
            _cards.Add(card);
            flowPanel.Controls.Add(card);
        }

        // ─── Удаление карточки ────────────────────────────────────────────
        private void RemoveCard(Panel card)
        {
            int idx = _cards.IndexOf(card);
            if (idx < 0) return;

            // Освобождаем ресурсы
            _variants[idx]?.Dispose();
            _customBgImages[idx]?.Dispose();

            // Удаляем из всех списков
            _variants.RemoveAt(idx);
            _variantNames.RemoveAt(idx);
            _customBgImages.RemoveAt(idx);
            _cards.RemoveAt(idx);

            // Обновляем Tag у оставшихся карточек
            for (int i = idx; i < _cards.Count; i++)
                _cards[i].Tag = i;

            flowPanel.Controls.Remove(card);
            card.Dispose();

            variantsLabel.Text = _variants.Count == 0
                ? "  Фоны  (нажми для применения)"
                : $"  Фоны — {_variants.Count} шт.  (нажми для применения)";
        }

        // ─── Применить вариант ───────────────────────────────────────────
        private void ApplyVariant(int index)
        {
            if (index < 0 || index >= _variants.Count) return;
            var img = _variants[index];
            var name = _variantNames[index];

            var res = MessageBox.Show(
                $"Применить фон «{name}» к фото?",
                "Применить фон",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (res != DialogResult.Yes) return;

            try
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    ms.Position = 0;
                    var copy = Image.FromStream(ms);
                    MainForm.Instance.commitNewVersion(copy, _imageId, "Фон: " + name, "banner");
                    MainForm.Instance.updateListView();
                }
                MessageBox.Show($"Фон «{name}» применён!", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }

    // P/Invoke для placeholder
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
    }
}
