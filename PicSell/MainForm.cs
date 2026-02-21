using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Data.SQLite;
using System.IO;
using CourseWork;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using IniParser.Model;
using IniParser;
using PluginBase;
using System.Reflection;


namespace PicSell
{
    public partial class MainForm : Form
    {
        public class PluginManager
        {
            private readonly string configPath = "config.ini";
            private readonly FileIniDataParser parser = new FileIniDataParser();

            public void LoadPluginsFromIni()
            {
                if (!File.Exists(configPath))
                    return;

                IniData data = parser.ReadFile(configPath);

                if (data.Sections.ContainsSection("Plugins"))
                {
                    PluginsLoadForm.plugins.Clear();
                    PluginsLoadForm.pluginPaths.Clear();
                    PluginsLoadForm.guids.Clear();

                    foreach (var key in data["Plugins"])
                    {
                        string dllPath = key.Value;

                        if (!File.Exists(dllPath))
                            continue;

                        try
                        {
                            Assembly pluginAssembly = Assembly.LoadFrom(dllPath);
                            var pluginTypes = pluginAssembly.GetTypes()
                                .Where(t => typeof(IImageEditing).IsAssignableFrom(t)
                                         && !t.IsInterface && !t.IsAbstract);

                            foreach (var type in pluginTypes)
                            {
                                if (Activator.CreateInstance(type) is IImageEditing plugin)
                                {
                                    string guid = plugin.GetGUID();

                                    if (!PluginsLoadForm.guids.Contains(guid))
                                    {
                                        PluginsLoadForm.plugins.Add(plugin);
                                        PluginsLoadForm.pluginPaths.Add(dllPath);
                                        PluginsLoadForm.guids.Add(guid);

                                        //костыль :)
                                        
                                        PluginsLoadForm temp = new PluginsLoadForm();
                                        temp.AddPluginImgEditingButton(plugin);
                                        temp.Close();
                                        
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка загрузки плагина {dllPath}:\n{ex.Message}", "Ошибка");
                        }
                    }
                }
            }


            public void WritePluginsToIni()
            {
                IniData data = File.Exists(configPath)
                    ? parser.ReadFile(configPath)
                    : new IniData();

                data.Sections.RemoveSection("Plugins");
                data.Sections.AddSection("Plugins");

                for (int i = 0; i < PluginsLoadForm.pluginPaths.Count; i++)
                {
                    string key = $"plugin{i + 1}";
                    string path = PluginsLoadForm.pluginPaths[i];
                    data["Plugins"][key] = path;
                }

                parser.WriteFile(configPath, data);
            }

        }



        public static MainForm Instance { get; private set; }

        public MainForm()
        {
            InitializeComponent();

            // Dark Photoshop-style theme
            DarkTheme.Apply(this);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.WindowState = FormWindowState.Maximized;

            // Workspace (left) = very dark, Sidebar (right) = panel color
            splitContainer1.Panel1.BackColor = DarkTheme.DarkBg;
            splitContainer1.Panel2.BackColor = DarkTheme.PanelBg;

            // Version history bar
            versionsHistoryPanel.BackColor = DarkTheme.VersionBar;
            prevVersButton.Text = "\u25C0";
            nextVersButton.Text = "\u25B6";
            origVersButton.Text = "\u25CE  \u041E\u0440\u0438\u0433\u0438\u043D\u0430\u043B";
            origVersButton.BackColor = DarkTheme.VersionBar;

            // Style changeLabel
            changeLabel.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            changeLabel.ForeColor = DarkTheme.DimText;

            LoadPhotosFromDB();
            updateModeGUI();

            LicenseManager.Initialize();
            CheckLicenseAndUpdateUI();

            Instance = this;
            onePhButton.Enabled = false; //потому что ни одно изображение не выделено, не с чем работать

            PluginManager pm = new PluginManager();
            pm.LoadPluginsFromIni();

            // Style the background buttons
            DarkTheme.StyleButton(removeBackButton);
            removeBackButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            DarkTheme.StyleButton(replaceBackButton);
            replaceBackButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            DarkTheme.StyleButton(editImageButton);
            editImageButton.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            // Custom-drawn ListView for better selection visuals
            CurrentListView.OwnerDraw = true;
            CurrentListView.DrawItem += CurrentListView_DrawItem;
            CurrentListView.MouseMove += CurrentListView_MouseMove;
            CurrentListView.MouseLeave += CurrentListView_MouseLeave;
        }

        private int _hoveredIndex = -1;

        private void CurrentListView_MouseMove(object sender, MouseEventArgs e)
        {
            var hit = CurrentListView.HitTest(e.Location);
            int idx = hit.Item != null ? hit.Item.Index : -1;
            if (idx != _hoveredIndex)
            {
                _hoveredIndex = idx;
                CurrentListView.Invalidate();
            }
        }

        private void CurrentListView_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                CurrentListView.Invalidate();
            }
        }

        private void CurrentListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            bool selected = e.Item.Selected;
            bool hovered = e.Item.Index == _hoveredIndex;

            Rectangle bounds = e.Bounds;
            int pad = 6;
            Rectangle card = new Rectangle(bounds.X + pad, bounds.Y + pad, bounds.Width - pad * 2, bounds.Height - pad * 2);

            // Card background
            Color cardBg;
            if (selected)
                cardBg = Color.FromArgb(38, 79, 120);    // DarkTheme.Selection
            else if (hovered)
                cardBg = Color.FromArgb(55, 55, 55);
            else
                cardBg = Color.FromArgb(38, 38, 38);

            using (var brush = new SolidBrush(cardBg))
            {
                // Rounded rectangle
                var path = RoundedRect(card, 8);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillPath(brush, path);
                path.Dispose();
            }

            // Border
            if (selected)
            {
                using (var pen = new Pen(DarkTheme.Accent, 2f))
                {
                    var path = RoundedRect(card, 8);
                    e.Graphics.DrawPath(pen, path);
                    path.Dispose();
                }
            }
            else if (hovered)
            {
                using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1f))
                {
                    var path = RoundedRect(card, 8);
                    e.Graphics.DrawPath(pen, path);
                    path.Dispose();
                }
            }

            // Draw image centered in card
            if (!string.IsNullOrEmpty(e.Item.ImageKey) && CurrentImageList.Images.ContainsKey(e.Item.ImageKey))
            {
                Image img = CurrentImageList.Images[e.Item.ImageKey];
                int imgSize = Math.Min(card.Width - 16, card.Height - 16);
                int imgX = card.X + (card.Width - imgSize) / 2;
                int imgY = card.Y + (card.Height - imgSize) / 2;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(img, imgX, imgY, imgSize, imgSize);
            }

