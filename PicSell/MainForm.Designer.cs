namespace PicSell
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.файлToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadPhotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePhotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manyPhButton = new System.Windows.Forms.Button();
            this.onePhButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.viewModeGroupBox = new System.Windows.Forms.GroupBox();
            this.HueTrackBar = new System.Windows.Forms.TrackBar();
            this.SaturationTrackBar = new System.Windows.Forms.TrackBar();
            this.LightnessTrackBar = new System.Windows.Forms.TrackBar();
            this.hslGroupBox = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CurrentImageList = new System.Windows.Forms.ImageList(this.components);
            this.CurrentListView = new PicSell.ToggleListView();
            this.listViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearWorkspaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CurrentPictureBox = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.drawColorDialog = new System.Windows.Forms.ColorDialog();
            this.drawSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.prevVersButton = new System.Windows.Forms.Button();
            this.nextVersButton = new System.Windows.Forms.Button();
            this.origVersButton = new System.Windows.Forms.Button();
            this.versionsHistoryPanel = new System.Windows.Forms.Panel();
            this.drawGroupBox = new System.Windows.Forms.GroupBox();
            this.removeBackButton = new System.Windows.Forms.Button();
            this.replaceBackButton = new System.Windows.Forms.Button();
            this.editImageButton = new System.Windows.Forms.Button();
            this.bannerGeneratorButton = new System.Windows.Forms.Button();
            this.pluginsPanel = new System.Windows.Forms.Panel();
            this.aiEmbedPanel = new System.Windows.Forms.Panel();
            this.changeLabel = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.menuStrip1.SuspendLayout();
            this.viewModeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HueTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SaturationTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightnessTrackBar)).BeginInit();
            this.hslGroupBox.SuspendLayout();
            this.listViewContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CurrentPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawSizeTrackBar)).BeginInit();
            this.versionsHistoryPanel.SuspendLayout();
            this.drawGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.файлToolStripMenuItem,
            this.PluginsToolStripMenuItem,
            this.statsToolStripMenuItem,
            this.aboutBoxToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1259, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            this.файлToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadPhotoToolStripMenuItem,
            this.savePhotoToolStripMenuItem});
            this.файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            this.файлToolStripMenuItem.Size = new System.Drawing.Size(59, 24);
            this.файлToolStripMenuItem.Text = "Файл";
            // 
            // loadPhotoToolStripMenuItem
            // 
            this.loadPhotoToolStripMenuItem.Name = "loadPhotoToolStripMenuItem";
            this.loadPhotoToolStripMenuItem.Size = new System.Drawing.Size(204, 26);
            this.loadPhotoToolStripMenuItem.Text = "Загрузить фото";
            this.loadPhotoToolStripMenuItem.Click += new System.EventHandler(this.loadPhotoToolStripMenuItem_Click);
            // 
            // savePhotoToolStripMenuItem
            // 
            this.savePhotoToolStripMenuItem.Name = "savePhotoToolStripMenuItem";
            this.savePhotoToolStripMenuItem.Size = new System.Drawing.Size(204, 26);
            this.savePhotoToolStripMenuItem.Text = "Сохранить фото";
            this.savePhotoToolStripMenuItem.Click += new System.EventHandler(this.savePhotoToolStripMenuItem_Click);
            // 
            // PluginsToolStripMenuItem
            // 
            this.PluginsToolStripMenuItem.Name = "PluginsToolStripMenuItem";
            this.PluginsToolStripMenuItem.Size = new System.Drawing.Size(85, 24);
            this.PluginsToolStripMenuItem.Text = "Плагины";
            this.PluginsToolStripMenuItem.Click += new System.EventHandler(this.PluginsToolStripMenuItem_Click);
            // 
            // statsToolStripMenuItem
            // 
            this.statsToolStripMenuItem.Name = "statsToolStripMenuItem";
            this.statsToolStripMenuItem.Size = new System.Drawing.Size(98, 24);
            this.statsToolStripMenuItem.Text = "Статистика";
            this.statsToolStripMenuItem.Click += new System.EventHandler(this.statsToolStripMenuItem_Click);
            // 
            // aboutBoxToolStripMenuItem
            // 
            this.aboutBoxToolStripMenuItem.Name = "aboutBoxToolStripMenuItem";
            this.aboutBoxToolStripMenuItem.Size = new System.Drawing.Size(118, 24);
            this.aboutBoxToolStripMenuItem.Text = "О программе";
            this.aboutBoxToolStripMenuItem.Click += new System.EventHandler(this.aboutBoxToolStripMenuItem_Click);
            // 
            // manyPhButton
            // 
            this.manyPhButton.Location = new System.Drawing.Point(23, 37);
            this.manyPhButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.manyPhButton.Name = "manyPhButton";
            this.manyPhButton.Size = new System.Drawing.Size(85, 63);
            this.manyPhButton.TabIndex = 1;
            this.manyPhButton.Text = "Много фото";
            this.manyPhButton.UseVisualStyleBackColor = true;
            this.manyPhButton.Click += new System.EventHandler(this.manyPhButton_Click);
            // 
            // onePhButton
            // 
            this.onePhButton.Location = new System.Drawing.Point(115, 37);
            this.onePhButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.onePhButton.Name = "onePhButton";
            this.onePhButton.Size = new System.Drawing.Size(83, 63);
            this.onePhButton.TabIndex = 1;
            this.onePhButton.Text = "Одно фото";
            this.onePhButton.UseVisualStyleBackColor = true;
            this.onePhButton.Click += new System.EventHandler(this.onePhButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Режим просмотра";
            // 
            // viewModeGroupBox
            // 
            this.viewModeGroupBox.Controls.Add(this.label1);
            this.viewModeGroupBox.Controls.Add(this.manyPhButton);
            this.viewModeGroupBox.Controls.Add(this.onePhButton);
            this.viewModeGroupBox.Location = new System.Drawing.Point(104, 4);
            this.viewModeGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.viewModeGroupBox.Name = "viewModeGroupBox";
            this.viewModeGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.viewModeGroupBox.Size = new System.Drawing.Size(237, 110);
            this.viewModeGroupBox.TabIndex = 3;
            this.viewModeGroupBox.TabStop = false;
            // 
            // HueTrackBar
            // 
            this.HueTrackBar.Location = new System.Drawing.Point(96, 21);
            this.HueTrackBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.HueTrackBar.Maximum = 360;
            this.HueTrackBar.Minimum = 1;
            this.HueTrackBar.Name = "HueTrackBar";
            this.HueTrackBar.Size = new System.Drawing.Size(237, 56);
            this.HueTrackBar.TabIndex = 3;
            this.HueTrackBar.Value = 1;
            this.HueTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HueTrackBar_MouseUp);
            // 
            // SaturationTrackBar
            // 
            this.SaturationTrackBar.Location = new System.Drawing.Point(96, 82);
            this.SaturationTrackBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SaturationTrackBar.Maximum = 100;
            this.SaturationTrackBar.Minimum = 1;
            this.SaturationTrackBar.Name = "SaturationTrackBar";
            this.SaturationTrackBar.Size = new System.Drawing.Size(237, 56);
            this.SaturationTrackBar.TabIndex = 2;
            this.SaturationTrackBar.Value = 1;
            this.SaturationTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SaturationTrackBar_MouseUp);
            // 
            // LightnessTrackBar
            // 
            this.LightnessTrackBar.Location = new System.Drawing.Point(96, 145);
            this.LightnessTrackBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.LightnessTrackBar.Maximum = 100;
            this.LightnessTrackBar.Minimum = 1;
            this.LightnessTrackBar.Name = "LightnessTrackBar";
            this.LightnessTrackBar.Size = new System.Drawing.Size(237, 56);
            this.LightnessTrackBar.TabIndex = 1;
            this.LightnessTrackBar.Value = 1;
            this.LightnessTrackBar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.LightnessTrackBar_MouseUp);
            // 
            // hslGroupBox
            // 
            this.hslGroupBox.Controls.Add(this.label4);
            this.hslGroupBox.Controls.Add(this.label3);
            this.hslGroupBox.Controls.Add(this.label2);
            this.hslGroupBox.Controls.Add(this.SaturationTrackBar);
            this.hslGroupBox.Controls.Add(this.LightnessTrackBar);
            this.hslGroupBox.Controls.Add(this.HueTrackBar);
            this.hslGroupBox.Enabled = false;
            this.hslGroupBox.Location = new System.Drawing.Point(15, 118);
            this.hslGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.hslGroupBox.Name = "hslGroupBox";
            this.hslGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.hslGroupBox.Size = new System.Drawing.Size(388, 203);
            this.hslGroupBox.TabIndex = 5;
            this.hslGroupBox.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 145);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "L";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(16, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "S";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "H";
            // 
            // CurrentImageList
            // 
            this.CurrentImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.CurrentImageList.ImageSize = new System.Drawing.Size(160, 160);
            this.CurrentImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // CurrentListView
            // 
            this.CurrentListView.AllowDrop = true;
            this.CurrentListView.ContextMenuStrip = this.listViewContextMenu;
            this.CurrentListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentListView.HideSelection = false;
            this.CurrentListView.LargeImageList = this.CurrentImageList;
            this.CurrentListView.Location = new System.Drawing.Point(0, 0);
            this.CurrentListView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CurrentListView.Name = "CurrentListView";
            this.CurrentListView.Size = new System.Drawing.Size(927, 690);
            this.CurrentListView.TabIndex = 6;
            this.CurrentListView.UseCompatibleStateImageBehavior = false;
            this.CurrentListView.SelectedIndexChanged += new System.EventHandler(this.CurrentListView_SelectedIndexChanged);
            this.CurrentListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.CurrentListView_DragDrop);
            this.CurrentListView.DragEnter += new System.Windows.Forms.DragEventHandler(this.CurrentListView_DragEnter);
            this.CurrentListView.DoubleClick += new System.EventHandler(this.onePhButton_Click);
            // 
            // listViewContextMenu
            // 
            this.listViewContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.listViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearWorkspaceToolStripMenuItem});
            this.listViewContextMenu.Name = "listViewContextMenu";
            this.listViewContextMenu.Size = new System.Drawing.Size(268, 28);
            // 
            // clearWorkspaceToolStripMenuItem
            // 
            this.clearWorkspaceToolStripMenuItem.Name = "clearWorkspaceToolStripMenuItem";
            this.clearWorkspaceToolStripMenuItem.Size = new System.Drawing.Size(267, 24);
            this.clearWorkspaceToolStripMenuItem.Text = "Очистить рабочую область";
            this.clearWorkspaceToolStripMenuItem.Click += new System.EventHandler(this.clearWorkspaceToolStripMenuItem_Click);
            // 
            // CurrentPictureBox
            // 
            this.CurrentPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CurrentPictureBox.Location = new System.Drawing.Point(0, 0);
            this.CurrentPictureBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CurrentPictureBox.Name = "CurrentPictureBox";
            this.CurrentPictureBox.Size = new System.Drawing.Size(927, 649);
            this.CurrentPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.CurrentPictureBox.TabIndex = 7;
            this.CurrentPictureBox.TabStop = false;
            this.CurrentPictureBox.Visible = false;
            this.CurrentPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CurrentPictureBox_MouseDown);
            this.CurrentPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CurrentPictureBox_MouseMove);
            this.CurrentPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CurrentPictureBox_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Location = new System.Drawing.Point(20, 46);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(341, 32);
            this.panel1.TabIndex = 8;
            this.panel1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // drawSizeTrackBar
            // 
            this.drawSizeTrackBar.Location = new System.Drawing.Point(20, 98);
            this.drawSizeTrackBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.drawSizeTrackBar.Name = "drawSizeTrackBar";
            this.drawSizeTrackBar.Size = new System.Drawing.Size(341, 56);
            this.drawSizeTrackBar.TabIndex = 15;
            // 
            // prevVersButton
            // 
            this.prevVersButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.prevVersButton.Location = new System.Drawing.Point(0, 0);
            this.prevVersButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.prevVersButton.Name = "prevVersButton";
            this.prevVersButton.Size = new System.Drawing.Size(75, 41);
            this.prevVersButton.TabIndex = 16;
            this.prevVersButton.Text = "<-";
            this.prevVersButton.UseVisualStyleBackColor = true;
            this.prevVersButton.Click += new System.EventHandler(this.prevVersButton_Click);
            // 
            // nextVersButton
            // 
            this.nextVersButton.Dock = System.Windows.Forms.DockStyle.Right;
            this.nextVersButton.Location = new System.Drawing.Point(852, 0);
            this.nextVersButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nextVersButton.Name = "nextVersButton";
            this.nextVersButton.Size = new System.Drawing.Size(75, 41);
            this.nextVersButton.TabIndex = 16;
            this.nextVersButton.Text = "->";
            this.nextVersButton.UseVisualStyleBackColor = true;
            this.nextVersButton.Click += new System.EventHandler(this.nextVersButton_Click);
            // 
            // origVersButton
            // 
            this.origVersButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.origVersButton.Location = new System.Drawing.Point(81, 2);
            this.origVersButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.origVersButton.Name = "origVersButton";
            this.origVersButton.Size = new System.Drawing.Size(755, 34);
            this.origVersButton.TabIndex = 17;
            this.origVersButton.Text = "original";
            this.origVersButton.UseVisualStyleBackColor = true;
            this.origVersButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.origVersButton_MouseDown);
            this.origVersButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.origVersButton_MouseUp);
            // 
            // versionsHistoryPanel
            // 
            this.versionsHistoryPanel.Controls.Add(this.nextVersButton);
            this.versionsHistoryPanel.Controls.Add(this.prevVersButton);
            this.versionsHistoryPanel.Controls.Add(this.origVersButton);
            this.versionsHistoryPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.versionsHistoryPanel.Location = new System.Drawing.Point(0, 649);
            this.versionsHistoryPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.versionsHistoryPanel.Name = "versionsHistoryPanel";
            this.versionsHistoryPanel.Size = new System.Drawing.Size(927, 41);
            this.versionsHistoryPanel.TabIndex = 18;
            this.versionsHistoryPanel.Visible = false;
            // 
            // drawGroupBox
            // 
            this.drawGroupBox.Controls.Add(this.drawSizeTrackBar);
            this.drawGroupBox.Controls.Add(this.panel1);
            this.drawGroupBox.Location = new System.Drawing.Point(15, 326);
            this.drawGroupBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.drawGroupBox.Name = "drawGroupBox";
            this.drawGroupBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.drawGroupBox.Size = new System.Drawing.Size(388, 166);
            this.drawGroupBox.TabIndex = 19;
            this.drawGroupBox.TabStop = false;
            this.drawGroupBox.Text = "Рисование";
            //
            // removeBackButton
            //
            this.removeBackButton.Location = new System.Drawing.Point(15, 498);
            this.removeBackButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.removeBackButton.Name = "removeBackButton";
            this.removeBackButton.Size = new System.Drawing.Size(388, 45);
            this.removeBackButton.TabIndex = 23;
            this.removeBackButton.Text = "Удалить фон";
            this.removeBackButton.UseVisualStyleBackColor = true;
            this.removeBackButton.Click += new System.EventHandler(this.removeBackButton_Click);
            //
            // replaceBackButton
            //
            this.replaceBackButton.Location = new System.Drawing.Point(15, 549);
            this.replaceBackButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.replaceBackButton.Name = "replaceBackButton";
            this.replaceBackButton.Size = new System.Drawing.Size(388, 45);
            this.replaceBackButton.TabIndex = 24;
            this.replaceBackButton.Text = "Заменить фон";
            this.replaceBackButton.UseVisualStyleBackColor = true;
            this.replaceBackButton.Click += new System.EventHandler(this.replaceBackButton_Click);
            //
            // editImageButton
            //
            this.editImageButton.Location = new System.Drawing.Point(15, 600);
            this.editImageButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.editImageButton.Name = "editImageButton";
            this.editImageButton.Size = new System.Drawing.Size(388, 45);
            this.editImageButton.TabIndex = 25;
            this.editImageButton.Text = "Редактор изображения";
            this.editImageButton.UseVisualStyleBackColor = true;
            this.editImageButton.Click += new System.EventHandler(this.editImageButton_Click);
            //
            // bannerGeneratorButton
            //
            this.bannerGeneratorButton.Location = new System.Drawing.Point(15, 651);
            this.bannerGeneratorButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bannerGeneratorButton.Name = "bannerGeneratorButton";
            this.bannerGeneratorButton.Size = new System.Drawing.Size(388, 45);
            this.bannerGeneratorButton.TabIndex = 26;
            this.bannerGeneratorButton.Text = "🎨  Варианты обработки";
            this.bannerGeneratorButton.UseVisualStyleBackColor = true;
            this.bannerGeneratorButton.Click += new System.EventHandler(this.bannerGeneratorButton_Click);
            //
            // pluginsPanel
            //
            this.pluginsPanel.Location = new System.Drawing.Point(15, 700);
            this.pluginsPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pluginsPanel.Name = "pluginsPanel";
            this.pluginsPanel.Size = new System.Drawing.Size(388, 362);
            this.pluginsPanel.TabIndex = 20;
            // 
            // changeLabel
            // 
            this.changeLabel.AutoSize = true;
            this.changeLabel.Location = new System.Drawing.Point(968, 0);
            this.changeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.changeLabel.Name = "changeLabel";
            this.changeLabel.Size = new System.Drawing.Size(245, 16);
            this.changeLabel.TabIndex = 21;
            this.changeLabel.Text = "отмена/возврат название действия";
            this.changeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.changeLabel.Visible = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CurrentPictureBox);
            this.splitContainer1.Panel1.Controls.Add(this.versionsHistoryPanel);
            this.splitContainer1.Panel1.Controls.Add(this.CurrentListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.drawGroupBox);
            this.splitContainer1.Panel2.Controls.Add(this.viewModeGroupBox);
            this.splitContainer1.Panel2.Controls.Add(this.removeBackButton);
            this.splitContainer1.Panel2.Controls.Add(this.replaceBackButton);
            this.splitContainer1.Panel2.Controls.Add(this.editImageButton);
            this.splitContainer1.Panel2.Controls.Add(this.bannerGeneratorButton);
            this.splitContainer1.Panel2.Controls.Add(this.pluginsPanel);
            this.splitContainer1.Panel2.Controls.Add(this.hslGroupBox);
            this.splitContainer1.Size = new System.Drawing.Size(1259, 690);
            this.splitContainer1.SplitterDistance = 927;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 22;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1259, 718);
            // aiEmbedPanel
            this.aiEmbedPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.aiEmbedPanel.Height = 300;
            this.aiEmbedPanel.Name = "aiEmbedPanel";
            this.aiEmbedPanel.TabIndex = 27;

            this.Controls.Add(this.changeLabel);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.aiEmbedPanel);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "PicSell";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.viewModeGroupBox.ResumeLayout(false);
            this.viewModeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HueTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SaturationTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LightnessTrackBar)).EndInit();
            this.hslGroupBox.ResumeLayout(false);
            this.hslGroupBox.PerformLayout();
            this.listViewContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.CurrentPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.drawSizeTrackBar)).EndInit();
            this.versionsHistoryPanel.ResumeLayout(false);
            this.drawGroupBox.ResumeLayout(false);
            this.drawGroupBox.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem файлToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem PluginsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutBoxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadPhotoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePhotoToolStripMenuItem;
        private System.Windows.Forms.Button manyPhButton;
        private System.Windows.Forms.Button onePhButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox viewModeGroupBox;
        private System.Windows.Forms.TrackBar HueTrackBar;
        private System.Windows.Forms.TrackBar SaturationTrackBar;
        private System.Windows.Forms.TrackBar LightnessTrackBar;
        private System.Windows.Forms.GroupBox hslGroupBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ImageList CurrentImageList;


        public PicSell.ToggleListView CurrentListView;

        //хз насколько так можно но изменил на public уровень защиты 
        public System.Windows.Forms.PictureBox CurrentPictureBox;
        
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ColorDialog drawColorDialog;
        private System.Windows.Forms.TrackBar drawSizeTrackBar;
        private System.Windows.Forms.Button prevVersButton;
        private System.Windows.Forms.Button nextVersButton;
        private System.Windows.Forms.Button origVersButton;
        private System.Windows.Forms.Panel versionsHistoryPanel;
        private System.Windows.Forms.GroupBox drawGroupBox;
        private System.Windows.Forms.ContextMenuStrip listViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem clearWorkspaceToolStripMenuItem;

        private System.Windows.Forms.Button removeBackButton;
        private System.Windows.Forms.Button replaceBackButton;
        private System.Windows.Forms.Button editImageButton;
        private System.Windows.Forms.Button bannerGeneratorButton;
        private System.Windows.Forms.Panel aiEmbedPanel;

        //вручную изменён на public
        public System.Windows.Forms.Panel pluginsPanel;
        private System.Windows.Forms.Label changeLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem statsToolStripMenuItem;
    }
}

