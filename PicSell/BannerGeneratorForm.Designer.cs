namespace PicSell
{
    partial class BannerGeneratorForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.headerPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.productNameBox = new System.Windows.Forms.TextBox();
            this.addBgButton = new System.Windows.Forms.Button();
            this.mainSplit = new System.Windows.Forms.SplitContainer();
            this.sourceLabel = new System.Windows.Forms.Label();
            this.sourcePictureBox = new System.Windows.Forms.PictureBox();
            this.variantsLabel = new System.Windows.Forms.Label();
            this.flowPanel = new System.Windows.Forms.FlowLayoutPanel();

            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.mainSplit).BeginInit();
            this.mainSplit.Panel1.SuspendLayout();
            this.mainSplit.Panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.sourcePictureBox).BeginInit();
            this.SuspendLayout();

            // headerPanel
            this.headerPanel.Controls.Add(this.addBgButton);
            this.headerPanel.Controls.Add(this.productNameBox);
            this.headerPanel.Controls.Add(this.titleLabel);
            this.headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.headerPanel.Height = 52;
            this.headerPanel.Name = "headerPanel";

            // titleLabel
            this.titleLabel.AutoSize = false;
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Text = "🖼  Конструктор карточки";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.titleLabel.Width = 240;
            this.titleLabel.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);

            // productNameBox
            this.productNameBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.productNameBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.productNameBox.Name = "productNameBox";
            this.productNameBox.TabIndex = 0;

            // addBgButton
            this.addBgButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.addBgButton.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.addBgButton.Name = "addBgButton";
            this.addBgButton.Size = new System.Drawing.Size(150, 52);
            this.addBgButton.TabIndex = 2;
            this.addBgButton.Text = "📁  Добавить фоны";
            this.addBgButton.UseVisualStyleBackColor = true;
            this.addBgButton.Click += new System.EventHandler(this.addBgButton_Click);

            // mainSplit
            this.mainSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.mainSplit.Name = "mainSplit";
            this.mainSplit.SplitterDistance = 270;
            this.mainSplit.SplitterWidth = 4;

            // mainSplit.Panel1 (исходное фото)
            this.mainSplit.Panel1.Controls.Add(this.sourcePictureBox);
            this.mainSplit.Panel1.Controls.Add(this.sourceLabel);

            // mainSplit.Panel2 (варианты)
            this.mainSplit.Panel2.Controls.Add(this.flowPanel);
            this.mainSplit.Panel2.Controls.Add(this.variantsLabel);

            // sourceLabel
            this.sourceLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourceLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.sourceLabel.Height = 28;
            this.sourceLabel.Name = "sourceLabel";
            this.sourceLabel.Text = "  Исходное фото";
            this.sourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // sourcePictureBox
            this.sourcePictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourcePictureBox.Name = "sourcePictureBox";
            this.sourcePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // variantsLabel
            this.variantsLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.variantsLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.variantsLabel.Height = 28;
            this.variantsLabel.Name = "variantsLabel";
            this.variantsLabel.Text = "  Фоны  (нажми для применения)";
            this.variantsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // flowPanel
            this.flowPanel.AutoScroll = true;
            this.flowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPanel.Name = "flowPanel";
            this.flowPanel.Padding = new System.Windows.Forms.Padding(8);

            // BannerGeneratorForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 660);
            this.Controls.Add(this.mainSplit);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "BannerGeneratorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Генератор баннеров — PicSell";

            this.headerPanel.ResumeLayout(false);
            this.mainSplit.Panel1.ResumeLayout(false);
            this.mainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.mainSplit).EndInit();
            this.mainSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.sourcePictureBox).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.TextBox productNameBox;
        private System.Windows.Forms.Button addBgButton;
        private System.Windows.Forms.SplitContainer mainSplit;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.PictureBox sourcePictureBox;
        private System.Windows.Forms.Label variantsLabel;
        private System.Windows.Forms.FlowLayoutPanel flowPanel;
    }
}
