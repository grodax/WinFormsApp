using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WinFormsApp1
{
    public partial class CounterpartiesForm : Form
    {
        private DataAccessSQLite dataAccess;
        

        public CounterpartiesForm()
        {
            InitializeComponent();
            dataAccess = new DataAccessSQLite();
            LoadCounterparties();
        }

        // Загрузка данных в DataGridView
        private void LoadCounterparties()
        {
            try
            {
                string sql = "SELECT CounterpartyID, Name, Address, Phone, Email FROM Counterparties";
                DataTable dt = dataAccess.ExecuteQuery(sql);
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки контрагентов: {ex.Message}");
            }
        }

        // Добавление контрагента
        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите наименование контрагента");
                return;
            }

            string sql = @"INSERT INTO Counterparties (Name, Address, Phone, Email) 
                      VALUES (@Name, @Address, @Phone, @Email)";

            SQLiteParameter[] parameters = {
            new SQLiteParameter("@Name", name),
            new SQLiteParameter("@Address", textBox2.Text.Trim()),
            new SQLiteParameter("@Phone", textBox3.Text.Trim()),
            new SQLiteParameter("@Email", textBox4.Text.Trim())
        };

            try
            {
                int result = dataAccess.ExecuteNonQuery(sql, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Контрагент успешно добавлен");
                    LoadCounterparties();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}");
            }
        }

        // Обновление контрагента
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите контрагента для обновления");
                return;
            }

            var row = dataGridView1.SelectedRows[0];
            if (row.Cells["CounterpartyID"].Value == null)
            {
                MessageBox.Show("Не удалось получить ID контрагента");
                return;
            }

            int counterpartyId = Convert.ToInt32(row.Cells["CounterpartyID"].Value);
            string name = textBox1.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите наименование контрагента");
                return;
            }

            string sql = @"UPDATE Counterparties 
                      SET Name = @Name, 
                          Address = @Address,
                          Phone = @Phone,
                          Email = @Email
                      WHERE CounterpartyID = @CounterpartyID";

            SQLiteParameter[] parameters = {
            new SQLiteParameter("@Name", name),
            new SQLiteParameter("@Address", textBox2.Text.Trim()),
            new SQLiteParameter("@Phone", textBox3.Text.Trim()),
            new SQLiteParameter("@Email", textBox4.Text.Trim()),
            new SQLiteParameter("@CounterpartyID", counterpartyId)
        };

            try
            {
                int result = dataAccess.ExecuteNonQuery(sql, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Данные контрагента обновлены");
                    LoadCounterparties();
                }
                else
                {
                    MessageBox.Show("Контрагент не был обновлен");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}");
            }
        }

        // Удаление контрагента
        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите контрагента для удаления");
                return;
            }

            var row = dataGridView1.SelectedRows[0];
            if (row.Cells["CounterpartyID"].Value == null)
            {
                MessageBox.Show("Не удалось получить ID контрагента");
                return;
            }

            int counterpartyId = Convert.ToInt32(row.Cells["CounterpartyID"].Value);

            // Проверка на использование в документах
            string checkSql = "SELECT COUNT(*) FROM DocumentHeaders WHERE CounterpartyID = @CounterpartyID";
            SQLiteParameter[] checkParams = {
        new SQLiteParameter("@CounterpartyID", counterpartyId)
    };

            try
            {
                // Исправление: передаем массив параметров
                object result = dataAccess.ExecuteScalar(checkSql, checkParams);
                int documentsCount = Convert.ToInt32(result ?? 0);

                if (documentsCount > 0)
                {
                    MessageBox.Show("Невозможно удалить - контрагент используется в документах");
                    return;
                }

                string deleteSql = "DELETE FROM Counterparties WHERE CounterpartyID = @CounterpartyID";
                SQLiteParameter[] deleteParams = {
            new SQLiteParameter("@CounterpartyID", counterpartyId)
        };

                int rowsAffected = dataAccess.ExecuteNonQuery(deleteSql, deleteParams);

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Контрагент удален");
                    LoadCounterparties();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Контрагент не был удален");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        // Очистка полей
        private void ClearFields()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
        }

        // Заполнение полей при выборе строки
        private void dataGridViewCounterparties_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var row = dataGridView1.SelectedRows[0];
                textBox1.Text = row.Cells["Name"].Value?.ToString() ?? "";
                textBox2.Text = row.Cells["Address"].Value?.ToString() ?? "";
                textBox3.Text = row.Cells["Phone"].Value?.ToString() ?? "";
                textBox4.Text = row.Cells["Email"].Value?.ToString() ?? "";
            }
        }
    }
}
