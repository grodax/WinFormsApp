using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            DatabaseHelper.InitializeDatabase();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProductsForm form = new ProductsForm();
            form.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CounterpartiesForm form = new CounterpartiesForm();
            form.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DocumentsForm form = new DocumentsForm();
            form.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            StocksForm form = new StocksForm();
            form.ShowDialog();
        }
    }
}