            // Checkmark for selected items
            if (selected)
            {
                int ckSize = 22;
                int ckX = card.Right - ckSize - 4;
                int ckY = card.Top + 4;
                using (var circle = new SolidBrush(DarkTheme.Accent))
                {
                    e.Graphics.FillEllipse(circle, ckX, ckY, ckSize, ckSize);
                }
                // Draw checkmark
                using (var pen = new Pen(Color.White, 2f))
                {
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
                    e.Graphics.DrawLine(pen, ckX + 5, ckY + 11, ckX + 9, ckY + 15);
                    e.Graphics.DrawLine(pen, ckX + 9, ckY + 15, ckX + 16, ckY + 7);
                }
            }
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        #region LicenseChecking
        private void CheckLicenseAndUpdateUI()
        {
            this.Text = "PicSell - Полная версия";
            statsToolStripMenuItem.Enabled = true;
            PluginsToolStripMenuItem.Enabled = true;
        }
        #endregion

        #region RemoveBackground
        private IImageEditing _removeBackPlugin;

        private IImageEditing GetRemoveBackPlugin()
        {
            if (_removeBackPlugin != null)
                return _removeBackPlugin;

            // Check if already loaded via plugin system
            foreach (var p in PluginsLoadForm.plugins)
            {
                if (p.GetGUID() == "{E16EDEBB-7E81-4856-A9D1-CDB6B996A744}")
                {
                    _removeBackPlugin = p;
                    return _removeBackPlugin;
                }
            }

            // Load from known path
            string pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "plugins");
            string dllPath = Path.Combine(pluginDir, "RemoveBackPlugin.dll");

            if (!File.Exists(dllPath))
            {
                // Try alternate path
                dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins", "RemoveBackPlugin.dll");
            }

            if (!File.Exists(dllPath))
                throw new FileNotFoundException("RemoveBackPlugin.dll не найден.");

            Assembly asm = Assembly.LoadFrom(dllPath);
            var type = asm.GetTypes().FirstOrDefault(t => typeof(IImageEditing).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            if (type == null)
                throw new Exception("Плагин удаления фона не содержит реализации IImageEditing.");

            _removeBackPlugin = (IImageEditing)Activator.CreateInstance(type);
            return _removeBackPlugin;
        }

