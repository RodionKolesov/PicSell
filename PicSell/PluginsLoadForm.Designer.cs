namespace PicSell
{
    partial class PluginsLoadForm
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
            this.loadPluginButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // loadPluginButton
            // 
            this.loadPluginButton.Location = new System.Drawing.Point(358, 12);
            this.loadPluginButton.Name = "loadPluginButton";
            this.loadPluginButton.Size = new System.Drawing.Size(159, 23);
            this.loadPluginButton.TabIndex = 0;
            this.loadPluginButton.Text = "Загрузить плагин";
            this.loadPluginButton.UseVisualStyleBackColor = true;
            this.loadPluginButton.Click += new System.EventHandler(this.loadPluginButton_Click);
            // 
            // PluginsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(935, 450);
            this.Controls.Add(this.loadPluginButton);
            this.Name = "PluginsForm";
            this.Text = "PluginsForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button loadPluginButton;
    }
}