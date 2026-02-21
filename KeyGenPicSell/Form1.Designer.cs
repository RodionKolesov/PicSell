namespace KeyGenPicSell
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxVersions = new System.Windows.Forms.ComboBox();
            this.chkStat = new System.Windows.Forms.CheckBox();
            this.chkPlugins = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.limitationsGroupBox = new System.Windows.Forms.GroupBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnRefreshUSB = new System.Windows.Forms.Button();
            this.btnSaveKey = new System.Windows.Forms.Button();
            this.labelSn = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxUSB = new System.Windows.Forms.ComboBox();
            this.btnGenerateKey = new System.Windows.Forms.Button();
            this.chkDraw = new System.Windows.Forms.CheckBox();
            this.limitationsGroupBox.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Имя пользователя:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Версия программы:";
            // 
            // comboBoxVersions
            // 
            this.comboBoxVersions.FormattingEnabled = true;
            this.comboBoxVersions.Location = new System.Drawing.Point(154, 53);
            this.comboBoxVersions.Name = "comboBoxVersions";
            this.comboBoxVersions.Size = new System.Drawing.Size(121, 24);
            this.comboBoxVersions.TabIndex = 2;
            this.comboBoxVersions.SelectedIndexChanged += new System.EventHandler(this.comboBoxVersions_SelectedIndexChanged);
            // 
            // chkStat
            // 
            this.chkStat.AutoSize = true;
            this.chkStat.Location = new System.Drawing.Point(15, 32);
            this.chkStat.Name = "chkStat";
            this.chkStat.Size = new System.Drawing.Size(213, 20);
            this.chkStat.TabIndex = 3;
            this.chkStat.Text = "Выключить окно статистики";
            this.chkStat.UseVisualStyleBackColor = true;
            // 
            // chkPlugins
            // 
            this.chkPlugins.AutoSize = true;
            this.chkPlugins.Location = new System.Drawing.Point(15, 55);
            this.chkPlugins.Name = "chkPlugins";
            this.chkPlugins.Size = new System.Drawing.Size(339, 20);
            this.chkPlugins.TabIndex = 3;
            this.chkPlugins.Text = "Выключить возможность добавления плагинов";
            this.chkPlugins.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 221);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(214, 16);
            this.label4.TabIndex = 5;
            this.label4.Text = "Окончание действия лицензии:";
            // 
            // limitationsGroupBox
            // 
            this.limitationsGroupBox.Controls.Add(this.chkStat);
            this.limitationsGroupBox.Controls.Add(this.chkDraw);
            this.limitationsGroupBox.Controls.Add(this.chkPlugins);
            this.limitationsGroupBox.Location = new System.Drawing.Point(19, 94);
            this.limitationsGroupBox.Name = "limitationsGroupBox";
            this.limitationsGroupBox.Size = new System.Drawing.Size(769, 110);
            this.limitationsGroupBox.TabIndex = 7;
            this.limitationsGroupBox.TabStop = false;
            this.limitationsGroupBox.Text = "Ограничения:";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Location = new System.Drawing.Point(236, 216);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(552, 22);
            this.dateTimePicker1.TabIndex = 7;
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(154, 20);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(121, 22);
            this.textBoxUsername.TabIndex = 8;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnRefreshUSB);
            this.groupBox2.Controls.Add(this.btnSaveKey);
            this.groupBox2.Controls.Add(this.labelSn);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.comboBoxUSB);
            this.groupBox2.Location = new System.Drawing.Point(19, 291);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(769, 100);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "USB";
            // 
            // btnRefreshUSB
            // 
            this.btnRefreshUSB.Location = new System.Drawing.Point(232, 25);
            this.btnRefreshUSB.Name = "btnRefreshUSB";
            this.btnRefreshUSB.Size = new System.Drawing.Size(87, 24);
            this.btnRefreshUSB.TabIndex = 6;
            this.btnRefreshUSB.Text = "обновить";
            this.btnRefreshUSB.UseVisualStyleBackColor = true;
            this.btnRefreshUSB.Click += new System.EventHandler(this.btnRefreshUsb_Click);
            // 
            // btnSaveKey
            // 
            this.btnSaveKey.Location = new System.Drawing.Point(554, 21);
            this.btnSaveKey.Name = "btnSaveKey";
            this.btnSaveKey.Size = new System.Drawing.Size(209, 72);
            this.btnSaveKey.TabIndex = 5;
            this.btnSaveKey.Text = "Сохранение ключа на выбранное USB устройство";
            this.btnSaveKey.UseVisualStyleBackColor = true;
            this.btnSaveKey.Click += new System.EventHandler(this.btnSaveKey_Click);
            // 
            // labelSn
            // 
            this.labelSn.AutoSize = true;
            this.labelSn.Location = new System.Drawing.Point(147, 66);
            this.labelSn.Name = "labelSn";
            this.labelSn.Size = new System.Drawing.Size(21, 16);
            this.labelSn.TabIndex = 4;
            this.labelSn.Text = "sn";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "Серийный номер:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Устройство:";
            // 
            // comboBoxUSB
            // 
            this.comboBoxUSB.FormattingEnabled = true;
            this.comboBoxUSB.Location = new System.Drawing.Point(105, 25);
            this.comboBoxUSB.Name = "comboBoxUSB";
            this.comboBoxUSB.Size = new System.Drawing.Size(121, 24);
            this.comboBoxUSB.TabIndex = 2;
            this.comboBoxUSB.SelectedIndexChanged += new System.EventHandler(this.comboBoxUSB_SelectedIndexChanged);
            // 
            // btnGenerateKey
            // 
            this.btnGenerateKey.Location = new System.Drawing.Point(19, 251);
            this.btnGenerateKey.Name = "btnGenerateKey";
            this.btnGenerateKey.Size = new System.Drawing.Size(769, 23);
            this.btnGenerateKey.TabIndex = 10;
            this.btnGenerateKey.Text = "Генерация ключа";
            this.btnGenerateKey.UseVisualStyleBackColor = true;
            this.btnGenerateKey.Click += new System.EventHandler(this.btnGenerateKey_Click);
            // 
            // chkDraw
            // 
            this.chkDraw.AutoSize = true;
            this.chkDraw.Location = new System.Drawing.Point(15, 81);
            this.chkDraw.Name = "chkDraw";
            this.chkDraw.Size = new System.Drawing.Size(256, 20);
            this.chkDraw.TabIndex = 3;
            this.chkDraw.Text = "Выключить возможность рисовать";
            this.chkDraw.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 404);
            this.Controls.Add(this.dateTimePicker1);
            this.Controls.Add(this.btnGenerateKey);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxUsername);
            this.Controls.Add(this.limitationsGroupBox);
            this.Controls.Add(this.comboBoxVersions);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.limitationsGroupBox.ResumeLayout(false);
            this.limitationsGroupBox.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxVersions;
        private System.Windows.Forms.CheckBox chkStat;
        private System.Windows.Forms.CheckBox chkPlugins;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox limitationsGroupBox;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxUSB;
        private System.Windows.Forms.Button btnRefreshUSB;
        private System.Windows.Forms.Button btnSaveKey;
        private System.Windows.Forms.Label labelSn;
        private System.Windows.Forms.Button btnGenerateKey;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.CheckBox chkDraw;
    }
}

