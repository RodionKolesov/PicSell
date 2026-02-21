using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicSell
{
    public partial class PromptThreadForm : Form
    {
        public int ThreadCount { get; private set; }

        public PromptThreadForm()
        {
            InitializeComponent();
            DarkTheme.Apply(this);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.Text = "PicSell \u2014 \u041F\u043E\u0442\u043E\u043A\u0438";
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(threadsNumTextBox.Text, out int count) && count > 0)
            {
                ThreadCount = count;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректное положительное целое число.", "Ошибка ввода",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                threadsNumTextBox.Focus();
            }
        }
    }
}
