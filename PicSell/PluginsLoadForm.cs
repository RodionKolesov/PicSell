using PluginBase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicSell
{
    // класс работающий не только с формой, но и выполняющий непосредственную загрузку плагинов, отрисовку интерфейсов и тп.
    public partial class PluginsLoadForm : Form
    {
        public static PluginsLoadForm Instance { get; private set; }

        public static List<IImageEditing> plugins = new List<IImageEditing>();

        public static List<string> pluginPaths = new List<string>(); // путь к DLL

        public static List<string> guids = new List<string>();
        
        public PluginsLoadForm()
        {
            InitializeComponent();
            DarkTheme.Apply(this);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Text = "PicSell \u2014 \u041F\u043B\u0430\u0433\u0438\u043D\u044B";

            //отрисовка ранее загруженных плагинов (если они есть)
            if (plugins?.Any() == true)
            {
                foreach (var plugin in plugins)
                {
                    try
                    {
                        string info = plugin.GetInfo();
                        AddPluginLabel(info);
                    }
                    catch (Exception ex)
                    {
                        AddPluginLabel($"Ошибка при загрузке плагина: {ex.Message}");
                    }
                }
            }
        }

        private void loadPluginButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Plugin DLL (*.dll)|*.dll",
                Title = "Выберите DLL с плагином"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string dllPath = ofd.FileName;

                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(dllPath);

                    var pluginTypes = pluginAssembly.GetTypes()
                        .Where(t => typeof(IImageEditing).IsAssignableFrom(t)
                                 && !t.IsInterface
                                 && !t.IsAbstract);

                    foreach (var type in pluginTypes)
                    {
                        if (Activator.CreateInstance(type) is IImageEditing plugin)
                        {
                            string guid = plugin.GetGUID();

                            if (guids.Contains(guid))
                            {
                                MessageBox.Show("Этот плагин уже подключен.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            }
                            else
                            {
                                plugins.Add(plugin);
                                guids.Add(plugin.GetGUID());
                                pluginPaths.Add(dllPath); // <--- сохраняем путь к DLL

                                AddPluginLabel(plugin.GetInfo());
                                AddPluginImgEditingButton(plugin);

                            }
                        }

                        //if type == другому то обработка другая
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    string errors = string.Join("\n", ex.LoaderExceptions.Select(error => error.Message));
                    MessageBox.Show($"Ошибка при загрузке типов из сборки:\n{errors}", "Ошибка");
                }
            }
        }

        //добавление гуи связанного с плагином
        public void AddPluginLabel(string text, int topPadding = 10, int labelHeight = 30)
        {
            Label label = new Label();
            label.Text = text;
            label.TextAlign = ContentAlignment.MiddleCenter;
            label.Dock = DockStyle.Top;
            label.Height = labelHeight;
            label.Padding = new Padding(0, topPadding, 0, 0);
            label.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            label.ForeColor = DarkTheme.Text;
            label.BackColor = Color.Transparent;

            // Контекстное меню с единственным пунктом
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem removeItem = new ToolStripMenuItem("Забыть плагин");

            // Обработчик нажатия на "Удалить плагин"
            removeItem.Click += (sender, e) =>
            {
                pluginRemove(label.Text); // Передаём текст метки
            };

            contextMenu.Items.Add(removeItem);
            label.ContextMenuStrip = contextMenu;

            // При открытии меню — делаем текст жирным
            contextMenu.Opened += (sender, e) =>
            {
                label.Font = new Font(label.Font, FontStyle.Bold);
            };

            // При закрытии — возвращаем обычный стиль
            contextMenu.Closed += (sender, e) =>
            {
                label.Font = new Font(label.Font, FontStyle.Regular);
            };

            // Обработка правого клика (если хочешь принудительно показывать меню)
            label.MouseDown += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    label.ContextMenuStrip.Show(label, e.Location);
                }
            };

            // Spacer — внешний отступ
            Panel spacer = new Panel();
            spacer.Dock = DockStyle.Top;
            spacer.Height = labelHeight + topPadding;
            spacer.BackColor = DarkTheme.MainBg;

            this.Controls.Add(label);
            this.Controls.Add(spacer);
        }

        private void pluginRemove(string pluginInfo)
        {
            for (int i = 0; i < plugins.Count; i++)
            {
                if (plugins[i].GetInfo() == pluginInfo)
                {
                    string guid = plugins[i].GetGUID();

                    guids.Remove(guid);

                    plugins.RemoveAt(i);

                    pluginPaths.RemoveAt(i);

                    updatePluginsLabels();
                    updatePluginsButtons();
                    return;
                }
            }
        }


        private void updatePluginsLabels()
        {
            
            foreach (Control label in this.Controls.OfType<Label>().ToList())
            {
                MainForm.Instance.pluginsPanel.Controls.Remove(label);
                label.Dispose(); 
            }
            

            foreach (IPlugin plugin in plugins)
            {
                AddPluginLabel(plugin.GetInfo());
            }
        }

        private void updatePluginsButtons()
        {            
            foreach (Control button in MainForm.Instance.pluginsPanel.Controls.OfType<Button>().ToList())
            {
                MainForm.Instance.pluginsPanel.Controls.Remove(button);
                button.Dispose(); 
            }

            foreach (IImageEditing plugin in plugins)
            {
                AddPluginImgEditingButton(plugin);
            }
        }

        public void AddPluginImgEditingButton(IImageEditing plugin)
        {
            Button myButton = new Button();

            myButton.Text = plugin.GetGUIinfo();
            myButton.Dock = DockStyle.Top;
            myButton.Height = 40;
            DarkTheme.StyleButton(myButton);

            myButton.Click += (s, e) => {
                var selectedItems = MainForm.Instance.CurrentListView.SelectedItems;
                if (selectedItems.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы одно изображение.");
                    return;
                }
                try
                {
                    string settingsString = plugin.SetSettings();

                    if (selectedItems.Count > 1)
                    {
                        processManyImages(selectedItems, plugin, settingsString);
                        MainForm.Instance.updateListView();
                    }

                    else if (selectedItems.Count == 1)
                    {
                        Image prevImage = MainForm.Instance.CurrentPictureBox.Image;
                        Image newImage = plugin.ProcessImage(prevImage, settingsString);
                        MainForm.Instance.CurrentPictureBox.Image = newImage;

                        string imageKey = MainForm.Instance.CurrentListView.SelectedItems[0].ImageKey;
                        if (int.TryParse(imageKey, out int imgId))
                        {
                            MainForm.Instance.commitNewVersion(newImage, imgId, plugin.GetGUIinfo());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при обработке изображения плагином: " + ex.Message);
                }
                

            };

            MainForm.Instance.pluginsPanel.Controls.Add(myButton);
        }

        #region manyImages(4Lab)

        // для многопоточности
        BlockingCollection<string> _sqlLogQueue = new BlockingCollection<string>();
        int idleIntervalMs = 2000; 
        bool _loggingActive = true;
        Thread _loggerThread;


        public int PromptThreadCount()
        {
            using (var form = new PromptThreadForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return form.ThreadCount;
                }
                else
                {
                    throw new OperationCanceledException("Ввод количества потоков был отменён.");
                }
            }
        }

        public void ClearThreadsTable(string dbPath)
        {
            using (var connection = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                connection.Open();

                using (var command = new SQLiteCommand("DELETE FROM Threads;", connection))
                {
                    command.ExecuteNonQuery();
                }

                // Опционально: освободить пространство и сбросить автоинкремент
                using (var vacuumCommand = new SQLiteCommand("VACUUM;", connection))
                {
                    vacuumCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }
        
        public void StartLoggerThread(string dbPath)
        {
            ClearThreadsTable(dbPath);
            _loggerThread = new Thread(() =>
            {
                string connStr = $"Data Source={dbPath};Version=3;Pooling=True;Max Pool Size=100;";
                using (var connection = new SQLiteConnection(connStr))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка подключения к базе: " + ex.Message);
                        return;
                    }

                    while (_loggingActive)
                    {
                        if (_sqlLogQueue.TryTake(out var sql, TimeSpan.FromMilliseconds(idleIntervalMs)))
                        {
                            using (var command = new SQLiteCommand(sql, connection))
                            {
                                try
                                {
                                    command.ExecuteNonQuery();
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Ошибка при логировании в БД: " + ex.Message);
                                }
                            }
                        }
                        else
                        {
                            // очередь пуста — логгер "дремлет"
                        }
                    }
                }
            })
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };

            _loggerThread.Start();
        }

        public byte[] LoadImageBytesFromDisk(int imgId)
        {
            string connectionString = "Data Source=DBpicsell.db;Version=3;";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // Сначала находим текущую версию для изображения
                string getVersionIdSql = "SELECT CurrentVersID FROM Images WHERE ImgID = @imgId";
                int currentVersId;

                using (var cmd = new SQLiteCommand(getVersionIdSql, connection))
                {
                    cmd.Parameters.AddWithValue("@imgId", imgId);
                    var result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        throw new Exception($"Не найдена текущая версия для изображения ImgID={imgId}");

                    currentVersId = Convert.ToInt32(result);
                }

                // Теперь получаем путь к файлу (VersionPath)
                string getPathSql = "SELECT VersionPath FROM Versions WHERE VersID = @versId";

                using (var cmd = new SQLiteCommand(getPathSql, connection))
                {
                    cmd.Parameters.AddWithValue("@versId", currentVersId);
                    var pathResult = cmd.ExecuteScalar();

                    if (pathResult == null || pathResult == DBNull.Value)
                        throw new Exception($"Не найден путь для VersID={currentVersId}");

                    string imagePath = pathResult.ToString();

                    if (!File.Exists(imagePath))
                        throw new FileNotFoundException($"Файл по пути {imagePath} не найден.");

                    // Читаем файл как массив байтов
                    return File.ReadAllBytes(imagePath);
                }
            }
        }

        // удалить вероятнее всего он уже бесполезный т к есть коммит
        public void SaveNewImageVersion(int imgID, Image image)
        {
            string connStr = "Data Source=DBpicsell.db;Version=3;";
            string versionsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "versions");
            Directory.CreateDirectory(versionsDir); // Убедимся, что папка существует

            using (var connection = new SQLiteConnection(connStr))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int prevVersId;
                        using (var cmd = new SQLiteCommand("SELECT CurrentVersID FROM Images WHERE ImgID = @imgID", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@imgID", imgID);
                            var resultVers = cmd.ExecuteScalar();
                            if (resultVers == null || resultVers == DBNull.Value)
                                throw new Exception($"Текущая версия для ImgID={imgID} не найдена.");

                            prevVersId = Convert.ToInt32(resultVers);
                        }

                        int origVersId;
                        using (var cmd = new SQLiteCommand("SELECT OrigVersID FROM Versions WHERE VersID = @versID", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@versID", prevVersId);
                            var resultOrig = cmd.ExecuteScalar();
                            origVersId = Convert.ToInt32(resultOrig);
                        }

                        // Сохраняем изображение в файл
                        string fileName = $"img_{imgID}_v_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
                        string fullPath = Path.Combine(versionsDir, fileName);

                        image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Png);

                        // Вставляем новую версию с путём
                        int newVersId;
                        using (var cmd = new SQLiteCommand(@"
                    INSERT INTO Versions (OrigVersID, PrevVersID, VersionPath)
                    VALUES (@orig, @prev, @path);
                    SELECT last_insert_rowid();", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@orig", origVersId);
                            cmd.Parameters.AddWithValue("@prev", prevVersId);
                            cmd.Parameters.AddWithValue("@path", fullPath);
                            newVersId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Обновляем NextVersID у предыдущей версии
                        using (var cmd = new SQLiteCommand("UPDATE Versions SET NextVersID = @next WHERE VersID = @prev", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@next", newVersId);
                            cmd.Parameters.AddWithValue("@prev", prevVersId);
                            cmd.ExecuteNonQuery();
                        }

                        // Обновляем текущую версию изображения
                        using (var cmd = new SQLiteCommand("UPDATE Images SET CurrentVersID = @newVersID WHERE ImgID = @imgID", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@newVersID", newVersId);
                            cmd.Parameters.AddWithValue("@imgID", imgID);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Ошибка при сохранении новой версии: " + ex.Message);
                    }
                }
            }
        }


        public void processManyImages(ListView.SelectedListViewItemCollection selectedItems, IImageEditing plugin, string settings=null)
        {
            int threadCount = 2;
            if (selectedItems.Count < 4)
            {
                threadCount=selectedItems.Count;
            }
            else
            {
                threadCount = 4;
            }

            _loggingActive = true;
            _sqlLogQueue = new BlockingCollection<string>();
            StartLoggerThread("logs.db");

            Queue<(int imgID, byte[] blob)> taskQueue = new Queue<(int, byte[])>();

            foreach (ListViewItem item in selectedItems)
            {
                if (int.TryParse(item.ImageKey, out int imgId))
                {
                    byte[] blob = LoadImageBytesFromDisk(imgId);
                    taskQueue.Enqueue((imgId, blob));
                }
                else
                {
                    MessageBox.Show($"Ошибка: ImageKey \"{item.ImageKey}\" не является числом.");
                }
            }

            object _lockObj = new object();
            List<Thread> workers = new List<Thread>();

            for (int t = 0; t < threadCount; t++)
            {
                int localT = t;
                Thread worker = new Thread(() =>
                {
                    while (true)
                    {
                        (int imgID, byte[] blob) task;
                        lock (_lockObj)
                        {
                            if (taskQueue.Count == 0) break;
                            task = taskQueue.Dequeue();
                        }

                        try
                        {
                            using (var ms = new MemoryStream(task.blob))
                            using (var originalBmp = new Bitmap(ms))
                            {
                                Image inputImage = (Image)originalBmp;

                                var start = DateTime.Now;
                                var result = plugin.ProcessImage(originalBmp, settings);
                                var end = DateTime.Now;
                                int duration = (int)(end - start).TotalMilliseconds;

                                // с коммитом сейчас не работает
                                //SaveNewImageVersion(task.imgID, result);
                                MainForm.Instance.commitNewVersion(result, task.imgID, plugin.GetGUIinfo(), "many");

                                string log = $"INSERT INTO Threads (threadNum, photoID, time) VALUES ({localT + 1}, {task.imgID}, {duration});";
                                _sqlLogQueue.Add(log);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при обработке ImgID={task.imgID}: {ex.Message}");
                        }
                    }
                });
                worker.Start();
                workers.Add(worker);
            }

            foreach (var thread in workers)
                thread.Join();

            _loggingActive = false;
            _loggerThread.Join();

            MessageBox.Show("Обработка завершена!");
            MainForm.Instance.updateListView();
        }

        #endregion

    }
}
