namespace PicSell
{
    partial class AIAssistantForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.chatRichTextBox = new System.Windows.Forms.RichTextBox();
            this.inputPanel = new System.Windows.Forms.Panel();
            this.separatorPanel = new System.Windows.Forms.Panel();
            this.inputTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.apiKeyButton = new System.Windows.Forms.Button();
            this.providerButton = new System.Windows.Forms.Button();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.titleLabel = new System.Windows.Forms.Label();
            // нужны как поля, но не добавляются в форму
            this.headerPanel = new System.Windows.Forms.Panel();
            this.photoCountLabel = new System.Windows.Forms.Label();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.rightPanel = new System.Windows.Forms.Panel();

            this.inputPanel.SuspendLayout();
            this.inputTableLayout.SuspendLayout();
            this.SuspendLayout();

            // chatRichTextBox
            this.chatRichTextBox.BackColor = System.Drawing.Color.FromArgb(22, 22, 22);
            this.chatRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chatRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chatRichTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.chatRichTextBox.Name = "chatRichTextBox";
            this.chatRichTextBox.ReadOnly = true;
            this.chatRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.chatRichTextBox.TabIndex = 3;
            this.chatRichTextBox.Text = "";
            this.chatRichTextBox.Padding = new System.Windows.Forms.Padding(10, 8, 10, 4);

            // inputPanel — нижняя панель
            this.inputPanel.Controls.Add(this.inputTableLayout);
            this.inputPanel.Controls.Add(this.titleLabel);
            this.inputPanel.Controls.Add(this.separatorPanel);
            this.inputPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inputPanel.Height = 74;
            this.inputPanel.Name = "inputPanel";
            this.inputPanel.Padding = new System.Windows.Forms.Padding(10, 6, 10, 6);

            // separatorPanel — тонкая линия сверху
            this.separatorPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.separatorPanel.Height = 1;
            this.separatorPanel.Name = "separatorPanel";
            this.separatorPanel.BackColor = System.Drawing.Color.FromArgb(60, 60, 60);

            // titleLabel — строка со статусом снизу
            this.titleLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.titleLabel.Height = 16;
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Text = "Groq  ·  0 фото";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // inputTableLayout — 1 строка, 4 столбца
            this.inputTableLayout.ColumnCount = 4;
            this.inputTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.inputTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.inputTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.inputTableLayout.Controls.Add(this.apiKeyButton, 0, 0);
            this.inputTableLayout.Controls.Add(this.providerButton, 1, 0);
            this.inputTableLayout.Controls.Add(this.inputTextBox, 2, 0);
            this.inputTableLayout.Controls.Add(this.sendButton, 3, 0);
            this.inputTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputTableLayout.Name = "inputTableLayout";
            this.inputTableLayout.RowCount = 1;
            this.inputTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));

            // apiKeyButton — иконка шестерёнки слева
            this.apiKeyButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.apiKeyButton.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.apiKeyButton.Name = "apiKeyButton";
            this.apiKeyButton.TabIndex = 1;
            this.apiKeyButton.Text = "⚙";
            this.apiKeyButton.UseVisualStyleBackColor = true;
            this.apiKeyButton.Click += new System.EventHandler(this.apiKeyButton_Click);

            // providerButton — выбор нейросети
            this.providerButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.providerButton.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.providerButton.Name = "providerButton";
            this.providerButton.TabIndex = 3;
            this.providerButton.Text = "Groq ▾";
            this.providerButton.UseVisualStyleBackColor = true;
            this.providerButton.Click += new System.EventHandler(this.providerButton_Click);

            // inputTextBox — поле ввода
            this.inputTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.inputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.inputTextBox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.TabIndex = 0;

            // sendButton — кнопка отправки справа
            this.sendButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sendButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.sendButton.Name = "sendButton";
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Отправить";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);

            // AIAssistantForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(700, 300);
            this.Controls.Add(this.chatRichTextBox);
            this.Controls.Add(this.inputPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "AIAssistantForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AI Ассистент — PicSell";

            this.inputTableLayout.ResumeLayout(false);
            this.inputTableLayout.PerformLayout();
            this.inputPanel.ResumeLayout(false);
            this.inputPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        // основные контролы
        private System.Windows.Forms.RichTextBox chatRichTextBox;
        private System.Windows.Forms.Panel inputPanel;
        private System.Windows.Forms.Panel separatorPanel;
        private System.Windows.Forms.TableLayoutPanel inputTableLayout;
        private System.Windows.Forms.Button apiKeyButton;
        private System.Windows.Forms.Button providerButton;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.Label titleLabel;

        // поля, которые нужны коду в .cs, но не добавлены в форму
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label photoCountLabel;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel rightPanel;
    }
}
