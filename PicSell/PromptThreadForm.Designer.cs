namespace PicSell
{
    partial class PromptThreadForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.threadsNumTextBox = new System.Windows.Forms.TextBox();
            this.OkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(203, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Введите количество потоков:";
            // 
            // threadsNumTextBox
            // 
            this.threadsNumTextBox.Location = new System.Drawing.Point(13, 40);
            this.threadsNumTextBox.Name = "threadsNumTextBox";
            this.threadsNumTextBox.Size = new System.Drawing.Size(100, 22);
            this.threadsNumTextBox.TabIndex = 1;
            // 
            // OkButton
            // 
            this.OkButton.Location = new System.Drawing.Point(12, 77);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(235, 23);
            this.OkButton.TabIndex = 2;
            this.OkButton.Text = "Ок";
            this.OkButton.UseVisualStyleBackColor = true;
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // PromptThreadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 112);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.threadsNumTextBox);
            this.Controls.Add(this.label1);
            this.Name = "PromptThreadForm";
            this.Text = "PromptThreadForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox threadsNumTextBox;
        private System.Windows.Forms.Button OkButton;
    }
}