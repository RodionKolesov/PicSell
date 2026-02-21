using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PicSell
{
    public class TextEditorForm : Form
    {
        private class TextItem
        {
            public string Text = "";
            public string FontFamily = "Arial";
            public float FontSize = 48;
            public bool Bold;
            public bool Italic;
            public bool Underline;
            public Color SolidColor = Color.White;
            public bool UseGradient;
            public Color GradientColor1 = Color.Red;
            public Color GradientColor2 = Color.Blue;
            public bool GradientVertical = true;
            public float X;
            public float Y;

            public override string ToString()
            {
                string preview = Text.Length > 20 ? Text.Substring(0, 20) + "..." : Text;
                return $"{preview} [{FontFamily}, {FontSize}pt]";
            }
        }

        private readonly Image _sourceImage;
        private readonly List<TextItem> _items = new List<TextItem>();

        // Left panel controls
        private TextBox _textInput;
        private ComboBox _fontCombo;
        private NumericUpDown _sizeInput;
        private Button _boldBtn, _italicBtn, _underlineBtn;
        private Button _colorBtn;
        private CheckBox _gradientCheck;
        private Button _gradColor1Btn, _gradColor2Btn;
        private ComboBox _gradDirectionCombo;
        private Button _addBtn;
        private ListBox _itemsList;
        private Button _deleteBtn;

        // Text preview
        private PictureBox _textPreview;

        // Center
        private PictureBox _preview;

        // Bottom
        private Button _applyButton, _cancelButton;

        // Drag state
        private bool _dragging;
        private Point _dragStart;
        private float _dragItemStartX, _dragItemStartY;

        private bool _suppressSync;

        public Image ResultImage { get; private set; }

        public TextEditorForm(Image sourceImage)
        {
            _sourceImage = sourceImage;
            InitUI();
            RenderPreview();
            RenderTextPreview();
        }

        private void InitUI()
        {
            this.Text = "Редактор текста на изображении";
            this.Size = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);
            this.KeyPreview = true;

            // === Left panel ===
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 290,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            int y = 10;

            // Text input
            var textLabel = MakeLabel("Текст:", leftPanel, ref y);
            _textInput = new TextBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 60),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = DarkTheme.InputBg,
                ForeColor = DarkTheme.Text,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Текст"
            };
            leftPanel.Controls.Add(_textInput);
            _textInput.TextChanged += OnPropertyChanged;
            y += 68;

            // Font combo
            MakeLabel("Шрифт:", leftPanel, ref y);
            _fontCombo = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DarkTheme.InputBg,
                ForeColor = DarkTheme.Text
            };
            foreach (var ff in FontFamily.Families)
                _fontCombo.Items.Add(ff.Name);
            int arialIdx = _fontCombo.Items.IndexOf("Arial");
            _fontCombo.SelectedIndex = arialIdx >= 0 ? arialIdx : 0;
            _fontCombo.SelectedIndexChanged += OnPropertyChanged;
            leftPanel.Controls.Add(_fontCombo);
            y += 32;

            // Size
            MakeLabel("Размер:", leftPanel, ref y);
            _sizeInput = new NumericUpDown
            {
                Location = new Point(10, y),
                Size = new Size(260, 25),
                Minimum = 8,
                Maximum = 500,
                Value = 48,
                BackColor = DarkTheme.InputBg,
                ForeColor = DarkTheme.Text
            };
            _sizeInput.ValueChanged += OnPropertyChanged;
            leftPanel.Controls.Add(_sizeInput);
            y += 32;

            // Style buttons: B I U
            MakeLabel("Стиль:", leftPanel, ref y);
            _boldBtn = MakeToggleButton("B", new Point(10, y), FontStyle.Bold);
            _italicBtn = MakeToggleButton("I", new Point(55, y), FontStyle.Italic);
            _underlineBtn = MakeToggleButton("U", new Point(100, y), FontStyle.Regular);
            _underlineBtn.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            leftPanel.Controls.Add(_boldBtn);
            leftPanel.Controls.Add(_italicBtn);
            leftPanel.Controls.Add(_underlineBtn);
            _boldBtn.Click += OnStyleToggle;
            _italicBtn.Click += OnStyleToggle;
            _underlineBtn.Click += OnStyleToggle;
            y += 38;

            // Color
            MakeLabel("Цвет:", leftPanel, ref y);
            _colorBtn = new Button
            {
                Location = new Point(10, y),
                Size = new Size(260, 28),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Text = "",
                Cursor = Cursors.Hand
            };
            _colorBtn.FlatAppearance.BorderColor = DarkTheme.Border;
            _colorBtn.Click += (s, e) =>
            {
                using (var cd = new ColorDialog { Color = _colorBtn.BackColor, FullOpen = true })
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        _colorBtn.BackColor = cd.Color;
                        OnPropertyChanged(s, e);
                    }
                }
            };
            leftPanel.Controls.Add(_colorBtn);
            y += 36;

            // Gradient checkbox
            _gradientCheck = new CheckBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 24),
                Text = "Градиент",
                ForeColor = DarkTheme.Text,
                BackColor = Color.Transparent
            };
            _gradientCheck.CheckedChanged += (s, e) =>
            {
                UpdateGradientControlsVisibility();
                OnPropertyChanged(s, e);
            };
            leftPanel.Controls.Add(_gradientCheck);
            y += 28;

            // Gradient color 1
            _gradColor1Btn = new Button
            {
                Location = new Point(10, y),
                Size = new Size(125, 28),
                BackColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Text = "Цвет 1",
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = false
            };
            _gradColor1Btn.FlatAppearance.BorderColor = DarkTheme.Border;
            _gradColor1Btn.Click += (s, e) =>
            {
                using (var cd = new ColorDialog { Color = _gradColor1Btn.BackColor, FullOpen = true })
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        _gradColor1Btn.BackColor = cd.Color;
                        OnPropertyChanged(s, e);
                    }
                }
            };
            leftPanel.Controls.Add(_gradColor1Btn);

            // Gradient color 2
            _gradColor2Btn = new Button
            {
                Location = new Point(145, y),
                Size = new Size(125, 28),
                BackColor = Color.Blue,
                FlatStyle = FlatStyle.Flat,
                Text = "Цвет 2",
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Visible = false
            };
            _gradColor2Btn.FlatAppearance.BorderColor = DarkTheme.Border;
            _gradColor2Btn.Click += (s, e) =>
            {
                using (var cd = new ColorDialog { Color = _gradColor2Btn.BackColor, FullOpen = true })
                {
                    if (cd.ShowDialog() == DialogResult.OK)
                    {
                        _gradColor2Btn.BackColor = cd.Color;
                        OnPropertyChanged(s, e);
                    }
                }
            };
            leftPanel.Controls.Add(_gradColor2Btn);
            y += 36;

            // Gradient direction
            _gradDirectionCombo = new ComboBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = DarkTheme.InputBg,
                ForeColor = DarkTheme.Text,
                Visible = false
            };
            _gradDirectionCombo.Items.AddRange(new object[] { "Вертикальный", "Горизонтальный" });
            _gradDirectionCombo.SelectedIndex = 0;
            _gradDirectionCombo.SelectedIndexChanged += OnPropertyChanged;
            leftPanel.Controls.Add(_gradDirectionCombo);
            y += 32;

            // Add text button
            _addBtn = new Button
            {
                Location = new Point(10, y),
                Size = new Size(260, 36),
                Text = "Добавить текст",
                Cursor = Cursors.Hand
            };
            DarkTheme.StyleButton(_addBtn);
            _addBtn.BackColor = DarkTheme.Accent;
            _addBtn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _addBtn.Click += AddBtn_Click;
            leftPanel.Controls.Add(_addBtn);
            y += 44;

            // Items list
            MakeLabel("Текстовые элементы:", leftPanel, ref y);
            _itemsList = new ListBox
            {
                Location = new Point(10, y),
                Size = new Size(260, 120),
                BackColor = DarkTheme.InputBg,
                ForeColor = DarkTheme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
            _itemsList.SelectedIndexChanged += ItemsList_SelectedIndexChanged;
            leftPanel.Controls.Add(_itemsList);
            y += 128;

            // Delete button
            _deleteBtn = new Button
            {
                Location = new Point(10, y),
                Size = new Size(260, 30),
                Text = "Удалить",
                Cursor = Cursors.Hand
            };
            DarkTheme.StyleButton(_deleteBtn);
            _deleteBtn.Click += DeleteBtn_Click;
            leftPanel.Controls.Add(_deleteBtn);

            // === Text preview panel (fixed at bottom-left) ===
            var previewLabel = new Label
            {
                Text = "Предпросмотр текста:",
                Dock = DockStyle.Top,
                Height = 20,
                ForeColor = DarkTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(4, 2, 0, 0)
            };

            _textPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(30, 30, 30),
                BorderStyle = BorderStyle.FixedSingle
            };

            var textPreviewPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                Width = 290,
                Padding = new Padding(6)
            };
            textPreviewPanel.Controls.Add(_textPreview);
            textPreviewPanel.Controls.Add(previewLabel);

            // === Left container: scrollable controls on top + fixed preview on bottom ===
            var leftContainer = new Panel
            {
                Dock = DockStyle.Left,
                Width = 290
            };
            leftPanel.Dock = DockStyle.Fill;
            leftContainer.Controls.Add(leftPanel);
            leftContainer.Controls.Add(textPreviewPanel);

            // === Bottom panel ===
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            _applyButton = new Button
            {
                Text = "Применить",
                Size = new Size(120, 34),
                DialogResult = DialogResult.None
            };
            _applyButton.Click += ApplyButton_Click;

            _cancelButton = new Button
            {
                Text = "Отмена",
                Size = new Size(100, 34),
                DialogResult = DialogResult.Cancel
            };

            var hintLabel = new Label
            {
                Text = "Мышь — перемещение • Стрелки — сдвиг 5px (Shift — 1px)",
                AutoSize = true,
                ForeColor = DarkTheme.DimText,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic)
            };

            bottomPanel.Controls.Add(_applyButton);
            bottomPanel.Controls.Add(_cancelButton);
            bottomPanel.Controls.Add(hintLabel);

            bottomPanel.Resize += (s, e) =>
            {
                _applyButton.Location = new Point(bottomPanel.Width - _applyButton.Width - _cancelButton.Width - 24, (bottomPanel.Height - _applyButton.Height) / 2);
                _cancelButton.Location = new Point(bottomPanel.Width - _cancelButton.Width - 12, (bottomPanel.Height - _cancelButton.Height) / 2);
                hintLabel.Location = new Point(12, (bottomPanel.Height - hintLabel.Height) / 2);
            };

            // === Preview ===
            _preview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = DarkTheme.DarkBg
            };

            _preview.MouseDown += Preview_MouseDown;
            _preview.MouseMove += Preview_MouseMove;
            _preview.MouseUp += Preview_MouseUp;

            // Splitter between left panel and preview
            var splitter = new Splitter
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = DarkTheme.Border
            };

            this.Controls.Add(_preview);
            this.Controls.Add(splitter);
            this.Controls.Add(leftContainer);
            this.Controls.Add(bottomPanel);

            this.CancelButton = _cancelButton;

            // Apply dark theme
            DarkTheme.Apply(this);
            DarkTheme.StyleButton(_applyButton);
            DarkTheme.StyleButton(_cancelButton);
            _applyButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _applyButton.BackColor = DarkTheme.Accent;

            // Restore color button backgrounds (theme overrides them)
            _colorBtn.BackColor = Color.White;
            _gradColor1Btn.BackColor = Color.Red;
            _gradColor2Btn.BackColor = Color.Blue;
            _addBtn.BackColor = DarkTheme.Accent;
        }

        #region Helpers

        private Label MakeLabel(string text, Panel parent, ref int y)
        {
            var lbl = new Label
            {
                Text = text,
                Location = new Point(10, y),
                AutoSize = true,
                ForeColor = DarkTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F)
            };
            parent.Controls.Add(lbl);
            y += 20;
            return lbl;
        }

        private Button MakeToggleButton(string text, Point location, FontStyle style)
        {
            var btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(36, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = DarkTheme.BtnBg,
                ForeColor = DarkTheme.Text,
                Font = new Font("Segoe UI", 9F, style),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = DarkTheme.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = DarkTheme.BtnHover;
            return btn;
        }

        private void UpdateGradientControlsVisibility()
        {
            bool show = _gradientCheck.Checked;
            _gradColor1Btn.Visible = show;
            _gradColor2Btn.Visible = show;
            _gradDirectionCombo.Visible = show;
        }

        private void RenderTextPreview()
        {
            string text = _textInput.Text;
            if (string.IsNullOrWhiteSpace(text)) text = "Abc";

            string fontName = _fontCombo.SelectedItem?.ToString() ?? "Arial";
            float fontSize = (float)_sizeInput.Value;
            bool bold = _boldBtn.BackColor == DarkTheme.Accent;
            bool italic = _italicBtn.BackColor == DarkTheme.Accent;
            bool underline = _underlineBtn.BackColor == DarkTheme.Accent;
            bool useGradient = _gradientCheck.Checked;

            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            if (underline) style |= FontStyle.Underline;

            FontFamily family;
            try { family = new FontFamily(fontName); }
            catch { family = new FontFamily("Arial"); }

            if (!family.IsStyleAvailable(style))
            {
                if (family.IsStyleAvailable(FontStyle.Regular)) style = FontStyle.Regular;
                else if (family.IsStyleAvailable(FontStyle.Bold)) style = FontStyle.Bold;
                else if (family.IsStyleAvailable(FontStyle.Italic)) style = FontStyle.Italic;
            }

            // Measure text bounds
            float emSize = fontSize * 96f / 72f;
            RectangleF bounds;
            using (var measurePath = new GraphicsPath())
            {
                measurePath.AddString(text, family, (int)style, emSize, PointF.Empty, StringFormat.GenericDefault);
                bounds = measurePath.GetBounds();
            }

            if (bounds.Width < 1 || bounds.Height < 1)
            {
                family.Dispose();
                return;
            }

            // Create bitmap with padding
            int pad = 10;
            int bmpW = (int)bounds.Width + pad * 2 + 2;
            int bmpH = (int)bounds.Height + pad * 2 + 2;
            var bmp = new Bitmap(bmpW, bmpH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                g.Clear(Color.FromArgb(30, 30, 30));

                // Draw checkerboard to show transparency
                using (var lightBrush = new SolidBrush(Color.FromArgb(45, 45, 45)))
                {
                    for (int cy = 0; cy < bmpH; cy += 10)
                        for (int cx = 0; cx < bmpW; cx += 10)
                            if ((cx / 10 + cy / 10) % 2 == 0)
                                g.FillRectangle(lightBrush, cx, cy, 10, 10);
                }

                float drawX = pad - bounds.X;
                float drawY = pad - bounds.Y;

                using (var path = new GraphicsPath())
                {
                    path.AddString(text, family, (int)style, emSize,
                        new PointF(drawX, drawY), StringFormat.GenericDefault);

                    RectangleF pathBounds = path.GetBounds();

                    if (useGradient && pathBounds.Width > 0 && pathBounds.Height > 0)
                    {
                        var gradRect = new RectangleF(pathBounds.X - 1, pathBounds.Y - 1,
                            pathBounds.Width + 2, pathBounds.Height + 2);
                        var mode = _gradDirectionCombo.SelectedIndex == 0
                            ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
                        using (var gradBrush = new LinearGradientBrush(gradRect,
                            _gradColor1Btn.BackColor, _gradColor2Btn.BackColor, mode))
                        {
                            g.FillPath(gradBrush, path);
                        }
                    }
                    else
                    {
                        using (var solidBrush = new SolidBrush(_colorBtn.BackColor))
                        {
                            g.FillPath(solidBrush, path);
                        }
                    }

                    using (var pen = new Pen(Color.FromArgb(180, 0, 0, 0), Math.Max(1f, fontSize / 30f)))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            family.Dispose();

            var old = _textPreview.Image;
            _textPreview.Image = bmp;
            old?.Dispose();
        }

        #endregion

        #region Adding / Selecting / Deleting items

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_textInput.Text))
            {
                MessageBox.Show("Введите текст.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var item = new TextItem
            {
                Text = _textInput.Text,
                FontFamily = _fontCombo.SelectedItem?.ToString() ?? "Arial",
                FontSize = (float)_sizeInput.Value,
                Bold = _boldBtn.BackColor == DarkTheme.Accent,
                Italic = _italicBtn.BackColor == DarkTheme.Accent,
                Underline = _underlineBtn.BackColor == DarkTheme.Accent,
                SolidColor = _colorBtn.BackColor,
                UseGradient = _gradientCheck.Checked,
                GradientColor1 = _gradColor1Btn.BackColor,
                GradientColor2 = _gradColor2Btn.BackColor,
                GradientVertical = _gradDirectionCombo.SelectedIndex == 0,
                X = _sourceImage.Width / 2f - 100,
                Y = _sourceImage.Height / 2f - 30
            };

            _items.Add(item);
            _itemsList.Items.Add(item);
            _itemsList.SelectedIndex = _itemsList.Items.Count - 1;
            RenderPreview();
        }

        private void ItemsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_itemsList.SelectedIndex < 0 || _itemsList.SelectedIndex >= _items.Count)
                return;

            _suppressSync = true;
            var item = _items[_itemsList.SelectedIndex];

            _textInput.Text = item.Text;

            int fontIdx = _fontCombo.Items.IndexOf(item.FontFamily);
            if (fontIdx >= 0) _fontCombo.SelectedIndex = fontIdx;

            _sizeInput.Value = (decimal)Math.Max((float)_sizeInput.Minimum, Math.Min((float)_sizeInput.Maximum, item.FontSize));

            SetToggle(_boldBtn, item.Bold);
            SetToggle(_italicBtn, item.Italic);
            SetToggle(_underlineBtn, item.Underline);

            _colorBtn.BackColor = item.SolidColor;
            _gradientCheck.Checked = item.UseGradient;
            _gradColor1Btn.BackColor = item.GradientColor1;
            _gradColor2Btn.BackColor = item.GradientColor2;
            _gradDirectionCombo.SelectedIndex = item.GradientVertical ? 0 : 1;

            UpdateGradientControlsVisibility();
            _suppressSync = false;

            RenderTextPreview();
            RenderPreview();
        }

        private void SetToggle(Button btn, bool active)
        {
            btn.BackColor = active ? DarkTheme.Accent : DarkTheme.BtnBg;
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            int idx = _itemsList.SelectedIndex;
            if (idx < 0) return;

            _items.RemoveAt(idx);
            _itemsList.Items.RemoveAt(idx);

            if (_itemsList.Items.Count > 0)
                _itemsList.SelectedIndex = Math.Min(idx, _itemsList.Items.Count - 1);

            RenderPreview();
        }

        #endregion

        #region Property changes

        private void OnStyleToggle(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            bool active = btn.BackColor != DarkTheme.Accent;
            btn.BackColor = active ? DarkTheme.Accent : DarkTheme.BtnBg;
            OnPropertyChanged(sender, e);
        }

        private void OnPropertyChanged(object sender, EventArgs e)
        {
            if (_suppressSync) return;

            RenderTextPreview();

            if (_itemsList.SelectedIndex < 0 || _itemsList.SelectedIndex >= _items.Count) return;

            var item = _items[_itemsList.SelectedIndex];
            item.Text = _textInput.Text;
            item.FontFamily = _fontCombo.SelectedItem?.ToString() ?? "Arial";
            item.FontSize = (float)_sizeInput.Value;
            item.Bold = _boldBtn.BackColor == DarkTheme.Accent;
            item.Italic = _italicBtn.BackColor == DarkTheme.Accent;
            item.Underline = _underlineBtn.BackColor == DarkTheme.Accent;
            item.SolidColor = _colorBtn.BackColor;
            item.UseGradient = _gradientCheck.Checked;
            item.GradientColor1 = _gradColor1Btn.BackColor;
            item.GradientColor2 = _gradColor2Btn.BackColor;
            item.GradientVertical = _gradDirectionCombo.SelectedIndex == 0;

            // Update listbox display
            _itemsList.Items[_itemsList.SelectedIndex] = item;

            RenderPreview();
        }

        #endregion

        #region Rendering

        private void RenderPreview()
        {
            var bmp = new Bitmap(_sourceImage.Width, _sourceImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                g.DrawImage(_sourceImage, 0, 0, _sourceImage.Width, _sourceImage.Height);

                int selectedIdx = _itemsList.SelectedIndex;

                for (int i = 0; i < _items.Count; i++)
                {
                    DrawTextItem(g, _items[i], i == selectedIdx);
                }
            }

            var old = _preview.Image;
            _preview.Image = bmp;
            old?.Dispose();
        }

        private void DrawTextItem(Graphics g, TextItem item, bool selected)
        {
            if (string.IsNullOrEmpty(item.Text)) return;

            FontStyle style = FontStyle.Regular;
            if (item.Bold) style |= FontStyle.Bold;
            if (item.Italic) style |= FontStyle.Italic;
            if (item.Underline) style |= FontStyle.Underline;

            FontFamily family;
            try
            {
                family = new FontFamily(item.FontFamily);
            }
            catch
            {
                family = new FontFamily("Arial");
            }

            // Check if the style is available for this font family
            if (!family.IsStyleAvailable(style))
            {
                // Try to find an available style
                if (family.IsStyleAvailable(FontStyle.Regular))
                    style = FontStyle.Regular;
                else if (family.IsStyleAvailable(FontStyle.Bold))
                    style = FontStyle.Bold;
                else if (family.IsStyleAvailable(FontStyle.Italic))
                    style = FontStyle.Italic;
            }

            float emSize = item.FontSize * 96f / 72f; // points to pixels for GraphicsPath

            using (var path = new GraphicsPath())
            {
                path.AddString(
                    item.Text,
                    family,
                    (int)style,
                    emSize,
                    new PointF(item.X, item.Y),
                    StringFormat.GenericDefault
                );

                RectangleF bounds = path.GetBounds();

                if (item.UseGradient && bounds.Width > 0 && bounds.Height > 0)
                {
                    // Expand bounds slightly to avoid edge artifacts
                    var gradRect = new RectangleF(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 2);
                    var mode = item.GradientVertical ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;

                    using (var gradBrush = new LinearGradientBrush(gradRect, item.GradientColor1, item.GradientColor2, mode))
                    {
                        g.FillPath(gradBrush, path);
                    }
                }
                else
                {
                    using (var solidBrush = new SolidBrush(item.SolidColor))
                    {
                        g.FillPath(solidBrush, path);
                    }
                }

                // Outline for readability
                using (var pen = new Pen(Color.FromArgb(180, 0, 0, 0), Math.Max(1f, item.FontSize / 30f)))
                {
                    g.DrawPath(pen, path);
                }

                // Selection highlight
                if (selected && bounds.Width > 0 && bounds.Height > 0)
                {
                    using (var selPen = new Pen(Color.FromArgb(200, DarkTheme.Accent), 2f))
                    {
                        selPen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(selPen, bounds.X - 4, bounds.Y - 4, bounds.Width + 8, bounds.Height + 8);
                    }
                }
            }

            family.Dispose();
        }

        #endregion

        #region Mouse drag

        private PointF PictureBoxToImage(Point pbPoint)
        {
            if (_preview.Image == null) return PointF.Empty;

            int imgW = _preview.Image.Width;
            int imgH = _preview.Image.Height;
            int pbW = _preview.ClientSize.Width;
            int pbH = _preview.ClientSize.Height;

            float ratioX = (float)pbW / imgW;
            float ratioY = (float)pbH / imgH;
            float ratio = Math.Min(ratioX, ratioY);

            float dispW = imgW * ratio;
            float dispH = imgH * ratio;
            float padX = (pbW - dispW) / 2f;
            float padY = (pbH - dispH) / 2f;

            float imgX = (pbPoint.X - padX) / ratio;
            float imgY = (pbPoint.Y - padY) / ratio;

            return new PointF(imgX, imgY);
        }

        private RectangleF GetTextItemBounds(TextItem item)
        {
            if (string.IsNullOrEmpty(item.Text)) return RectangleF.Empty;

            FontStyle style = FontStyle.Regular;
            if (item.Bold) style |= FontStyle.Bold;
            if (item.Italic) style |= FontStyle.Italic;
            if (item.Underline) style |= FontStyle.Underline;

            FontFamily family;
            try { family = new FontFamily(item.FontFamily); }
            catch { family = new FontFamily("Arial"); }

            if (!family.IsStyleAvailable(style))
            {
                if (family.IsStyleAvailable(FontStyle.Regular)) style = FontStyle.Regular;
                else if (family.IsStyleAvailable(FontStyle.Bold)) style = FontStyle.Bold;
                else if (family.IsStyleAvailable(FontStyle.Italic)) style = FontStyle.Italic;
            }

            float emSize = item.FontSize * 96f / 72f;
            RectangleF bounds;
            using (var path = new GraphicsPath())
            {
                path.AddString(item.Text, family, (int)style, emSize,
                    new PointF(item.X, item.Y), StringFormat.GenericDefault);
                bounds = path.GetBounds();
            }
            family.Dispose();
            return bounds;
        }

        private int HitTestTextItem(PointF imgPoint)
        {
            // Iterate from last to first (top-most items first)
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                RectangleF bounds = GetTextItemBounds(_items[i]);
                // Add some padding for easier clicking
                bounds.Inflate(6, 6);
                if (bounds.Contains(imgPoint))
                    return i;
            }
            return -1;
        }

        private void Preview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            PointF imgPoint = PictureBoxToImage(e.Location);

            // Hit-test: try to select the element under the cursor
            int hitIdx = HitTestTextItem(imgPoint);
            if (hitIdx >= 0 && hitIdx != _itemsList.SelectedIndex)
            {
                _itemsList.SelectedIndex = hitIdx;
            }

            if (_itemsList.SelectedIndex < 0) return;

            var item = _items[_itemsList.SelectedIndex];
            _dragging = true;
            _dragStart = e.Location;
            _dragItemStartX = item.X;
            _dragItemStartY = item.Y;
            _preview.Cursor = Cursors.SizeAll;
        }

        private void Preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            if (_itemsList.SelectedIndex < 0) return;

            PointF startImg = PictureBoxToImage(_dragStart);
            PointF nowImg = PictureBoxToImage(e.Location);

            var item = _items[_itemsList.SelectedIndex];
            item.X = _dragItemStartX + (nowImg.X - startImg.X);
            item.Y = _dragItemStartY + (nowImg.Y - startImg.Y);

            RenderPreview();
        }

        private void Preview_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            _preview.Cursor = Cursors.Default;
        }

        #endregion

        #region Keyboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_itemsList.SelectedIndex < 0 || _itemsList.SelectedIndex >= _items.Count)
                return base.ProcessCmdKey(ref msg, keyData);

            Keys key = keyData & Keys.KeyCode;
            bool shift = (keyData & Keys.Shift) != 0;
            float step = shift ? 1f : 5f;

            var item = _items[_itemsList.SelectedIndex];

            switch (key)
            {
                case Keys.Left:
                    item.X -= step;
                    RenderPreview();
                    return true;
                case Keys.Right:
                    item.X += step;
                    RenderPreview();
                    return true;
                case Keys.Up:
                    item.Y -= step;
                    RenderPreview();
                    return true;
                case Keys.Down:
                    item.Y += step;
                    RenderPreview();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Apply / Cancel

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Нет текстовых элементов для применения.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = new Bitmap(_sourceImage.Width, _sourceImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                g.DrawImage(_sourceImage, 0, 0, _sourceImage.Width, _sourceImage.Height);

                foreach (var item in _items)
                {
                    DrawTextItem(g, item, false);
                }
            }

            ResultImage = result;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion
    }
}
