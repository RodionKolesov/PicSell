using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace PicSell
{
    public class BackgroundEditorForm : Form
    {
        // ===================== ELEMENT CLASSES =====================

        private abstract class Element
        {
            public float X, Y;
            public string Name;
            public int Opacity = 255; // 0..255
            public abstract RectangleF Bounds { get; }
            public abstract void Draw(Graphics g, bool selected);
            public abstract Element Clone();
        }

        private class ImageElement : Element
        {
            public Image Source;
            public float Scale = 1.0f;
            public float Width { get { return Source.Width * Scale; } }
            public float Height { get { return Source.Height * Scale; } }
            public override RectangleF Bounds { get { return new RectangleF(X, Y, Width, Height); } }

            public override void Draw(Graphics g, bool selected)
            {
                if (Opacity < 255)
                {
                    var cm = new System.Drawing.Imaging.ColorMatrix();
                    cm.Matrix33 = Opacity / 255f;
                    var ia = new System.Drawing.Imaging.ImageAttributes();
                    ia.SetColorMatrix(cm);
                    g.DrawImage(Source, new Rectangle((int)X, (int)Y, (int)Width, (int)Height),
                        0, 0, Source.Width, Source.Height, GraphicsUnit.Pixel, ia);
                    ia.Dispose();
                }
                else
                {
                    g.DrawImage(Source, X, Y, Width, Height);
                }
                if (selected) DrawSelection(g, Bounds);
            }

            public override string ToString() { return $"[Фото] {Name} ({(int)(Scale * 100)}%)"; }

            public override Element Clone()
            {
                return new ImageElement { Source = Source, Scale = Scale, X = X + 20, Y = Y + 20, Name = Name + " копия", Opacity = Opacity };
            }
        }

        private class TextElement : Element
        {
            public string Text = "";
            public string FontFamily = "Arial";
            public float FontSize = 48;
            public bool Bold, Italic, Underline;
            public Color SolidColor = Color.White;
            public bool UseGradient;
            public Color GradientColor1 = Color.Red;
            public Color GradientColor2 = Color.Blue;
            public bool GradientVertical = true;
            // Shadow
            public bool HasShadow = false;
            public Color ShadowColor = Color.FromArgb(128, 0, 0, 0);
            public float ShadowX = 3, ShadowY = 3;
            // Outline
            public Color OutlineColor = Color.FromArgb(180, 0, 0, 0);
            public float OutlineWidth = 0;

            public override RectangleF Bounds
            {
                get { using (var path = BuildPath(0, 0)) return path.GetBounds(); }
            }

            public GraphicsPath BuildPath(float ox, float oy)
            {
                FontStyle style = FontStyle.Regular;
                if (Bold) style |= FontStyle.Bold;
                if (Italic) style |= FontStyle.Italic;
                if (Underline) style |= FontStyle.Underline;

                System.Drawing.FontFamily fam;
                try { fam = new System.Drawing.FontFamily(FontFamily); }
                catch { fam = new System.Drawing.FontFamily("Arial"); }

                if (!fam.IsStyleAvailable(style))
                {
                    if (fam.IsStyleAvailable(FontStyle.Regular)) style = FontStyle.Regular;
                    else if (fam.IsStyleAvailable(FontStyle.Bold)) style = FontStyle.Bold;
                }

                float emSize = FontSize * 96f / 72f;
                var path = new GraphicsPath();
                path.AddString(string.IsNullOrEmpty(Text) ? " " : Text,
                    fam, (int)style, emSize, new PointF(X + ox, Y + oy), StringFormat.GenericDefault);
                fam.Dispose();
                return path;
            }

            public override void Draw(Graphics g, bool selected)
            {
                if (string.IsNullOrEmpty(Text)) return;

                // Shadow
                if (HasShadow)
                {
                    using (var shadowPath = BuildPath(ShadowX, ShadowY))
                    using (var shadowBrush = new SolidBrush(ShadowColor))
                        g.FillPath(shadowBrush, shadowPath);
                }

                using (var path = BuildPath(0, 0))
                {
                    RectangleF bounds = path.GetBounds();
                    if (bounds.Width < 1 || bounds.Height < 1) return;

                    // Outline
                    if (OutlineWidth > 0)
                    {
                        using (var pen = new Pen(OutlineColor, OutlineWidth))
                        {
                            pen.LineJoin = LineJoin.Round;
                            g.DrawPath(pen, path);
                        }
                    }

                    // Fill
                    if (UseGradient)
                    {
                        var gr = new RectangleF(bounds.X - 1, bounds.Y - 1, bounds.Width + 2, bounds.Height + 2);
                        var mode = GradientVertical ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
                        using (var brush = new LinearGradientBrush(gr, GradientColor1, GradientColor2, mode))
                            g.FillPath(brush, path);
                    }
                    else
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(Opacity, SolidColor)))
                            g.FillPath(brush, path);
                    }

                    if (selected) DrawSelection(g, bounds);
                }
            }

            public override string ToString()
            {
                string t = Text != null && Text.Length > 15 ? Text.Substring(0, 15) + "..." : Text;
                return $"[Текст] {t}";
            }

            public override Element Clone()
            {
                return new TextElement
                {
                    Text = Text, FontFamily = FontFamily, FontSize = FontSize, Bold = Bold, Italic = Italic, Underline = Underline,
                    SolidColor = SolidColor, UseGradient = UseGradient, GradientColor1 = GradientColor1, GradientColor2 = GradientColor2,
                    GradientVertical = GradientVertical, HasShadow = HasShadow, ShadowColor = ShadowColor, ShadowX = ShadowX, ShadowY = ShadowY,
                    OutlineColor = OutlineColor, OutlineWidth = OutlineWidth, Opacity = Opacity,
                    X = X + 20, Y = Y + 20, Name = Name + " копия"
                };
            }
        }

        private class BadgeElement : Element
        {
            public string Text = "СКИДКА";
            public string FontFamily = "Arial";
            public float FontSize = 20;
            public bool Bold = true;
            public Color FillColor = Color.FromArgb(220, 40, 40);
            public Color TextColor = Color.White;
            public Color BorderColor = Color.Transparent;
            public float BorderWidth = 0;
            public float PadX = 18, PadY = 8;
            public float CornerRadius = 12;
            public float BadgeWidth = 0, BadgeHeight = 0; // 0 = auto

            private SizeF MeasureText(Graphics g)
            {
                FontStyle fs = Bold ? FontStyle.Bold : FontStyle.Regular;
                using (var f = new Font(FontFamily, FontSize, fs))
                    return g.MeasureString(Text ?? " ", f);
            }

            public override RectangleF Bounds
            {
                get
                {
                    using (var bmp = new Bitmap(1, 1))
                    using (var g = Graphics.FromImage(bmp))
                    {
                        var ts = MeasureText(g);
                        float w = BadgeWidth > 0 ? BadgeWidth : ts.Width + PadX * 2;
                        float h = BadgeHeight > 0 ? BadgeHeight : ts.Height + PadY * 2;
                        return new RectangleF(X, Y, w, h);
                    }
                }
            }

            public override void Draw(Graphics g, bool selected)
            {
                var ts = MeasureText(g);
                float w = BadgeWidth > 0 ? BadgeWidth : ts.Width + PadX * 2;
                float h = BadgeHeight > 0 ? BadgeHeight : ts.Height + PadY * 2;
                var rect = new RectangleF(X, Y, w, h);

                using (var path = RoundedRectPath(rect, CornerRadius))
                {
                    using (var brush = new SolidBrush(Color.FromArgb(Opacity, FillColor)))
                        g.FillPath(brush, path);
                    if (BorderWidth > 0 && BorderColor.A > 0)
                        using (var pen = new Pen(BorderColor, BorderWidth))
                            g.DrawPath(pen, path);
                }

                // Draw text centered
                FontStyle fs = Bold ? FontStyle.Bold : FontStyle.Regular;
                using (var f = new Font(FontFamily, FontSize, fs))
                using (var brush = new SolidBrush(TextColor))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(Text ?? "", f, brush, new RectangleF(X, Y, w, h), sf);
                }

                if (selected) DrawSelection(g, rect);
            }

            public override string ToString()
            {
                string t = Text != null && Text.Length > 12 ? Text.Substring(0, 12) + "..." : Text;
                return $"[Плашка] {t}";
            }

            public override Element Clone()
            {
                return new BadgeElement
                {
                    Text = Text, FontFamily = FontFamily, FontSize = FontSize, Bold = Bold,
                    FillColor = FillColor, TextColor = TextColor, BorderColor = BorderColor, BorderWidth = BorderWidth,
                    PadX = PadX, PadY = PadY, CornerRadius = CornerRadius, BadgeWidth = BadgeWidth, BadgeHeight = BadgeHeight,
                    Opacity = Opacity, X = X + 20, Y = Y + 20, Name = Name + " копия"
                };
            }
        }

        private static GraphicsPath RoundedRectPath(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            if (d > r.Height) d = r.Height;
            if (d > r.Width) d = r.Width;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void DrawSelection(Graphics g, RectangleF bounds)
        {
            using (var pen = new Pen(Color.FromArgb(200, DarkTheme.Accent), 2f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawRectangle(pen, bounds.X - 3, bounds.Y - 3, bounds.Width + 6, bounds.Height + 6);
            }
        }

        // ===================== FIELDS =====================

        private Image _background;
        private readonly List<Element> _elements = new List<Element>();
        private int _selectedIndex = -1;

        // Background gradient
        private bool _bgGradient = false;
        private Color _bgColor1 = Color.FromArgb(240, 228, 200);
        private Color _bgColor2 = Color.FromArgb(200, 180, 150);
        private bool _bgGradVertical = true;

        private PictureBox _preview;
        private Button _applyButton, _cancelButton;
        private Button _addImageBtn, _addTextBtn, _addBadgeBtn, _deleteBtn;
        private Button _dupBtn, _moveUpBtn, _moveDownBtn;
        private ListBox _elementsList;

        // Image size controls
        private NumericUpDown _widthInput, _heightInput;
        private bool _suppressSizeSync;

        // Text controls
        private Panel _textPropsPanel;
        private TextBox _txtInput;
        private ComboBox _fontCombo;
        private NumericUpDown _fontSizeInput;
        private Button _boldBtn, _italicBtn, _underlineBtn;
        private Button _colorBtn;
        private CheckBox _gradientCheck;
        private Button _gradColor1Btn, _gradColor2Btn;
        private ComboBox _gradDirCombo;
        private CheckBox _shadowCheck;
        private Button _shadowColorBtn;
        private NumericUpDown _shadowXInput, _shadowYInput;
        private Button _outlineColorBtn;
        private NumericUpDown _outlineWidthInput;

        // Badge controls
        private Panel _badgePropsPanel;
        private TextBox _badgeTxtInput;
        private NumericUpDown _badgeFontSize;
        private Button _badgeFillColorBtn, _badgeTextColorBtn, _badgeBorderColorBtn;
        private NumericUpDown _badgeCornerInput, _badgePadXInput, _badgePadYInput, _badgeBorderWInput;
        private CheckBox _badgeBoldCheck;

        // Image size panel
        private Panel _imgPropsPanel;

        // Bg gradient controls
        private CheckBox _bgGradCheck;
        private Button _bgColor1Btn, _bgColor2Btn;
        private ComboBox _bgGradDirCombo;

        // Drag
        private bool _dragging;
        private Point _dragStart;
        private float _dragStartX, _dragStartY;
        private bool _suppressTextSync;
        private bool _suppressBadgeSync;

        public Image ResultImage { get; private set; }

        public BackgroundEditorForm(Image foreground, Image background)
        {
            _background = background;
            InitUI();

            if (foreground != null)
            {
                var elem = new ImageElement
                {
                    Source = foreground,
                    X = (_background.Width - foreground.Width) / 2f,
                    Y = (_background.Height - foreground.Height) / 2f,
                    Name = "Товар"
                };
                _elements.Add(elem);
                _elementsList.Items.Add(elem);
                _elementsList.SelectedIndex = 0;
            }

            RenderPreview();
        }

        private void InitUI()
        {
            this.Text = "Редактор изображения";
            this.Size = new Size(1200, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 650);
            this.KeyPreview = true;

            // === Right panel (scrollable) ===
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 290, AutoScroll = true };
            int y = 8;

            // --- Background gradient ---
            AddLabel("Фон холста:", rightPanel, ref y, true);
            _bgGradCheck = new CheckBox { Location = new Point(8, y), Size = new Size(256, 22), Text = "Градиентный фон", ForeColor = DarkTheme.Text, BackColor = Color.Transparent };
            _bgGradCheck.CheckedChanged += (s, ev) => { _bgGradient = _bgGradCheck.Checked; UpdateBgGradVis(); RenderPreview(); };
            rightPanel.Controls.Add(_bgGradCheck);
            y += 24;

            _bgColor1Btn = new Button { Location = new Point(8, y), Size = new Size(122, 26), BackColor = _bgColor1, FlatStyle = FlatStyle.Flat, Text = "Цвет 1", ForeColor = Color.Black, Cursor = Cursors.Hand, Visible = false };
            _bgColor1Btn.FlatAppearance.BorderColor = DarkTheme.Border;
            _bgColor1Btn.Click += (s, ev) => { using (var cd = new ColorDialog { Color = _bgColor1Btn.BackColor, FullOpen = true }) { if (cd.ShowDialog() == DialogResult.OK) { _bgColor1Btn.BackColor = cd.Color; _bgColor1 = cd.Color; RenderPreview(); } } };
            rightPanel.Controls.Add(_bgColor1Btn);

            _bgColor2Btn = new Button { Location = new Point(138, y), Size = new Size(126, 26), BackColor = _bgColor2, FlatStyle = FlatStyle.Flat, Text = "Цвет 2", ForeColor = Color.Black, Cursor = Cursors.Hand, Visible = false };
            _bgColor2Btn.FlatAppearance.BorderColor = DarkTheme.Border;
            _bgColor2Btn.Click += (s, ev) => { using (var cd = new ColorDialog { Color = _bgColor2Btn.BackColor, FullOpen = true }) { if (cd.ShowDialog() == DialogResult.OK) { _bgColor2Btn.BackColor = cd.Color; _bgColor2 = cd.Color; RenderPreview(); } } };
            rightPanel.Controls.Add(_bgColor2Btn);
            y += 30;

            _bgGradDirCombo = new ComboBox { Location = new Point(8, y), Size = new Size(256, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, Visible = false };
            _bgGradDirCombo.Items.AddRange(new object[] { "Вертикальный", "Горизонтальный" });
            _bgGradDirCombo.SelectedIndex = 0;
            _bgGradDirCombo.SelectedIndexChanged += (s, ev) => { _bgGradVertical = _bgGradDirCombo.SelectedIndex == 0; RenderPreview(); };
            rightPanel.Controls.Add(_bgGradDirCombo);
            y += 30;

            // --- Elements list ---
            AddLabel("Элементы:", rightPanel, ref y, true);
            _elementsList = new ListBox
            {
                Location = new Point(8, y), Size = new Size(268, 110),
                BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text,
                BorderStyle = BorderStyle.FixedSingle
            };
            _elementsList.SelectedIndexChanged += ElementsList_Changed;
            rightPanel.Controls.Add(_elementsList);
            y += 116;

            // Add buttons row 1
            _addImageBtn = MakeBtn("+ Фото", new Point(8, y), new Size(82, 28));
            _addImageBtn.Click += AddImageBtn_Click;
            rightPanel.Controls.Add(_addImageBtn);

            _addTextBtn = MakeBtn("+ Текст", new Point(96, y), new Size(82, 28));
            _addTextBtn.BackColor = DarkTheme.Accent;
            _addTextBtn.Click += AddTextBtn_Click;
            rightPanel.Controls.Add(_addTextBtn);

            _addBadgeBtn = MakeBtn("+ Плашка", new Point(184, y), new Size(82, 28));
            _addBadgeBtn.BackColor = Color.FromArgb(180, 50, 50);
            _addBadgeBtn.Click += AddBadgeBtn_Click;
            rightPanel.Controls.Add(_addBadgeBtn);
            y += 32;

            // Row 2: duplicate, delete, layer order
            _dupBtn = MakeBtn("Копия", new Point(8, y), new Size(62, 26));
            _dupBtn.Click += DuplicateBtn_Click;
            rightPanel.Controls.Add(_dupBtn);

            _deleteBtn = MakeBtn("Удалить", new Point(74, y), new Size(62, 26));
            _deleteBtn.Click += DeleteBtn_Click;
            rightPanel.Controls.Add(_deleteBtn);

            _moveUpBtn = MakeBtn("\u25B2", new Point(142, y), new Size(40, 26));
            _moveUpBtn.Click += (s, ev) => MoveElement(-1);
            rightPanel.Controls.Add(_moveUpBtn);

            _moveDownBtn = MakeBtn("\u25BC", new Point(186, y), new Size(40, 26));
            _moveDownBtn.Click += (s, ev) => MoveElement(1);
            rightPanel.Controls.Add(_moveDownBtn);

            var centerH = MakeBtn("\u2194", new Point(232, y), new Size(34, 26));
            centerH.Click += (s, ev) => AlignSelected(true);
            rightPanel.Controls.Add(centerH);

            // Removed centerV from this row, put it below or combine
            y += 30;

            var centerV = MakeBtn("\u2195 Центр верт.", new Point(8, y), new Size(120, 26));
            centerV.Click += (s, ev) => AlignSelected(false);
            rightPanel.Controls.Add(centerV);
            y += 32;

            // ====== Image properties ======
            _imgPropsPanel = new Panel { Location = new Point(0, y), Size = new Size(280, 50), Visible = false };
            int iy = 0;
            AddLabel("Размер:", _imgPropsPanel, ref iy, false);

            _imgPropsPanel.Controls.Add(new Label { Text = "Ш:", Location = new Point(8, iy + 3), AutoSize = true, ForeColor = DarkTheme.Text, Font = new Font("Segoe UI", 9F) });
            _widthInput = new NumericUpDown { Location = new Point(28, iy), Size = new Size(80, 25), Minimum = 10, Maximum = 10000, Value = 100, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _widthInput.ValueChanged += WidthInput_Changed;
            _imgPropsPanel.Controls.Add(_widthInput);

            _imgPropsPanel.Controls.Add(new Label { Text = "В:", Location = new Point(118, iy + 3), AutoSize = true, ForeColor = DarkTheme.Text, Font = new Font("Segoe UI", 9F) });
            _heightInput = new NumericUpDown { Location = new Point(138, iy), Size = new Size(80, 25), Minimum = 10, Maximum = 10000, Value = 100, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _heightInput.ValueChanged += HeightInput_Changed;
            _imgPropsPanel.Controls.Add(_heightInput);
            rightPanel.Controls.Add(_imgPropsPanel);

            // ====== Text properties ======
            _textPropsPanel = new Panel { Location = new Point(0, y), Size = new Size(280, 480), Visible = false };
            int ty = 0;

            AddLabel("Текст:", _textPropsPanel, ref ty, false);
            _txtInput = new TextBox { Location = new Point(8, ty), Size = new Size(260, 50), Multiline = true, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, BorderStyle = BorderStyle.FixedSingle };
            _txtInput.TextChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_txtInput);
            ty += 56;

            AddLabel("Шрифт:", _textPropsPanel, ref ty, false);
            _fontCombo = new ComboBox { Location = new Point(8, ty), Size = new Size(260, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            foreach (var ff in FontFamily.Families) _fontCombo.Items.Add(ff.Name);
            int ai = _fontCombo.Items.IndexOf("Arial");
            _fontCombo.SelectedIndex = ai >= 0 ? ai : 0;
            _fontCombo.SelectedIndexChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_fontCombo);
            ty += 30;

            AddLabel("Размер:", _textPropsPanel, ref ty, false);
            _fontSizeInput = new NumericUpDown { Location = new Point(8, ty), Size = new Size(100, 25), Minimum = 8, Maximum = 500, Value = 48, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _fontSizeInput.ValueChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_fontSizeInput);
            ty += 30;

            _boldBtn = MakeToggle("B", new Point(8, ty), FontStyle.Bold);
            _italicBtn = MakeToggle("I", new Point(48, ty), FontStyle.Italic);
            _underlineBtn = MakeToggle("U", new Point(88, ty), FontStyle.Regular);
            _underlineBtn.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
            _boldBtn.Click += OnStyleToggle; _italicBtn.Click += OnStyleToggle; _underlineBtn.Click += OnStyleToggle;
            _textPropsPanel.Controls.Add(_boldBtn); _textPropsPanel.Controls.Add(_italicBtn); _textPropsPanel.Controls.Add(_underlineBtn);
            ty += 34;

            AddLabel("Цвет:", _textPropsPanel, ref ty, false);
            _colorBtn = MakeColorBtn(new Point(8, ty), new Size(260, 24), Color.White);
            _colorBtn.Click += (s, ev) => { PickColor(_colorBtn); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_colorBtn);
            ty += 28;

            _gradientCheck = new CheckBox { Location = new Point(8, ty), Size = new Size(260, 20), Text = "Градиент текста", ForeColor = DarkTheme.Text, BackColor = Color.Transparent };
            _gradientCheck.CheckedChanged += (s, ev) => { UpdateGradVis(); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_gradientCheck);
            ty += 22;

            _gradColor1Btn = MakeColorBtn(new Point(8, ty), new Size(122, 24), Color.Red);
            _gradColor1Btn.Text = "Цвет 1"; _gradColor1Btn.Visible = false;
            _gradColor1Btn.Click += (s, ev) => { PickColor(_gradColor1Btn); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_gradColor1Btn);

            _gradColor2Btn = MakeColorBtn(new Point(138, ty), new Size(126, 24), Color.Blue);
            _gradColor2Btn.Text = "Цвет 2"; _gradColor2Btn.Visible = false;
            _gradColor2Btn.Click += (s, ev) => { PickColor(_gradColor2Btn); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_gradColor2Btn);
            ty += 28;

            _gradDirCombo = new ComboBox { Location = new Point(8, ty), Size = new Size(260, 24), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, Visible = false };
            _gradDirCombo.Items.AddRange(new object[] { "Вертикальный", "Горизонтальный" });
            _gradDirCombo.SelectedIndex = 0;
            _gradDirCombo.SelectedIndexChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_gradDirCombo);
            ty += 28;

            // Outline
            AddLabel("Контур:", _textPropsPanel, ref ty, false);
            _outlineColorBtn = MakeColorBtn(new Point(8, ty), new Size(140, 24), Color.Black);
            _outlineColorBtn.Text = "Цвет контура";
            _outlineColorBtn.Click += (s, ev) => { PickColor(_outlineColorBtn); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_outlineColorBtn);

            _outlineWidthInput = new NumericUpDown { Location = new Point(156, ty), Size = new Size(60, 24), Minimum = 0, Maximum = 30, Value = 0, DecimalPlaces = 1, Increment = 0.5M, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _outlineWidthInput.ValueChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_outlineWidthInput);
            _textPropsPanel.Controls.Add(new Label { Text = "px", Location = new Point(220, ty + 3), AutoSize = true, ForeColor = DarkTheme.DimText, BackColor = Color.Transparent });
            ty += 28;

            // Shadow
            _shadowCheck = new CheckBox { Location = new Point(8, ty), Size = new Size(260, 20), Text = "Тень", ForeColor = DarkTheme.Text, BackColor = Color.Transparent };
            _shadowCheck.CheckedChanged += (s, ev) => { UpdateShadowVis(); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_shadowCheck);
            ty += 22;

            _shadowColorBtn = MakeColorBtn(new Point(8, ty), new Size(120, 24), Color.FromArgb(128, 0, 0, 0));
            _shadowColorBtn.Text = "Цвет тени"; _shadowColorBtn.Visible = false;
            _shadowColorBtn.Click += (s, ev) => { PickColor(_shadowColorBtn); OnTextPropChanged(s, ev); };
            _textPropsPanel.Controls.Add(_shadowColorBtn);

            _shadowXInput = new NumericUpDown { Location = new Point(136, ty), Size = new Size(55, 24), Minimum = -20, Maximum = 20, Value = 3, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, Visible = false };
            _shadowXInput.ValueChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_shadowXInput);

            _shadowYInput = new NumericUpDown { Location = new Point(198, ty), Size = new Size(55, 24), Minimum = -20, Maximum = 20, Value = 3, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, Visible = false };
            _shadowYInput.ValueChanged += OnTextPropChanged;
            _textPropsPanel.Controls.Add(_shadowYInput);

            rightPanel.Controls.Add(_textPropsPanel);

            // ====== Badge properties ======
            _badgePropsPanel = new Panel { Location = new Point(0, y), Size = new Size(280, 300), Visible = false };
            int by = 0;

            AddLabel("Текст плашки:", _badgePropsPanel, ref by, false);
            _badgeTxtInput = new TextBox { Location = new Point(8, by), Size = new Size(260, 25), BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text, BorderStyle = BorderStyle.FixedSingle };
            _badgeTxtInput.TextChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgeTxtInput);
            by += 30;

            AddLabel("Размер шрифта:", _badgePropsPanel, ref by, false);
            _badgeFontSize = new NumericUpDown { Location = new Point(8, by), Size = new Size(80, 25), Minimum = 8, Maximum = 200, Value = 20, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _badgeFontSize.ValueChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgeFontSize);
            _badgeBoldCheck = new CheckBox { Location = new Point(100, by + 2), Size = new Size(100, 20), Text = "Жирный", ForeColor = DarkTheme.Text, BackColor = Color.Transparent, Checked = true };
            _badgeBoldCheck.CheckedChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgeBoldCheck);
            by += 28;

            AddLabel("Цвет фона:", _badgePropsPanel, ref by, false);
            _badgeFillColorBtn = MakeColorBtn(new Point(8, by), new Size(126, 24), Color.FromArgb(220, 40, 40));
            _badgeFillColorBtn.Click += (s, ev) => { PickColor(_badgeFillColorBtn); OnBadgePropChanged(s, ev); };
            _badgePropsPanel.Controls.Add(_badgeFillColorBtn);

            _badgeTextColorBtn = MakeColorBtn(new Point(140, by), new Size(126, 24), Color.White);
            _badgeTextColorBtn.Text = "Цвет текста";
            _badgeTextColorBtn.Click += (s, ev) => { PickColor(_badgeTextColorBtn); OnBadgePropChanged(s, ev); };
            _badgePropsPanel.Controls.Add(_badgeTextColorBtn);
            by += 28;

            AddLabel("Рамка:", _badgePropsPanel, ref by, false);
            _badgeBorderColorBtn = MakeColorBtn(new Point(8, by), new Size(126, 24), Color.Transparent);
            _badgeBorderColorBtn.Text = "Цвет рамки";
            _badgeBorderColorBtn.Click += (s, ev) => { PickColor(_badgeBorderColorBtn); OnBadgePropChanged(s, ev); };
            _badgePropsPanel.Controls.Add(_badgeBorderColorBtn);

            _badgeBorderWInput = new NumericUpDown { Location = new Point(140, by), Size = new Size(60, 24), Minimum = 0, Maximum = 10, Value = 0, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _badgeBorderWInput.ValueChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgeBorderWInput);
            by += 28;

            AddLabel("Скругление:", _badgePropsPanel, ref by, false);
            _badgeCornerInput = new NumericUpDown { Location = new Point(8, by), Size = new Size(70, 24), Minimum = 0, Maximum = 100, Value = 12, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _badgeCornerInput.ValueChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgeCornerInput);

            _badgePropsPanel.Controls.Add(new Label { Text = "Отступ X/Y:", Location = new Point(90, by + 3), AutoSize = true, ForeColor = DarkTheme.DimText, BackColor = Color.Transparent });
            _badgePadXInput = new NumericUpDown { Location = new Point(170, by), Size = new Size(45, 24), Minimum = 0, Maximum = 100, Value = 18, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _badgePadXInput.ValueChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgePadXInput);
            _badgePadYInput = new NumericUpDown { Location = new Point(220, by), Size = new Size(45, 24), Minimum = 0, Maximum = 100, Value = 8, BackColor = DarkTheme.InputBg, ForeColor = DarkTheme.Text };
            _badgePadYInput.ValueChanged += OnBadgePropChanged;
            _badgePropsPanel.Controls.Add(_badgePadYInput);

            rightPanel.Controls.Add(_badgePropsPanel);

            // === Bottom panel ===
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            _applyButton = new Button { Text = "Применить", Size = new Size(120, 34) };
            _applyButton.Click += ApplyButton_Click;
            _cancelButton = new Button { Text = "Отмена", Size = new Size(100, 34), DialogResult = DialogResult.Cancel };

            var hint = new Label { Text = "Мышь — перемещение • Колесо — масштаб • Стрелки — сдвиг (Shift=1px)", AutoSize = true, ForeColor = DarkTheme.DimText, Font = new Font("Segoe UI", 8F, FontStyle.Italic) };
            bottomPanel.Controls.Add(_applyButton); bottomPanel.Controls.Add(_cancelButton); bottomPanel.Controls.Add(hint);
            bottomPanel.Resize += (s, e) =>
            {
                _applyButton.Location = new Point(bottomPanel.Width - _applyButton.Width - _cancelButton.Width - 24, (bottomPanel.Height - _applyButton.Height) / 2);
                _cancelButton.Location = new Point(bottomPanel.Width - _cancelButton.Width - 12, (bottomPanel.Height - _cancelButton.Height) / 2);
                hint.Location = new Point(12, (bottomPanel.Height - hint.Height) / 2);
            };

            // === Preview ===
            _preview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = DarkTheme.DarkBg };
            _preview.MouseDown += Preview_MouseDown;
            _preview.MouseMove += Preview_MouseMove;
            _preview.MouseUp += Preview_MouseUp;
            _preview.MouseWheel += Preview_MouseWheel;

            var splitter = new Splitter { Dock = DockStyle.Right, Width = 4, BackColor = DarkTheme.Border };

            this.Controls.Add(_preview);
            this.Controls.Add(splitter);
            this.Controls.Add(rightPanel);
            this.Controls.Add(bottomPanel);
            this.CancelButton = _cancelButton;

            DarkTheme.Apply(this);
            DarkTheme.StyleButton(_applyButton);
            DarkTheme.StyleButton(_cancelButton);
            _applyButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            _applyButton.BackColor = DarkTheme.Accent;
            _addTextBtn.BackColor = DarkTheme.Accent;
            _addBadgeBtn.BackColor = Color.FromArgb(180, 50, 50);
        }

        #region UI Helpers

        private void AddLabel(string text, Control parent, ref int y, bool bold)
        {
            parent.Controls.Add(new Label
            {
                Text = text, Location = new Point(8, y), AutoSize = true,
                ForeColor = DarkTheme.Text, BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular)
            });
            y += 18;
        }

        private Button MakeBtn(string text, Point loc, Size size)
        {
            var btn = new Button { Text = text, Location = loc, Size = size, Cursor = Cursors.Hand };
            DarkTheme.StyleButton(btn);
            return btn;
        }

        private Button MakeToggle(string text, Point loc, FontStyle style)
        {
            var btn = new Button
            {
                Text = text, Location = loc, Size = new Size(34, 28),
                FlatStyle = FlatStyle.Flat, BackColor = DarkTheme.BtnBg, ForeColor = DarkTheme.Text,
                Font = new Font("Segoe UI", 9F, style), Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = DarkTheme.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = DarkTheme.BtnHover;
            return btn;
        }

        private Button MakeColorBtn(Point loc, Size size, Color color)
        {
            var btn = new Button { Location = loc, Size = size, BackColor = color, FlatStyle = FlatStyle.Flat, Text = "", Cursor = Cursors.Hand, ForeColor = Color.White };
            btn.FlatAppearance.BorderColor = DarkTheme.Border;
            return btn;
        }

        private void PickColor(Button btn)
        {
            using (var cd = new ColorDialog { Color = btn.BackColor, FullOpen = true })
                if (cd.ShowDialog() == DialogResult.OK) btn.BackColor = cd.Color;
        }

        private void UpdateGradVis()
        {
            bool v = _gradientCheck.Checked;
            _gradColor1Btn.Visible = v; _gradColor2Btn.Visible = v; _gradDirCombo.Visible = v;
        }

        private void UpdateShadowVis()
        {
            bool v = _shadowCheck.Checked;
            _shadowColorBtn.Visible = v; _shadowXInput.Visible = v; _shadowYInput.Visible = v;
        }

        private void UpdateBgGradVis()
        {
            bool v = _bgGradCheck.Checked;
            _bgColor1Btn.Visible = v; _bgColor2Btn.Visible = v; _bgGradDirCombo.Visible = v;
        }

        private void AlignSelected(bool horizontal)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _elements.Count) return;
            var elem = _elements[_selectedIndex];
            var b = elem.Bounds;
            if (horizontal) elem.X += (_background.Width - b.Width) / 2f - b.X + elem.X;
            else elem.Y += (_background.Height - b.Height) / 2f - b.Y + elem.Y;
            RenderPreview();
        }

        private void MoveElement(int dir)
        {
            if (_selectedIndex < 0) return;
            int newIdx = _selectedIndex + dir;
            if (newIdx < 0 || newIdx >= _elements.Count) return;
            var tmp = _elements[_selectedIndex];
            _elements[_selectedIndex] = _elements[newIdx];
            _elements[newIdx] = tmp;
            RefreshElementsList();
            _elementsList.SelectedIndex = newIdx;
        }

        private void RefreshElementsList()
        {
            _elementsList.Items.Clear();
            foreach (var e in _elements) _elementsList.Items.Add(e);
        }

        #endregion

        #region Elements management

        private int _counter = 1;

        private void AddImageBtn_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Title = "Выберите изображение", Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    Image img = Image.FromFile(ofd.FileName);
                    var elem = new ImageElement
                    {
                        Source = img,
                        X = (_background.Width - img.Width) / 2f,
                        Y = (_background.Height - img.Height) / 2f,
                        Name = "Фото " + (++_counter)
                    };
                    _elements.Add(elem);
                    _elementsList.Items.Add(elem);
                    _elementsList.SelectedIndex = _elementsList.Items.Count - 1;
                    RenderPreview();
                }
                catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
            }
        }

        private void AddTextBtn_Click(object sender, EventArgs e)
        {
            var elem = new TextElement
            {
                Text = "Текст",
                X = _background.Width / 2f - 100,
                Y = _background.Height / 2f - 30,
                Name = "Текст " + (++_counter)
            };
            _elements.Add(elem);
            _elementsList.Items.Add(elem);
            _elementsList.SelectedIndex = _elementsList.Items.Count - 1;
            RenderPreview();
        }

        private void AddBadgeBtn_Click(object sender, EventArgs e)
        {
            var elem = new BadgeElement
            {
                Text = "СКИДКА",
                X = 30, Y = 30,
                Name = "Плашка " + (++_counter)
            };
            _elements.Add(elem);
            _elementsList.Items.Add(elem);
            _elementsList.SelectedIndex = _elementsList.Items.Count - 1;
            RenderPreview();
        }

        private void DuplicateBtn_Click(object sender, EventArgs e)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _elements.Count) return;
            var clone = _elements[_selectedIndex].Clone();
            _elements.Add(clone);
            _elementsList.Items.Add(clone);
            _elementsList.SelectedIndex = _elementsList.Items.Count - 1;
            RenderPreview();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (_selectedIndex < 0) return;
            _elements.RemoveAt(_selectedIndex);
            _elementsList.Items.RemoveAt(_selectedIndex);
            if (_elementsList.Items.Count > 0)
                _elementsList.SelectedIndex = Math.Min(_selectedIndex, _elementsList.Items.Count - 1);
            else _selectedIndex = -1;
            ShowPropsForSelected();
            RenderPreview();
        }

        private void ElementsList_Changed(object sender, EventArgs e)
        {
            _selectedIndex = _elementsList.SelectedIndex;
            ShowPropsForSelected();
            RenderPreview();
        }

        private void ShowPropsForSelected()
        {
            _imgPropsPanel.Visible = false;
            _textPropsPanel.Visible = false;
            _badgePropsPanel.Visible = false;

            if (_selectedIndex < 0 || _selectedIndex >= _elements.Count) return;
            var elem = _elements[_selectedIndex];

            if (elem is ImageElement ie)
            {
                _imgPropsPanel.Visible = true;
                _suppressSizeSync = true;
                _widthInput.Value = Math.Max(_widthInput.Minimum, Math.Min(_widthInput.Maximum, (decimal)ie.Width));
                _heightInput.Value = Math.Max(_heightInput.Minimum, Math.Min(_heightInput.Maximum, (decimal)ie.Height));
                _suppressSizeSync = false;
            }
            else if (elem is TextElement te)
            {
                _textPropsPanel.Visible = true;
                _suppressTextSync = true;
                _txtInput.Text = te.Text;
                int fi = _fontCombo.Items.IndexOf(te.FontFamily);
                if (fi >= 0) _fontCombo.SelectedIndex = fi;
                _fontSizeInput.Value = (decimal)Math.Max(8, Math.Min(500, te.FontSize));
                _boldBtn.BackColor = te.Bold ? DarkTheme.Accent : DarkTheme.BtnBg;
                _italicBtn.BackColor = te.Italic ? DarkTheme.Accent : DarkTheme.BtnBg;
                _underlineBtn.BackColor = te.Underline ? DarkTheme.Accent : DarkTheme.BtnBg;
                _colorBtn.BackColor = te.SolidColor;
                _gradientCheck.Checked = te.UseGradient;
                _gradColor1Btn.BackColor = te.GradientColor1;
                _gradColor2Btn.BackColor = te.GradientColor2;
                _gradDirCombo.SelectedIndex = te.GradientVertical ? 0 : 1;
                _shadowCheck.Checked = te.HasShadow;
                _shadowColorBtn.BackColor = te.ShadowColor;
                _shadowXInput.Value = (decimal)te.ShadowX;
                _shadowYInput.Value = (decimal)te.ShadowY;
                _outlineColorBtn.BackColor = te.OutlineColor;
                _outlineWidthInput.Value = (decimal)Math.Max(0, Math.Min(30, te.OutlineWidth));
                UpdateGradVis();
                UpdateShadowVis();
                _suppressTextSync = false;
            }
            else if (elem is BadgeElement be)
            {
                _badgePropsPanel.Visible = true;
                _suppressBadgeSync = true;
                _badgeTxtInput.Text = be.Text;
                _badgeFontSize.Value = (decimal)Math.Max(8, Math.Min(200, be.FontSize));
                _badgeBoldCheck.Checked = be.Bold;
                _badgeFillColorBtn.BackColor = be.FillColor;
                _badgeTextColorBtn.BackColor = be.TextColor;
                _badgeBorderColorBtn.BackColor = be.BorderColor;
                _badgeBorderWInput.Value = (decimal)be.BorderWidth;
                _badgeCornerInput.Value = (decimal)be.CornerRadius;
                _badgePadXInput.Value = (decimal)be.PadX;
                _badgePadYInput.Value = (decimal)be.PadY;
                _suppressBadgeSync = false;
            }
        }

        #endregion

        #region Property handlers

        private void WidthInput_Changed(object sender, EventArgs e)
        {
            if (_suppressSizeSync || _selectedIndex < 0) return;
            if (!(_elements[_selectedIndex] is ImageElement ie)) return;
            ie.Scale = (float)_widthInput.Value / ie.Source.Width;
            _suppressSizeSync = true;
            _heightInput.Value = Math.Max(_heightInput.Minimum, Math.Min(_heightInput.Maximum, (decimal)ie.Height));
            _suppressSizeSync = false;
            _elementsList.Items[_selectedIndex] = ie;
            RenderPreview();
        }

        private void HeightInput_Changed(object sender, EventArgs e)
        {
            if (_suppressSizeSync || _selectedIndex < 0) return;
            if (!(_elements[_selectedIndex] is ImageElement ie)) return;
            ie.Scale = (float)_heightInput.Value / ie.Source.Height;
            _suppressSizeSync = true;
            _widthInput.Value = Math.Max(_widthInput.Minimum, Math.Min(_widthInput.Maximum, (decimal)ie.Width));
            _suppressSizeSync = false;
            _elementsList.Items[_selectedIndex] = ie;
            RenderPreview();
        }

        private void OnStyleToggle(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.BackColor = btn.BackColor == DarkTheme.Accent ? DarkTheme.BtnBg : DarkTheme.Accent;
            OnTextPropChanged(sender, e);
        }

        private void OnTextPropChanged(object sender, EventArgs e)
        {
            if (_suppressTextSync) return;
            if (_selectedIndex < 0 || !(_elements[_selectedIndex] is TextElement te)) return;

            te.Text = _txtInput.Text;
            te.FontFamily = _fontCombo.SelectedItem?.ToString() ?? "Arial";
            te.FontSize = (float)_fontSizeInput.Value;
            te.Bold = _boldBtn.BackColor == DarkTheme.Accent;
            te.Italic = _italicBtn.BackColor == DarkTheme.Accent;
            te.Underline = _underlineBtn.BackColor == DarkTheme.Accent;
            te.SolidColor = _colorBtn.BackColor;
            te.UseGradient = _gradientCheck.Checked;
            te.GradientColor1 = _gradColor1Btn.BackColor;
            te.GradientColor2 = _gradColor2Btn.BackColor;
            te.GradientVertical = _gradDirCombo.SelectedIndex == 0;
            te.HasShadow = _shadowCheck.Checked;
            te.ShadowColor = _shadowColorBtn.BackColor;
            te.ShadowX = (float)_shadowXInput.Value;
            te.ShadowY = (float)_shadowYInput.Value;
            te.OutlineColor = _outlineColorBtn.BackColor;
            te.OutlineWidth = (float)_outlineWidthInput.Value;

            _elementsList.Items[_selectedIndex] = te;
            RenderPreview();
        }

        private void OnBadgePropChanged(object sender, EventArgs e)
        {
            if (_suppressBadgeSync) return;
            if (_selectedIndex < 0 || !(_elements[_selectedIndex] is BadgeElement be)) return;

            be.Text = _badgeTxtInput.Text;
            be.FontSize = (float)_badgeFontSize.Value;
            be.Bold = _badgeBoldCheck.Checked;
            be.FillColor = _badgeFillColorBtn.BackColor;
            be.TextColor = _badgeTextColorBtn.BackColor;
            be.BorderColor = _badgeBorderColorBtn.BackColor;
            be.BorderWidth = (float)_badgeBorderWInput.Value;
            be.CornerRadius = (float)_badgeCornerInput.Value;
            be.PadX = (float)_badgePadXInput.Value;
            be.PadY = (float)_badgePadYInput.Value;

            _elementsList.Items[_selectedIndex] = be;
            RenderPreview();
        }

        #endregion

        #region Keyboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _elements.Count)
                return base.ProcessCmdKey(ref msg, keyData);

            Keys key = keyData & Keys.KeyCode;
            bool shift = (keyData & Keys.Shift) != 0;
            float step = shift ? 1f : 5f;
            var elem = _elements[_selectedIndex];

            switch (key)
            {
                case Keys.Left: elem.X -= step; RenderPreview(); return true;
                case Keys.Right: elem.X += step; RenderPreview(); return true;
                case Keys.Up: elem.Y -= step; RenderPreview(); return true;
                case Keys.Down: elem.Y += step; RenderPreview(); return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #endregion

        #region Rendering

        private void RenderPreview()
        {
            int w = _background.Width, h = _background.Height;
            var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                // Background
                if (_bgGradient)
                {
                    var mode = _bgGradVertical ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, w, h), _bgColor1, _bgColor2, mode))
                        g.FillRectangle(brush, 0, 0, w, h);
                }
                else
                {
                    g.DrawImage(_background, 0, 0, w, h);
                }

                for (int i = 0; i < _elements.Count; i++)
                    _elements[i].Draw(g, i == _selectedIndex);
            }

            var old = _preview.Image;
            _preview.Image = bmp;
            old?.Dispose();
        }

        #endregion

        #region Mouse

        private PointF PictureBoxToImage(Point pb)
        {
            if (_preview.Image == null) return PointF.Empty;
            int iw = _preview.Image.Width, ih = _preview.Image.Height;
            int pw = _preview.ClientSize.Width, ph = _preview.ClientSize.Height;
            float r = Math.Min((float)pw / iw, (float)ph / ih);
            float dx = (pw - iw * r) / 2f, dy = (ph - ih * r) / 2f;
            return new PointF((pb.X - dx) / r, (pb.Y - dy) / r);
        }

        private int HitTest(PointF p)
        {
            for (int i = _elements.Count - 1; i >= 0; i--)
            {
                var b = _elements[i].Bounds;
                b.Inflate(6, 6);
                if (b.Contains(p)) return i;
            }
            return -1;
        }

        private void Preview_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PointF p = PictureBoxToImage(e.Location);
            int hit = HitTest(p);
            if (hit >= 0) _elementsList.SelectedIndex = hit;
            if (_selectedIndex < 0) return;

            var elem = _elements[_selectedIndex];
            _dragging = true;
            _dragStart = e.Location;
            _dragStartX = elem.X;
            _dragStartY = elem.Y;
            _preview.Cursor = Cursors.SizeAll;
            _preview.Focus();
        }

        private void Preview_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || _selectedIndex < 0) return;
            PointF s = PictureBoxToImage(_dragStart), n = PictureBoxToImage(e.Location);
            var elem = _elements[_selectedIndex];
            elem.X = _dragStartX + (n.X - s.X);
            elem.Y = _dragStartY + (n.Y - s.Y);
            RenderPreview();
        }

        private void Preview_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            _preview.Cursor = Cursors.Default;
        }

        private void Preview_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_selectedIndex < 0 || !(_elements[_selectedIndex] is ImageElement ie)) return;
            float step = 0.05f;
            ie.Scale += e.Delta > 0 ? step : -step;
            if (ie.Scale < 0.05f) ie.Scale = 0.05f;
            if (ie.Scale > 10f) ie.Scale = 10f;
            ShowPropsForSelected();
            _elementsList.Items[_selectedIndex] = ie;
            RenderPreview();
        }

        #endregion

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            int w = _background.Width, h = _background.Height;
            var final = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(final))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                if (_bgGradient)
                {
                    var mode = _bgGradVertical ? LinearGradientMode.Vertical : LinearGradientMode.Horizontal;
                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, w, h), _bgColor1, _bgColor2, mode))
                        g.FillRectangle(brush, 0, 0, w, h);
                }
                else
                {
                    g.DrawImage(_background, 0, 0, w, h);
                }

                foreach (var elem in _elements)
                    elem.Draw(g, false);
            }

            ResultImage = final;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
