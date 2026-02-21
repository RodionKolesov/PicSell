using System;
using System.Drawing;
using System.Windows.Forms;

namespace PicSell
{
    public static class DarkTheme
    {
        // Photoshop-inspired color palette
        public static readonly Color MainBg = Color.FromArgb(42, 42, 42);
        public static readonly Color DarkBg = Color.FromArgb(26, 26, 26);
        public static readonly Color PanelBg = Color.FromArgb(51, 51, 51);
        public static readonly Color MenuBg = Color.FromArgb(48, 48, 48);
        public static readonly Color Text = Color.FromArgb(210, 210, 210);
        public static readonly Color DimText = Color.FromArgb(150, 150, 150);
        public static readonly Color Accent = Color.FromArgb(38, 128, 235);
        public static readonly Color BtnBg = Color.FromArgb(62, 62, 62);
        public static readonly Color BtnHover = Color.FromArgb(78, 78, 78);
        public static readonly Color Border = Color.FromArgb(70, 70, 70);
        public static readonly Color Selection = Color.FromArgb(38, 79, 120);
        public static readonly Color InputBg = Color.FromArgb(35, 35, 35);
        public static readonly Color VersionBar = Color.FromArgb(50, 50, 50);

        public static void Apply(Form form)
        {
            form.BackColor = MainBg;
            form.ForeColor = Text;
            ApplyRecursive(form);
        }

        private static void ApplyRecursive(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                StyleControl(c);

                if (c.ContextMenuStrip != null)
                    StyleContextMenu(c.ContextMenuStrip);

                if (c.HasChildren)
                    ApplyRecursive(c);
            }
        }

        private static void StyleControl(Control c)
        {
            c.ForeColor = Text;

            if (c is MenuStrip)
            {
                var ms = (MenuStrip)c;
                ms.BackColor = MenuBg;
                ms.Renderer = new DarkMenuRenderer();
            }
            else if (c is Button)
            {
                StyleButton((Button)c);
            }
            else if (c is GroupBox)
            {
                c.BackColor = MainBg;
            }
            else if (c is SplitContainer)
            {
                var sc = (SplitContainer)c;
                sc.BackColor = Color.FromArgb(22, 22, 22);
            }
            else if (c is ListView)
            {
                var lv = (ListView)c;
                lv.BackColor = DarkBg;
                lv.ForeColor = Text;
            }
            else if (c is PictureBox)
            {
                c.BackColor = DarkBg;
            }
            else if (c is TextBox)
            {
                var tb = (TextBox)c;
                tb.BackColor = InputBg;
                tb.ForeColor = Text;
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is DataGridView)
            {
                StyleDataGrid((DataGridView)c);
            }
            else if (c is TrackBar)
            {
                c.BackColor = MainBg;
            }
            else if (c is Label)
            {
                c.BackColor = Color.Transparent;
            }
            else if (c is SplitterPanel)
            {
                // Handled by SplitContainer parent or in form code
            }
            else if (c is Panel)
            {
                var pnl = (Panel)c;
                // Skip color preview panel (drawing color)
                if (pnl.Name != "panel1")
                    pnl.BackColor = MainBg;
            }
            else if (c is TableLayoutPanel)
            {
                c.BackColor = MainBg;
            }
            else
            {
                c.BackColor = MainBg;
            }
        }

        public static void StyleButton(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = BtnBg;
            btn.ForeColor = Text;
            btn.FlatAppearance.BorderColor = Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = BtnHover;
            btn.FlatAppearance.MouseDownBackColor = Accent;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        }

        public static void StyleContextMenu(ContextMenuStrip cms)
        {
            cms.BackColor = MenuBg;
            cms.ForeColor = Text;
            cms.Renderer = new DarkMenuRenderer();
        }

        public static void StyleDataGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = DarkBg;
            dgv.GridColor = Border;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
            dgv.DefaultCellStyle.ForeColor = Text;
            dgv.DefaultCellStyle.SelectionBackColor = Selection;
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);

            dgv.ColumnHeadersDefaultCellStyle.BackColor = MenuBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = MenuBg;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            dgv.RowHeadersDefaultCellStyle.BackColor = MenuBg;
            dgv.RowHeadersDefaultCellStyle.ForeColor = Text;
            dgv.RowHeadersDefaultCellStyle.SelectionBackColor = Selection;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgv.AlternatingRowsDefaultCellStyle.ForeColor = Text;
        }

        public static void StyleLabel(Label label)
        {
            label.ForeColor = Text;
            label.BackColor = Color.Transparent;
            label.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        }
    }

    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable()) { }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = DarkTheme.Text;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rect = new Rectangle(Point.Empty, e.Item.Size);
            Color bgColor = (e.Item.Selected || e.Item.Pressed)
                ? DarkTheme.Selection
                : DarkTheme.MenuBg;

            using (var brush = new SolidBrush(bgColor))
                e.Graphics.FillRectangle(brush, rect);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(DarkTheme.MenuBg))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var pen = new Pen(DarkTheme.Border))
                e.Graphics.DrawLine(pen, 0, e.AffectedBounds.Height - 1, e.AffectedBounds.Width, e.AffectedBounds.Height - 1);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using (var pen = new Pen(DarkTheme.Border))
                e.Graphics.DrawLine(pen, 28, y, e.Item.Width - 4, y);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = DarkTheme.Text;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(38, 38, 38)))
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuBorder { get { return DarkTheme.Border; } }
        public override Color MenuItemBorder { get { return DarkTheme.Selection; } }
        public override Color MenuItemSelected { get { return DarkTheme.Selection; } }
        public override Color MenuItemSelectedGradientBegin { get { return DarkTheme.Selection; } }
        public override Color MenuItemSelectedGradientEnd { get { return DarkTheme.Selection; } }
        public override Color MenuItemPressedGradientBegin { get { return DarkTheme.Accent; } }
        public override Color MenuItemPressedGradientEnd { get { return DarkTheme.Accent; } }
        public override Color MenuStripGradientBegin { get { return DarkTheme.MenuBg; } }
        public override Color MenuStripGradientEnd { get { return DarkTheme.MenuBg; } }
        public override Color ToolStripDropDownBackground { get { return DarkTheme.MenuBg; } }
        public override Color ImageMarginGradientBegin { get { return Color.FromArgb(38, 38, 38); } }
        public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(38, 38, 38); } }
        public override Color ImageMarginGradientEnd { get { return Color.FromArgb(38, 38, 38); } }
        public override Color SeparatorDark { get { return DarkTheme.Border; } }
        public override Color SeparatorLight { get { return DarkTheme.Border; } }
    }
}
