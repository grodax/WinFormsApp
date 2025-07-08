using System;
using System.Windows.Forms;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        DatabaseHelper.InitializeDatabase();
    }

    private void btnProducts_Click(object sender, EventArgs e)
    {
        ProductsForm form = new ProductsForm();
        form.ShowDialog();
    }

    private void btnCounterparties_Click(object sender, EventArgs e)
    {
        CounterpartiesForm form = new CounterpartiesForm();
        form.ShowDialog();
    }

    private void btnDocuments_Click(object sender, EventArgs e)
    {
        DocumentsForm form = new DocumentsForm();
        form.ShowDialog();
    }

    private void btnStocks_Click(object sender, EventArgs e)
    {
        StocksForm form = new StocksForm();
        form.ShowDialog();
    }
}