        private async void removeBackButton_Click(object sender, EventArgs e)
        {
            if (CurrentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите изображение для удаления фона.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Collect all selected image IDs
            var selectedItems = new List<ListViewItem>();
            foreach (ListViewItem item in CurrentListView.SelectedItems)
                selectedItems.Add(item);

            try
            {
                var plugin = GetRemoveBackPlugin();
                removeBackButton.Enabled = false;

                int total = selectedItems.Count;
                int done = 0;

                foreach (var item in selectedItems)
                {
                    string imageKey = item.ImageKey;
                    if (!int.TryParse(imageKey, out int imgId)) continue;

                    done++;
                    removeBackButton.Text = $"Обработка {done}/{total}...";

                    // Load image from DB
                    Image sourceImage = LoadImageFromDB(imgId);
                    if (sourceImage == null) continue;

                    Image newImage = await Task.Run(() => plugin.ProcessImage(sourceImage));
                    sourceImage.Dispose();

                    // If this is the currently displayed image, update PictureBox
                    if (CurrentPictureBox.Image != null && lastSelectedKey == imageKey)
                    {
                        CurrentPictureBox.Image = newImage;
                    }

                    commitNewVersion(newImage, imgId, "удаление фона", total > 1 ? "batch" : "one");
                }

                if (total > 1) updateListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении фона: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                removeBackButton.Enabled = true;
                removeBackButton.Text = "Удалить фон";
            }
        }

        private Image LoadImageFromDB(int imgId)
        {
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT Versions.VersionPath FROM Images
                        JOIN Versions ON Images.CurrentVersID = Versions.VersID
                        WHERE Images.ImgID = @imgId";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@imgId", imgId);
                        var result = cmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return null;
                        string filePath = result.ToString();
                        if (!File.Exists(filePath)) return null;
                        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            fs.CopyTo(ms);
                            ms.Position = 0;
                            return (Image)Image.FromStream(ms).Clone();
                        }
                    }
                }
            }
            catch { return null; }
        }
        private void replaceBackButton_Click(object sender, EventArgs e)
        {
            if (CurrentListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите изображения для замены фона.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите изображение для фона";
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    Image bgImage = Image.FromFile(ofd.FileName);

                    var selectedItems = new List<ListViewItem>();
                    foreach (ListViewItem item in CurrentListView.SelectedItems)
                        selectedItems.Add(item);

                    int total = selectedItems.Count;

                    if (total == 1)
                    {
                        // Single photo: open editor for positioning
                        string imageKey = selectedItems[0].ImageKey;
                        if (!int.TryParse(imageKey, out int imgId)) return;
                        Image foreground = LoadImageFromDB(imgId);
                        if (foreground == null) { MessageBox.Show("Не удалось загрузить изображение.", "Ошибка"); return; }

                        using (var editor = new BackgroundEditorForm(foreground, bgImage))
                        {
                            if (editor.ShowDialog() == DialogResult.OK)
                            {
                                if (lastSelectedKey == imageKey && CurrentPictureBox.Image != null)
                                    CurrentPictureBox.Image = editor.ResultImage;
                                commitNewVersion(editor.ResultImage, imgId, "замена фона");
                            }
                        }
                        foreground.Dispose();
                    }
                    else
                    {
                        // Batch: auto-center each foreground on background
                        replaceBackButton.Enabled = false;
                        int done = 0;

                        foreach (var item in selectedItems)
                        {
                            string imageKey = item.ImageKey;
                            if (!int.TryParse(imageKey, out int imgId)) continue;

                            done++;
                            replaceBackButton.Text = $"Замена {done}/{total}...";
                            Application.DoEvents();

                            Image foreground = LoadImageFromDB(imgId);
                            if (foreground == null) continue;

                            // Compose: draw background, center foreground on it
                            int w = bgImage.Width, h = bgImage.Height;
                            var result = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            using (Graphics g = Graphics.FromImage(result))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.DrawImage(bgImage, 0, 0, w, h);

                                float fx = (w - foreground.Width) / 2f;
                                float fy = (h - foreground.Height) / 2f;
                                g.DrawImage(foreground, fx, fy, foreground.Width, foreground.Height);
                            }

                            if (lastSelectedKey == imageKey && CurrentPictureBox.Image != null)
                                CurrentPictureBox.Image = result;

                            commitNewVersion(result, imgId, "замена фона", "batch");
                            foreground.Dispose();
                        }

                        updateListView();
                        replaceBackButton.Enabled = true;
                        replaceBackButton.Text = "Заменить фон";
                    }

                    bgImage.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при замене фона: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    replaceBackButton.Enabled = true;
                    replaceBackButton.Text = "Заменить фон";
                }
            }
        }

        private void editImageButton_Click(object sender, EventArgs e)
        {
            if (CurrentListView.SelectedItems.Count == 0 || CurrentPictureBox.Image == null)
            {
                MessageBox.Show("Сначала выберите и откройте изображение.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Image foreground = CurrentPictureBox.Image;
                // Background = copy of the same image so it shows 1:1
                // Foreground = same image as a movable/scalable element on top
                var background = new Bitmap(foreground);

                using (var editor = new BackgroundEditorForm(foreground, background))
                {
                    if (editor.ShowDialog() == DialogResult.OK)
                    {
                        CurrentPictureBox.Image = editor.ResultImage;

                        string imageKey = CurrentListView.SelectedItems[0].ImageKey;
                        if (int.TryParse(imageKey, out int imgId))
                        {
                            commitNewVersion(editor.ResultImage, imgId, "редактирование изображения");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при работе с редактором изображения: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        internal List<Pixel> pixels;
        bool onePhMode = false;
        public static string connectionString = "Data Source=DBpicsell.db;Version=3;Pooling=True;";
        string lastSelectedKey = null;

        internal List<Pixel> originalPixels;

        internal List<Pixel> GetPixels(Bitmap bitmap)
        {
            var pixels = new List<Pixel>(bitmap.Width * bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    pixels.Add(new Pixel()
                    {
                        Color = bitmap.GetPixel(x, y),
                        Point = new Point { X = x, Y = y }
                    });
                }
            }
            return pixels;
        }

        #region UpdatingGUI
        public static void AddImageWithAspectRatio(ImageList imageList, Image image, string key)
        {
            // Проверка на null
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            // Определяем соотношение сторон исходного изображения
            float aspectRatio = (float)image.Width / image.Height;

            // Вычисляем новые размеры с сохранением пропорций
            int newWidth, newHeight;

            if (aspectRatio > 1) // ширина больше высоты
            {
                newWidth = imageList.ImageSize.Width;
                newHeight = (int)(newWidth / aspectRatio);
            }
            else // высота больше ширины или равны
            {
                newHeight = imageList.ImageSize.Height;
                newWidth = (int)(newHeight * aspectRatio);
            }

            // Создаем bitmap с прозрачным фоном
            Bitmap newImage = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height);
            newImage.MakeTransparent();

            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                // Вычисляем позицию для центрирования изображения
                int x = (imageList.ImageSize.Width - newWidth) / 2;
                int y = (imageList.ImageSize.Height - newHeight) / 2;

                // Рисуем изображение с сохранением пропорций по центру
                g.DrawImage(image, x, y, newWidth, newHeight);
            }

            // Добавляем в ImageList с ключом
            imageList.Images.Add(key, newImage);
        }
        public void SelectListViewItemByImgID(string key)
        {

            foreach (ListViewItem item in CurrentListView.Items)
            {
                if (item.ImageKey == key)
                {
                    // Снять выделение со всех
                    foreach (ListViewItem i in CurrentListView.SelectedItems)
                        i.Selected = false;

                    // Выделить нужный
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible(); // Прокрутка к элементу
                    break;
                }
            }
        }
        public void LoadPhotosFromDB()
        {
            string query = @"
                SELECT Images.ImgID, Versions.VersionPath
                FROM Images
                JOIN Versions ON Images.CurrentVersID = Versions.VersID";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            int imgId = reader.GetInt32(0);
                            string filePath = reader["VersionPath"].ToString();

                            if (File.Exists(filePath))
                            {
                                using (var img = Image.FromFile(filePath))
                                {
                                    
                                    //CurrentImageList.Images.Add(imgId.ToString(), img); // ключ — строка
                                    AddImageWithAspectRatio(CurrentImageList, img, imgId.ToString());
                                    CurrentListView.Items.Add(new ListViewItem
                                    {
                                        ImageKey = imgId.ToString()
                                        //Text = $"Image {imgId}" // Можно заменить на что-то осмысленнее
                                    });

                                }
                            }
                            else
                            {
                                Console.WriteLine($"Файл не найден: {filePath}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при загрузке изображения: {ex.Message}");
                        }
                    }
                }
            }
            if (lastSelectedKey != null) SelectListViewItemByImgID(lastSelectedKey);
        }

        public void updateModeGUI()
        {
            if (onePhMode)
            {
                CurrentPictureBox.Visible = true;
                CurrentListView.Visible = false;
                versionsHistoryPanel.Visible = true;

                // среднее значение между минимальным и максимальным значениями
                int middleValue = (SaturationTrackBar.Minimum + SaturationTrackBar.Maximum) / 2;
                int middleValueHue = (HueTrackBar.Minimum + HueTrackBar.Maximum) / 2;

                // значение ползунка в середину диапазона
                SaturationTrackBar.Value = middleValue;
                LightnessTrackBar.Value = middleValue;
                HueTrackBar.Value = middleValueHue;

                hslGroupBox.Enabled = true;
                drawGroupBox.Visible = false;
            }
            else
            {
                CurrentPictureBox.Visible = false;
                CurrentListView.Visible = true;
                versionsHistoryPanel.Visible = false;

                hslGroupBox.Enabled = false;
                drawGroupBox.Visible = false;
            }
        }

        public void updateListView()
        {
            CurrentImageList.Images.Clear();
            CurrentListView.Clear();
            //curImageIndex = -1;
            LoadPhotosFromDB();
        }
        private void manyPhButton_Click(object sender, EventArgs e)
        {
            onePhMode = false;
            updateModeGUI();
        }

        private void onePhButton_Click(object sender, EventArgs e)
        {
            if (CurrentListView.SelectedItems.Count == 0) return;
            onePhMode = true;
            updateModeGUI();
            // Force load the selected image
            CurrentListView_SelectedIndexChanged(sender, e);
        }

        private void CurrentListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CurrentListView.SelectedItems.Count > 0)
            {
                onePhButton.Enabled = true;

                // Only load full image into PictureBox when in single-photo mode
                if (onePhMode)
                {
                    try
                    {
                        var selectedItem = CurrentListView.SelectedItems[0];
                        string imageKey = selectedItem.ImageKey;

                        if (int.TryParse(imageKey, out int imgId))
                        {
                            using (var connection = new SQLiteConnection(connectionString))
                            {
                                connection.Open();

                                string query = @"
                                    SELECT Versions.VersionPath
                                    FROM Images
                                    JOIN Versions ON Images.CurrentVersID = Versions.VersID
                                    WHERE Images.ImgID = @imgId";

                                using (var command = new SQLiteCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@imgId", imgId);

                                    using (var reader = command.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            string filePath = reader["VersionPath"].ToString();

                                            if (File.Exists(filePath))
                                            {
                                                if (CurrentPictureBox.Image != null)
                                                {
                                                    var oldImage = CurrentPictureBox.Image;
                                                    CurrentPictureBox.Image = null;
                                                    oldImage.Dispose();
                                                }

                                                Image fullImage;
                                                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                                                using (MemoryStream ms = new MemoryStream())
                                                {
                                                    fs.CopyTo(ms);
                                                    ms.Position = 0;
                                                    fullImage = Image.FromStream(ms);
                                                    CurrentPictureBox.Image = (Image)fullImage.Clone();
                                                }

                                                using (var bitmap = new Bitmap(fullImage))
                                                {
                                                    pixels = GetPixels(bitmap);
                                                    originalPixels = new List<Pixel>(pixels.Select(pixel => new Pixel(pixel)));
                                                    originalBitmap = CreateBitmapFromPixels(originalPixels, bitmap.Width, bitmap.Height);
                                                }

                                                fullImage.Dispose();
                                            }
                                            else
                                            {
                                                MessageBox.Show($"Файл изображения не найден: {filePath}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                CurrentPictureBox.Image = null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        UpdateVersionButtonsState();
                        lastSelectedKey = imageKey;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отображении изображения: {ex.Message}");
                    }
                }
            }
            else if (CurrentListView.SelectedItems.Count <= 0) onePhButton.Enabled = false;
        }

        #endregion

        #region LoadPlugins
        private void PluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PluginsLoadForm pluginsWindow = new PluginsLoadForm(); 
            pluginsWindow.ShowDialog(); 
        }
        #endregion

        #region updateDB

        private void SaveImageToDatabaseAndFile(SQLiteConnection connection, string filePath)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // 1. Вставка в Images
                    var insertImageCmd = new SQLiteCommand("INSERT INTO Images (CurrentVersID, OrigPath) VALUES (0, @path)", connection, transaction);
                    insertImageCmd.Parameters.AddWithValue("@path", filePath);
                    insertImageCmd.ExecuteNonQuery();
                    long imgId = connection.LastInsertRowId;

                    // 2. Загрузка и конвертация в PNG
                    byte[] imageBytes;
                    using (var img = Image.FromFile(filePath))
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }

                    // 3. Создание папки versions при необходимости
                    string versionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
                    if (!Directory.Exists(versionsFolder))
                        Directory.CreateDirectory(versionsFolder);

                    // 4. Вставка временной версии
                    var insertVersionCmd = new SQLiteCommand("INSERT INTO Versions (OrigVersID, VersionPath) VALUES (0, 'temp')", connection, transaction);
                    insertVersionCmd.ExecuteNonQuery();
                    long versId = connection.LastInsertRowId;

                    // 5. Сохранение файла
                    string versPath = Path.Combine(versionsFolder, $"{versId}.png");
                    File.WriteAllBytes(versPath, imageBytes);

                    // 6. Обновление пути и оригинальной версии
                    var updateVersionCmd = new SQLiteCommand(
                        "UPDATE Versions SET VersionPath = @filePath WHERE VersID = @versId;" +
                        "UPDATE Versions SET OrigVersID = VersID WHERE VersID = @versId", connection, transaction);
                    updateVersionCmd.Parameters.AddWithValue("@filePath", versPath);
                    updateVersionCmd.Parameters.AddWithValue("@versId", versId);
                    updateVersionCmd.ExecuteNonQuery();

                    // 7. Добавление комментария
                    var updateChangeCmd = new SQLiteCommand(
                        "UPDATE Versions SET Change = @changeText WHERE VersID = @versId", connection, transaction);
                    updateChangeCmd.Parameters.AddWithValue("@changeText", "до исходного состояния");
                    updateChangeCmd.Parameters.AddWithValue("@versId", versId);
                    updateChangeCmd.ExecuteNonQuery();

                    // 8. Установка текущей версии в Images
                    var updateImageCmd = new SQLiteCommand(
                        "UPDATE Images SET CurrentVersID = @versId WHERE ImgID = @imgId", connection, transaction);
                    updateImageCmd.Parameters.AddWithValue("@versId", versId);
                    updateImageCmd.Parameters.AddWithValue("@imgId", imgId);
                    updateImageCmd.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при добавлении изображения: " + ex.Message);
                }
            }
        }


        //загрузка в бд новых изображений используя openfiledialog
        private void loadPhotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var connection = new SQLiteConnection(MainForm.connectionString))
                    {
                        connection.Open();
                        foreach (string filePath in openFileDialog.FileNames)
                        {
                            SaveImageToDatabaseAndFile(connection, filePath);
                        }
                    }
                }
            }
            updateListView();
        }



        //удаление всех данных из бд + обновление listview
        private void clearWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                text: "Вы уверены что хотите удалить все фотографии из существующей рабочей области?\n\nВсе фотографии удалятся без возможности восстановления!",
                "Предупреждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

                if (result == DialogResult.Yes)
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        var deleteCmd = new SQLiteCommand(@"
                DELETE FROM Versions;
                DELETE FROM Images;
            ", connection);
                        deleteCmd.ExecuteNonQuery();
                    }
                    updateListView();
                    
                    //удалить картинку из pictureBox
                    if (CurrentPictureBox.Image != null)
                    {
                        CurrentPictureBox.Image.Dispose();
                        CurrentPictureBox.Image = null;
                    }
                }

                // Очистить папку versions
                string versionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
                if (Directory.Exists(versionsFolder))
                {
                    foreach (string file in Directory.GetFiles(versionsFolder))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при удалении файла: " + ex.Message);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при очистке рабочей области: " + ex.Message);
            }
            

        }


        //создание новой версии изображения в бд + удаление ненужных веток версий
        public void commitNewVersion(Image image, int imgId, string change = null, string amount="one")
        {
            try
            {
                if (image == null) return;

                string versionsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
                if (!Directory.Exists(versionsFolder))
                {
                    Directory.CreateDirectory(versionsFolder);
                }

                // Получаем изображение в виде байтов
                byte[] newImageBytes;
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    newImageBytes = ms.ToArray();
                }

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        long prevVersId = -1;
                        long origVersId = imgId;

                        // Получаем текущую (последнюю) версию изображения
                        using (var getCurrentCmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgId", connection, transaction))
                        {
                            getCurrentCmd.Parameters.AddWithValue("@imgId", imgId);
                            object result = getCurrentCmd.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                prevVersId = Convert.ToInt64(result);
                            }
                        }

                        // Получаем OrigVersID, если есть предыдущая версия
                        if (prevVersId != -1)
                        {
                            using (var getPrevInfoCmd = new SQLiteCommand("SELECT OrigVersID FROM Versions WHERE VersID = @prevId", connection, transaction))
                            {
                                getPrevInfoCmd.Parameters.AddWithValue("@prevId", prevVersId);
                                object result = getPrevInfoCmd.ExecuteScalar();
                                if (result != DBNull.Value && result != null)
                                {
                                    origVersId = Convert.ToInt64(result);
                                }
                            }

                            DeleteFutureVersions(prevVersId, connection, transaction);
                        }

                        // Временная вставка
                        long newVersId;
                        using (var insertCmd = new SQLiteCommand(
                            "INSERT INTO Versions (OrigVersID, PrevVersID, VersionPath) VALUES (@origId, @prevId, 'temp')",
                            connection, transaction))
                        {
                            insertCmd.Parameters.AddWithValue("@origId", origVersId);
                            insertCmd.Parameters.AddWithValue("@prevId", prevVersId != -1 ? (object)prevVersId : DBNull.Value);
                            insertCmd.ExecuteNonQuery();
                            newVersId = connection.LastInsertRowId;
                        }

                        string filePath = Path.Combine(versionsFolder, $"{newVersId}.png");
                        File.WriteAllBytes(filePath, newImageBytes);

                        using (var updateCmd = new SQLiteCommand(
                            "UPDATE Versions SET VersionPath = @filePath WHERE VersID = @versId",
                            connection, transaction))
                        {
                            updateCmd.Parameters.AddWithValue("@filePath", filePath);
                            updateCmd.Parameters.AddWithValue("@versId", newVersId);
                            updateCmd.ExecuteNonQuery();
                        }

                        if (!string.IsNullOrWhiteSpace(change))
                        {
                            using (var updateChangeCmd = new SQLiteCommand(
                                "UPDATE Versions SET Change = @changeText WHERE VersID = @versId",
                                connection, transaction))
                            {
                                updateChangeCmd.Parameters.AddWithValue("@changeText", change);
                                updateChangeCmd.Parameters.AddWithValue("@versId", newVersId);
                                updateChangeCmd.ExecuteNonQuery();
                            }
                        }

                        using (var updateImageCmd = new SQLiteCommand(
                            "UPDATE Images SET CurrentVersID = @newVersId WHERE ImgID = @imgId",
                            connection, transaction))
                        {
                            updateImageCmd.Parameters.AddWithValue("@newVersId", newVersId);
                            updateImageCmd.Parameters.AddWithValue("@imgId", imgId);
                            updateImageCmd.ExecuteNonQuery();
                        }

                        if (prevVersId != -1)
                        {
                            using (var updatePrevCmd = new SQLiteCommand(
                                "UPDATE Versions SET NextVersID = @newVersId WHERE VersID = @prevVersId",
                                connection, transaction))
                            {
                                updatePrevCmd.Parameters.AddWithValue("@newVersId", newVersId);
                                updatePrevCmd.Parameters.AddWithValue("@prevVersId", prevVersId);
                                updatePrevCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                }
                if (amount == "one")
                {
                    updateListView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении новой версии изображения: " + ex.Message,
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // вспомогательный метод для удаления альтернативных веток версий
        void DeleteFutureVersions(long fromVersId, SQLiteConnection conn, SQLiteTransaction tx)
        {
            // Получаем всех "детей" текущей версии
            using (var cmd = new SQLiteCommand("SELECT VersID FROM Versions WHERE PrevVersID = @fromId", conn, tx))
            {
                cmd.Parameters.AddWithValue("@fromId", fromVersId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long childVersId = reader.GetInt64(0);

                        // Рекурсивно удаляем потомков
                        DeleteFutureVersions(childVersId, conn, tx);

                        // Получаем путь к файлу
                        string versionPath = null;
                        using (var pathCmd = new SQLiteCommand("SELECT VersionPath FROM Versions WHERE VersID = @id", conn, tx))
                        {
                            pathCmd.Parameters.AddWithValue("@id", childVersId);
                            versionPath = pathCmd.ExecuteScalar() as string;
                        }

                        // Удаляем файл, если он существует
                        if (!string.IsNullOrEmpty(versionPath) && File.Exists(versionPath))
                        {
                            try
                            {
                                File.Delete(versionPath);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось удалить файл \"{versionPath}\": {ex.Message}", "Ошибка удаления файла", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        // Удаляем саму версию
                        using (var deleteCmd = new SQLiteCommand("DELETE FROM Versions WHERE VersID = @id", conn, tx))
                        {
                            deleteCmd.Parameters.AddWithValue("@id", childVersId);
                            deleteCmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            // Очищаем NextVersID у текущей версии (она больше не ведёт к будущему)
            using (var clearNextCmd = new SQLiteCommand("UPDATE Versions SET NextVersID = NULL WHERE VersID = @id", conn, tx))
            {
                clearNextCmd.Parameters.AddWithValue("@id", fromVersId);
                clearNextCmd.ExecuteNonQuery();
            }
        }


        #endregion

        #region Drawing
        Color drawColor = Color.Black;
        private void panel1_Click(object sender, EventArgs e)
        {
            if (this.drawColorDialog.ShowDialog() == DialogResult.OK)
            {
                drawColor = this.drawColorDialog.Color;
                this.panel1.BackColor = drawColor;
            }
        }

        Graphics g;
        Point startPoint;

        private bool isMouse = false;

        private void CurrentPictureBox_MouseDown(object sender, MouseEventArgs e) { }
        private void CurrentPictureBox_MouseMove(object sender, MouseEventArgs e) { }
        private void CurrentPictureBox_MouseUp(object sender, MouseEventArgs e) { }

        //для получения правильных координат изображения в пикчербоксе
        private Point GetImageCoordinates(Point pictureBoxCoordinates)
        {
            // Получаем размеры изображения в CurrentPictureBox
            int imageWidth = CurrentPictureBox.Image.Width;
            int imageHeight = CurrentPictureBox.Image.Height;

            // Получаем размеры CurrentPictureBox
            int pictureBoxWidth = CurrentPictureBox.Width;
            int pictureBoxHeight = CurrentPictureBox.Height;

            // Вычисляем масштаб изображения по горизонтали и вертикали
            float horizontalScale = (float)pictureBoxWidth / imageWidth;
            float verticalScale = (float)pictureBoxHeight / imageHeight;

            // Используем минимальный масштаб, чтобы изображение целиком вписалось в pictureBox1
            float scale = Math.Min(horizontalScale, verticalScale);

            // Рассчитываем размеры изображения после масштабирования
            int scaledWidth = (int)(imageWidth * scale);
            int scaledHeight = (int)(imageHeight * scale);

            // Рассчитываем смещение изображения внутри pictureBox1
            int offsetX = (pictureBoxWidth - scaledWidth) / 2;
            int offsetY = (pictureBoxHeight - scaledHeight) / 2;

            // Корректируем координаты мыши, вычитая смещение и учитывая масштаб
            int imageX = (int)((pictureBoxCoordinates.X - offsetX) / scale);
            int imageY = (int)((pictureBoxCoordinates.Y - offsetY) / scale);

            return new Point(imageX, imageY);
        }
        #endregion

        #region WatchVersions
        private Image curImage; // Переменная для хранения изображения, которое отображается при нажатии
        private Timer visibleTimer = new Timer(); // для временного отображения изменений в истории версий

        private void ShowChangeLabel(string changeText)
        {
            changeLabel.Text = changeText;
            changeLabel.Visible = true;

            visibleTimer.Interval = 2000; // 3 секунды
            visibleTimer.Tick += (s, e) =>
            {
                changeLabel.Visible = false;
                visibleTimer.Stop();
            };
            visibleTimer.Start();
        }

        private void prevVersButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentListView.SelectedItems.Count == 0) return;

                string imageKey = CurrentListView.SelectedItems[0].ImageKey;
                if (!int.TryParse(imageKey, out int imgId)) return;

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Получаем текущую версию
                    long currentVersId;
                    using (var getCurrentCmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgId", connection))
                    {
                        getCurrentCmd.Parameters.AddWithValue("@imgId", imgId);
                        var result = getCurrentCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return;
                        currentVersId = Convert.ToInt64(result);
                    }

                    // Получаем PrevVersID
                    long prevVersId;
                    using (var getPrevCmd = new SQLiteCommand("SELECT PrevVersID FROM Versions WHERE VersID = @versId", connection))
                    {
                        getPrevCmd.Parameters.AddWithValue("@versId", currentVersId);
                        var result = getPrevCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return;
                        prevVersId = Convert.ToInt64(result);
                    }

                    // Получаем текст изменения (change) у текущей версии
                    string change = "";
                    using (var getChangeCmd = new SQLiteCommand("SELECT Change FROM Versions WHERE VersID = @versId", connection))
                    {
                        getChangeCmd.Parameters.AddWithValue("@versId", currentVersId);
                        var result = getChangeCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            change = result.ToString();
                        }
                    }

                    // Загружаем изображение предыдущей версии
                    string imagePath;
                    using (var getImageCmd = new SQLiteCommand("SELECT VersionPath FROM Versions WHERE VersID = @versId", connection))
                    {
                        getImageCmd.Parameters.AddWithValue("@versId", prevVersId);
                        var result = getImageCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return;
                        imagePath = result.ToString();
                    }
                    
                    if (File.Exists(imagePath))
                    {
                        try
                        {
                            // Загружаем изображение из файла
                            CurrentPictureBox.Image = Image.FromFile(imagePath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                                           "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            CurrentPictureBox.Image = null; // Очищаем PictureBox в случае ошибки
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Файл изображения не найден: {imagePath}",
                                       "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        CurrentPictureBox.Image = null; // Очищаем PictureBox если файл не найден
                    }

                    // Обновляем CurrentVersID в Images
                    using (var updateCmd = new SQLiteCommand("UPDATE Images SET CurrentVersID = @versId WHERE ImgID = @imgId", connection))
                    {
                        updateCmd.Parameters.AddWithValue("@versId", prevVersId);
                        updateCmd.Parameters.AddWithValue("@imgId", imgId);
                        updateCmd.ExecuteNonQuery();
                    }


                    ShowChangeLabel("Отменено " + change);

                    updateListView();
                    UpdateVersionButtonsState();

                    updateModeGUI(); //чтобы в нужное место возвращались ползунки
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при переходе к предыдущей версии: " + ex.Message);
            }
        }

        private void nextVersButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentListView.SelectedItems.Count == 0) return;

                string imageKey = CurrentListView.SelectedItems[0].ImageKey;
                if (!int.TryParse(imageKey, out int imgId)) return;

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Получаем текущую версию
                    long currentVersId;
                    using (var getCurrentCmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgId", connection))
                    {
                        getCurrentCmd.Parameters.AddWithValue("@imgId", imgId);
                        var result = getCurrentCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return;
                        currentVersId = Convert.ToInt64(result);
                    }

                    // Получаем NextVersID
                    long nextVersId;
                    using (var getNextCmd = new SQLiteCommand("SELECT NextVersID FROM Versions WHERE VersID = @versId", connection))
                    {
                        getNextCmd.Parameters.AddWithValue("@versId", currentVersId);
                        var result = getNextCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value) return;
                        nextVersId = Convert.ToInt64(result);
                    }

                    // Получаем текст изменения (change) у следующей версии
                    string change = "";
                    using (var getChangeCmd = new SQLiteCommand("SELECT Change FROM Versions WHERE VersID = @versId", connection))
                    {
                        getChangeCmd.Parameters.AddWithValue("@versId", nextVersId);
                        var result = getChangeCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            change = result.ToString();
                        }
                    }

                    // Загружаем изображение следующей версии
                    string imagePath;
                    using (var getImageCmd = new SQLiteCommand("SELECT VersionPath FROM Versions WHERE VersID = @versId", connection))
                    {
                        getImageCmd.Parameters.AddWithValue("@versId", nextVersId);
                        var result = getImageCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("Путь к изображению следующей версии не найден в базе данных",
                                           "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        imagePath = result.ToString();
                    }
                    try
                    {
                        if (File.Exists(imagePath))
                        {
                            // Используем using для корректного освобождения ресурсов
                            using (var bmpTemp = new Bitmap(imagePath))
                            {
                                CurrentPictureBox.Image = new Bitmap(bmpTemp);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Файл изображения не найден по пути: {imagePath}",
                                           "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            CurrentPictureBox.Image = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                                       "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CurrentPictureBox.Image = null;
                    }

                    // Обновляем CurrentVersID в Images
                    using (var updateCmd = new SQLiteCommand("UPDATE Images SET CurrentVersID = @versId WHERE ImgID = @imgId", connection))
                    {
                        updateCmd.Parameters.AddWithValue("@versId", nextVersId);
                        updateCmd.Parameters.AddWithValue("@imgId", imgId);
                        updateCmd.ExecuteNonQuery();
                    }

                    ShowChangeLabel("Восстановлено " + change);

                    updateListView();
                    UpdateVersionButtonsState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при переходе к следующей версии: " + ex.Message);
            }
        }

        private void UpdateVersionButtonsState()
        {
            if (CurrentListView.SelectedItems.Count == 0)
            {
                prevVersButton.Enabled = false;
                nextVersButton.Enabled = false;
                return;
            }

            string imageKey = CurrentListView.SelectedItems[0].ImageKey;
            if (!int.TryParse(imageKey, out int imgId))
            {
                prevVersButton.Enabled = false;
                nextVersButton.Enabled = false;
                return;
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                long currentVersId;
                using (var getCurrentCmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgId", connection))
                {
                    getCurrentCmd.Parameters.AddWithValue("@imgId", imgId);
                    var result = getCurrentCmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        prevVersButton.Enabled = false;
                        nextVersButton.Enabled = false;
                        return;
                    }
                    currentVersId = Convert.ToInt64(result);
                }

                // Проверка на наличие предыдущей версии
                using (var checkPrev = new SQLiteCommand("SELECT PrevVersID FROM Versions WHERE VersID = @versId", connection))
                {
                    checkPrev.Parameters.AddWithValue("@versId", currentVersId);
                    var result = checkPrev.ExecuteScalar();
                    prevVersButton.Enabled = (result != DBNull.Value && result != null);
                }

                // Проверка на наличие следующей версии
                using (var checkNext = new SQLiteCommand("SELECT NextVersID FROM Versions WHERE VersID = @versId", connection))
                {
                    checkNext.Parameters.AddWithValue("@versId", currentVersId);
                    var result = checkNext.ExecuteScalar();
                    nextVersButton.Enabled = (result != DBNull.Value && result != null);
                }
            }
        }

        private void origVersButton_MouseDown(object sender, MouseEventArgs e)
        {
            if (pixels == null || CurrentPictureBox.Image == null || CurrentListView.SelectedItems.Count == 0)
                return;

            string imageKey = CurrentListView.SelectedItems[0].ImageKey;
            if (!int.TryParse(imageKey, out int imgId))
                return;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Получаем текущую версию изображения
                long currentVersId;
                using (var cmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgId", connection))
                {
                    cmd.Parameters.AddWithValue("@imgId", imgId);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return;
                    currentVersId = Convert.ToInt64(result);
                }

                // Получаем OrigVersID текущей версии
                long origVersId;
                using (var cmd = new SQLiteCommand("SELECT OrigVersID FROM Versions WHERE VersID = @versId", connection))
                {
                    cmd.Parameters.AddWithValue("@versId", currentVersId);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return;
                    origVersId = Convert.ToInt64(result);
                }

                // Получаем путь к оригинальному изображению по OrigVersID
                string imagePath;
                using (var cmd = new SQLiteCommand("SELECT VersionPath FROM Versions WHERE VersID = @versId", connection))
                {
                    cmd.Parameters.AddWithValue("@versId", origVersId);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        MessageBox.Show("Оригинальное изображение не найдено в базе данных",
                                       "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    imagePath = result.ToString();
                }

                try
                {
                    if (File.Exists(imagePath))
                    {
                        curImage = CurrentPictureBox.Image;
                        using (var originalImage = Image.FromFile(imagePath))
                        {
                            CurrentPictureBox.Image = new Bitmap(originalImage);
                            
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Файл оригинального изображения не найден по пути: {imagePath}",
                                       "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке оригинального изображения: {ex.Message}\nПуть: {imagePath}",
                                   "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Восстанавливаем предыдущее изображение в случае ошибки
                    CurrentPictureBox.Image = curImage;
                }
            }
        }

        private void origVersButton_MouseUp(object sender, MouseEventArgs e)
        {
            if (pixels == null || curImage == null)
                return;

            // Возвращаем оригинальное изображение
            CurrentPictureBox.Image = curImage;
        }


        #endregion

        #region HSL

        internal Bitmap originalBitmap;

        internal Bitmap CreateBitmapFromPixels(List<Pixel> pixels, int width, int height)
        {

            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Pixel pixel = pixels[y * width + x];
                        bitmap.SetPixel(x, y, pixel.Color);
                    }
                }
            }
            return bitmap;
        }

        internal void ChangeSaturation(Pixel pixel, float saturationDelta)
        {
            Color originalColor = pixel.Color;
            float h = originalColor.GetHue();
            float s = originalColor.GetSaturation() + saturationDelta;
            float l = originalColor.GetBrightness();
            if (s < 0) { s = 0; }
            else if (s > 1) { s = 1; }
            pixel.SetHSL(h, s, l);
        }

        private void ChangeLightness(Pixel pixel, float lightnessDelta)
        {
            Color originalColor = pixel.Color;
            float h = originalColor.GetHue();
            float s = originalColor.GetSaturation();
            float l = originalColor.GetBrightness() + lightnessDelta;
            if (l < 0) { l = 0; }
            else if (l > 1) { l = 1; }
            pixel.SetHSL(h, s, l);
        }

        private void ChangeHue(Pixel pixel, float hueDelta)
        {
            Color originalColor = pixel.Color;

            float h = (originalColor.GetHue() + hueDelta) % 360; // Оттенок находится в диапазоне [0, 360], поэтому используем операцию модуля для обработки выхода за границы
            if (h < 0) h += 360; 
            if (h == 360) h = 0; // На всякий случай

            float s = originalColor.GetSaturation();
            float l = originalColor.GetBrightness();
            pixel.SetHSL(h, s, l);
        }

        private int previousHue = 180;

        private void HueTrackBar_MouseUp(object sender, MouseEventArgs e)
        {

            if (pixels == null)
            {
                return;
            }

            int currentValue = HueTrackBar.Value;

            float deltaHue = currentValue - previousHue;

            var tempBitmap = CreateBitmapFromPixels(pixels, originalBitmap.Width, originalBitmap.Height);
            var tempPixels = new List<Pixel>(pixels.Select(pixel => new Pixel(pixel)));

            foreach (var pixel in tempPixels)
            {
                if (pixel.Color.A == 0) continue; // Пропускаем полностью прозрачные пиксели
                ChangeHue(pixel, deltaHue);
                tempBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);
            }

            CurrentPictureBox.Image = tempBitmap;
            previousHue = currentValue;

            string imageKey = CurrentListView.SelectedItems[0].ImageKey;
            if (int.TryParse(imageKey, out int imgId))
            {
                commitNewVersion((Image)tempBitmap, imgId, "изменение оттенка");
            }
        }

        private float previousSaturation = 0.5f;

        private void SaturationTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (pixels == null)
            {
                return;
            }

            float currentValue = SaturationTrackBar.Value / 100f;

            float deltaSatur = currentValue - previousSaturation;

            var tempBitmap = CreateBitmapFromPixels(pixels, originalBitmap.Width, originalBitmap.Height);
            var tempPixels = new List<Pixel>(pixels.Select(pixel => new Pixel(pixel)));

            foreach (var pixel in tempPixels)
            {
                if (pixel.Color.A == 0) continue; // Пропускаем полностью прозрачные пиксели
                ChangeSaturation(pixel, deltaSatur);
                tempBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color);
            }

            CurrentPictureBox.Image = tempBitmap;
            previousSaturation = currentValue;

            string imageKey = CurrentListView.SelectedItems[0].ImageKey;
            if (int.TryParse(imageKey, out int imgId))
            {
                commitNewVersion((Image)tempBitmap, imgId, "изменение насыщенности");
            }
        }

        private float previousLightness = 0.5f; // Переменная для хранения предыдущего значения ползунка

        private void LightnessTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (pixels == null)
            {
                return;
            }

            float currentValue = LightnessTrackBar.Value / 100f;

            float deltaLightness = currentValue - previousLightness;

            var tempBitmap = CreateBitmapFromPixels(pixels, originalBitmap.Width, originalBitmap.Height);
            var tempPixels = new List<Pixel>(pixels.Select(pixel => new Pixel(pixel)));

            foreach (var pixel in tempPixels)
            {
                if (pixel.Color.A == 0) continue; // Пропускаем полностью прозрачные пиксели
                ChangeLightness(pixel, deltaLightness);
                tempBitmap.SetPixel(pixel.Point.X, pixel.Point.Y, pixel.Color); // 
            }

            CurrentPictureBox.Image = tempBitmap;
            previousLightness = currentValue;

            string imageKey = CurrentListView.SelectedItems[0].ImageKey;
            if (int.TryParse(imageKey, out int imgId))
            {
                commitNewVersion((Image)tempBitmap, imgId, "изменение яркости");
            }
        }

        #endregion

        private void savePhotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() != DialogResult.OK)
                    return;

                string folderPath = folderDialog.SelectedPath;

                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Получаем все ImgID и пути к текущим версиям изображений
                    string query = @"
            SELECT Images.ImgID, Versions.VersionPath
            FROM Images
            JOIN Versions ON Images.CurrentVersID = Versions.VersID";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int imgId = reader.GetInt32(0);
                            string sourcePath = reader["VersionPath"].ToString();

                            try
                            {
                                if (!File.Exists(sourcePath))
                                {
                                    MessageBox.Show($"Файл по пути \"{sourcePath}\" не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    continue;
                                }

                                string fileExtension = Path.GetExtension(sourcePath);
                                string destinationPath = Path.Combine(folderPath, $"Image_{imgId}{fileExtension}");

                                File.Copy(sourcePath, destinationPath, overwrite: true);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при копировании изображения ID {imgId}: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }

                MessageBox.Show("Изображения успешно сохранены.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void aboutBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutWindow = new AboutBox(); // Создаем экземпляр окна
            aboutWindow.ShowDialog(); // Показываем как модальное окно
        }

        private void statisticToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // подключиться к дб logs.db
            StatisticForm sf = new StatisticForm(); // Создаем экземпляр окна
            sf.ShowDialog(); // Показываем как модальное окно
        }

        private void CurrentListView_DragEnter(object sender, DragEventArgs e)
        {
            // Разрешаем перетаскивание только файлов
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void CurrentListView_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string[] imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

            var imageFiles = files.Where(file =>
                imageExtensions.Contains(Path.GetExtension(file).ToLower())).ToList();

            if (imageFiles.Count == 0)
            {
                MessageBox.Show("Нет изображений в перетаскиваемых файлах.");
                return;
            }

            using (var connection = new SQLiteConnection(MainForm.connectionString))
            {
                connection.Open();
                foreach (var file in imageFiles)
                {
                    SaveImageToDatabaseAndFile(connection, file);
                }
            }

            updateListView(); // обнови ListView после загрузки
        }

        private void statsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StatisticForm statisticForm = new StatisticForm();
            statisticForm.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            PluginManager pm = new PluginManager();
            pm.WritePluginsToIni();
        }
    }

    public class ToggleListView : System.Windows.Forms.ListView
    {
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private bool _swallowed;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDOWN)
            {
                int x = (short)(m.LParam.ToInt32() & 0xFFFF);
                int y = (short)((m.LParam.ToInt32() >> 16) & 0xFFFF);
                var hit = this.HitTest(x, y);

                if (hit.Item != null)
                {
                    hit.Item.Selected = !hit.Item.Selected;
                    hit.Item.Focused = true;
                    _swallowed = true;
                    return;
                }
                _swallowed = false;
            }

            // Swallow the matching mouse-up so base doesn't reset selection
            if (m.Msg == WM_LBUTTONUP && _swallowed)
            {
                _swallowed = false;
                return;
            }

            base.WndProc(ref m);
        }
    }
}
