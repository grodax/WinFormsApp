using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class ProductsForm : Form
    {
        private DataAccessSQLite dataAccess;
        public ProductsForm()
        {
            InitializeComponent();
            dataAccess = new DataAccessSQLite();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                string sql = "SELECT ProductID, ProductName, Price FROM Products";
                DataTable dt = dataAccess.ExecuteQuery(sql);
                dataGridView1.DataSource = dt;

                // Убедитесь, что столбцы существуют
                if (!dataGridView1.Columns.Contains("ProductID") ||
                    !dataGridView1.Columns.Contains("ProductName") ||
                    !dataGridView1.Columns.Contains("Price"))
                {
                    MessageBox.Show("Ошибка: не все колонки существуют в DataGridView");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string productName = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(productName))
            {
                MessageBox.Show("Введите наименование товара");
                return;
            }

            if (!decimal.TryParse(textBox2.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену");
                return;
            }

            // Используем транзакцию для обеих операций
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Добавляем товар в таблицу Products
                        string insertProductSql = "INSERT INTO Products (ProductName, Price) VALUES (@ProductName, @Price)";
                        SQLiteParameter[] productParams = {
                    new SQLiteParameter("@ProductName", productName),
                    new SQLiteParameter("@Price", price)
                };

                        using (var cmd = new SQLiteCommand(insertProductSql, connection, transaction))
                        {
                            cmd.Parameters.AddRange(productParams);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Получаем ID только что добавленного товара
                        long productId = connection.LastInsertRowId;

                        // 3. Добавляем запись в ProductStocks (используем INSERT OR IGNORE)
                        string stockSql = @"INSERT OR IGNORE INTO ProductStocks 
                                  (ProductID, ActualQuantity, ReservedQuantity) 
                                  VALUES (@ProductID, 0, 0)";

                        using (var cmd = new SQLiteCommand(stockSql, connection, transaction))
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@ProductID", productId));
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Товар успешно добавлен");
                        LoadProducts();
                        ClearFields();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при добавлении товара: {ex.Message}");
                    }
                }
            }
        }

        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
        }

        // Улучшенный метод удаления товара
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите товар для удаления");
                    return;
                }

                // Безопасное получение ProductID
                var selectedRow = dataGridView1.SelectedRows[0];
                if (selectedRow.Cells["ProductID"].Value == null)
                {
                    MessageBox.Show("Не удалось получить ID товара");
                    return;
                }

                int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);

                // Проверка на использование товара
                string checkSql = "SELECT COUNT(*) FROM DocumentSpecifications WHERE ProductID = @ProductID";
                SQLiteParameter checkParam = new SQLiteParameter("@ProductID", productId);

                int usageCount = 0;
                try
                {
                    var result = dataAccess.ExecuteScalar(checkSql, new SQLiteParameter[] { checkParam });
                    usageCount = Convert.ToInt32(result ?? 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при проверке товара: {ex.Message}");
                    return;
                }

                if (usageCount > 0)
                {
                    MessageBox.Show("Невозможно удалить товар, так как он используется в документах");
                    return;
                }

                // Удаление в транзакции
                using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Удаляем из таблицы остатков
                            string deleteStockSql = "DELETE FROM ProductStocks WHERE ProductID = @ProductID";
                            using (var cmd = new SQLiteCommand(deleteStockSql, connection, transaction))
                            {
                                cmd.Parameters.Add(checkParam);
                                cmd.ExecuteNonQuery();
                            }

                            // Удаляем сам товар
                            string deleteSql = "DELETE FROM Products WHERE ProductID = @ProductID";
                            using (var cmd = new SQLiteCommand(deleteSql, connection, transaction))
                            {
                                cmd.Parameters.Add(checkParam);
                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    transaction.Commit();
                                    MessageBox.Show("Товар успешно удален");
                                    LoadProducts();
                                }
                                else
                                {
                                    MessageBox.Show("Товар не был удален. Возможно, он уже был удален.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка при удалении товара: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }

        // Улучшенный метод обновления товара
        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                if (dataGridView1.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Выберите товар для обновления");
                    return;
                }

                var selectedRow = dataGridView1.SelectedRows[0];

                if (selectedRow.Cells["ProductID"].Value == null)
                {
                    MessageBox.Show("Не удалось получить ID товара");
                    return;
                }

                int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
                string productName = textBox1.Text.Trim();

                if (string.IsNullOrEmpty(productName))
                {
                    MessageBox.Show("Введите наименование товара");
                    return;
                }

                if (!decimal.TryParse(textBox2.Text, out decimal price))
                {
                    MessageBox.Show("Введите корректную цену");
                    return;
                }

                // Обновление в транзакции
                using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string sql = "UPDATE Products SET ProductName = @ProductName, Price = @Price WHERE ProductID = @ProductID";

                            using (var cmd = new SQLiteCommand(sql, connection, transaction))
                            {
                                cmd.Parameters.Add(new SQLiteParameter("@ProductName", productName));
                                cmd.Parameters.Add(new SQLiteParameter("@Price", price));
                                cmd.Parameters.Add(new SQLiteParameter("@ProductID", productId));

                                int result = cmd.ExecuteNonQuery();

                                if (result > 0)
                                {
                                    transaction.Commit();
                                    MessageBox.Show("Товар успешно обновлен");
                                    LoadProducts();
                                    ClearFields();
                                }
                                else
                                {
                                    MessageBox.Show("Товар не был обновлен. Возможно, он был удален.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Ошибка при обновлении товара: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }

        // Улучшенный метод обработки выбора строки
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    var row = dataGridView1.SelectedRows[0];

                    // Безопасное обновление textBox1
                    textBox1.Text = row.Cells["ProductName"].Value?.ToString() ?? string.Empty;

                    // Безопасное обновление textBox2
                    if (row.Cells["Price"].Value != null && decimal.TryParse(row.Cells["Price"].Value.ToString(), out decimal price))
                    {
                        textBox2.Text = price.ToString();
                    }
                    else
                    {
                        textBox2.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}");
            }
        }
    }
}
