using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicSell
{
    public partial class StatisticForm : Form
    {
        public StatisticForm()
        {
            InitializeComponent();
            DarkTheme.Apply(this);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Text = "PicSell \u2014 \u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430";
            UpdateStatistics();
        }
        string connectionString = "Data Source=logs.db;Version=3;";
        private async Task<DataTable> GetStatisticsFromDatabase()
        {
            string query = "SELECT threadNum, COUNT(*) AS totalTasks, MIN(time) AS minTime, MAX(time) AS maxTime, AVG(time) AS avgTime " +
                           "FROM Threads GROUP BY threadNum";

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                var command = new SQLiteCommand(query, connection);
                var reader = await command.ExecuteReaderAsync();

                DataTable dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
        }

        public async void UpdateStatistics()
        {
            var dataTable = await GetStatisticsFromDatabase();
            dataGridView1.DataSource = dataTable;
        }

        private void StatisticForm_Load(object sender, EventArgs e)
        {

        }
    }
}
