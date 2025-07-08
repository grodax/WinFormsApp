using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class StocksForm : Form
    {
        private DataAccessSQLite dataAccess;

        public StocksForm()
        {
            InitializeComponent();
            dataAccess = new DataAccessSQLite();
            LoadStockData();
            LoadProducts();
        }

        // Загрузка данных об остатках
        private void LoadStockData()
        {
            try
            {
                string sql = @"SELECT ps.StockID, p.ProductName, 
                         ps.ActualQuantity, ps.ReservedQuantity,
                         (ps.ActualQuantity - ps.ReservedQuantity) as Available
                         FROM ProductStocks ps
                         JOIN Products p ON ps.ProductID = p.ProductID
                         ORDER BY p.ProductName";

                dataGridView1.DataSource = dataAccess.ExecuteQuery(sql);
                CalculateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки остатков: {ex.Message}");
            }
        }

        // Загрузка списка товаров в ComboBox
        private void LoadProducts()
        {
            try
            {
                string sql = "SELECT ProductID, ProductName FROM Products ORDER BY ProductName";
                comboBox1.DataSource = dataAccess.ExecuteQuery(sql);
                comboBox1.DisplayMember = "ProductName";
                comboBox1.ValueMember = "ProductID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        // Расчет итоговых значений
        private void CalculateTotals()
        {
            int totalActual = 0;
            int totalReserved = 0;
            int totalAvailable = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    totalActual += Convert.ToInt32(row.Cells["ActualQuantity"].Value);
                    totalReserved += Convert.ToInt32(row.Cells["ReservedQuantity"].Value);
                    totalAvailable += Convert.ToInt32(row.Cells["Available"].Value);
                }
            }
        }

        // Проведение инвентаризации (корректировка остатков)
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(textBox1.Text, out int actualQty) || actualQty < 0)
            {
                MessageBox.Show("Введите корректное фактическое количество");
                return;
            }

            int productId = Convert.ToInt32(comboBox1.SelectedValue);

            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql = @"UPDATE ProductStocks 
                                 SET ActualQuantity = @ActualQty
                                 WHERE ProductID = @ProductID";

                        SQLiteParameter[] parameters = {
                        new SQLiteParameter("@ActualQty", actualQty),
                        new SQLiteParameter("@ProductID", productId)
                    };

                        using (var cmd = new SQLiteCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddRange(parameters);
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                transaction.Commit();
                                MessageBox.Show("Остатки обновлены");
                                LoadStockData();
                                ClearFields();
                            }
                            else
                            {
                                MessageBox.Show("Товар не найден в остатках");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка инвентаризации: {ex.Message}");
                    }
                }
            }
        }

        // Добавление товара на склад (приход)
        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(textBox1.Text, out int addQty) || addQty <= 0)
            {
                MessageBox.Show("Введите корректное количество для прихода");
                return;
            }

            int productId = Convert.ToInt32(comboBox1.SelectedValue);

            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Используем INSERT OR REPLACE для обновления существующих записей
                        string sql = @"INSERT OR REPLACE INTO ProductStocks 
                                 (ProductID, ActualQuantity, ReservedQuantity)
                                 VALUES (
                                     @ProductID, 
                                     COALESCE((SELECT ActualQuantity FROM ProductStocks WHERE ProductID = @ProductID), 0) + @AddQty,
                                     COALESCE((SELECT ReservedQuantity FROM ProductStocks WHERE ProductID = @ProductID), 0)
                                 )";

                        SQLiteParameter[] parameters = {
                        new SQLiteParameter("@ProductID", productId),
                        new SQLiteParameter("@AddQty", addQty)
                    };

                        using (var cmd = new SQLiteCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddRange(parameters);
                            int result = cmd.ExecuteNonQuery();

                            transaction.Commit();
                            MessageBox.Show("Товар оприходован");
                            LoadStockData();
                            ClearFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при оприходовании: {ex.Message}");
                    }
                }
            }
        }

        // Списание товара со склада
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(textBox1.Text, out int removeQty) || removeQty <= 0)
            {
                MessageBox.Show("Введите корректное количество для списания");
                return;
            }

            int productId = Convert.ToInt32(comboBox1.SelectedValue);

            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Проверяем доступное количество
                        string checkSql = @"SELECT (ActualQuantity - ReservedQuantity) >= @RemoveQty 
                                      FROM ProductStocks 
                                      WHERE ProductID = @ProductID";

                        using (var checkCmd = new SQLiteCommand(checkSql, connection, transaction))
                        {
                            checkCmd.Parameters.Add(new SQLiteParameter("@RemoveQty", removeQty));
                            checkCmd.Parameters.Add(new SQLiteParameter("@ProductID", productId));

                            bool canRemove = Convert.ToBoolean(checkCmd.ExecuteScalar());

                            if (!canRemove)
                            {
                                MessageBox.Show("Недостаточно товара для списания");
                                return;
                            }
                        }

                        // 2. Выполняем списание
                        string updateSql = @"UPDATE ProductStocks 
                                       SET ActualQuantity = ActualQuantity - @RemoveQty
                                       WHERE ProductID = @ProductID";

                        using (var updateCmd = new SQLiteCommand(updateSql, connection, transaction))
                        {
                            updateCmd.Parameters.Add(new SQLiteParameter("@RemoveQty", removeQty));
                            updateCmd.Parameters.Add(new SQLiteParameter("@ProductID", productId));

                            int result = updateCmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                transaction.Commit();
                                MessageBox.Show("Товар списан");
                                LoadStockData();
                                ClearFields();
                            }
                            else
                            {
                                MessageBox.Show("Товар не найден в остатках");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка при списании: {ex.Message}");
                    }
                }
            }
        }

        // Очистка полей
        private void ClearFields()
        {
            textBox1.Text = "";
            comboBox1.SelectedIndex = -1;
        }

        // Обновление данных при выборе товара
        private void comboBoxProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, что есть выбранный элемент и он не null
                if (comboBox1.SelectedItem != null && comboBox1.SelectedValue != null)
                {
                    // Безопасное получение ProductID
                    object selectedValue = comboBox1.SelectedValue;

                    int productId;
                    if (selectedValue is DataRowView rowView)
                    {
                        // Если привязан DataTable, получаем значение через DataRowView
                        productId = Convert.ToInt32(rowView["ProductID"]);
                    }
                    else
                    {
                        // Прямое преобразование для простых случаев
                        productId = Convert.ToInt32(selectedValue);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе товара: {ex.Message}");
                Debug.WriteLine($"Ошибка в comboBoxProducts_SelectedIndexChanged: {ex}");
            }
        }

        // Обновление данных при изменении в таблице
        private void dataGridViewStock_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                CalculateTotals();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
