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
using WinFormsApp1.Models;

namespace WinFormsApp1
{
    public partial class DocumentsForm : Form
    {
        private DataAccessSQLite dataAccess;
        private DocumentsHandler documentsHandler;

        public DocumentsForm()
        {
            InitializeComponent();
            dataAccess = new DataAccessSQLite();
            LoadDocuments();
            LoadCounterparties();
            LoadProducts();
            documentsHandler = new DocumentsHandler();
        }

        // Загрузка списка документов
        private void LoadDocuments()
        {
            try
            {
                string sql = @"SELECT d.DocumentID, d.DocumentNumber, d.DocumentType, d.DocumentStatus,
                          c.Name as CounterpartyName, d.DocumentDate, d.TotalSum
                          FROM DocumentHeaders d
                          JOIN Counterparties c ON d.CounterpartyID = c.CounterpartyID
                          ORDER BY d.DocumentDate DESC";

                dataGridView1.DataSource = dataAccess.ExecuteQuery(sql);

                comboBox3.DataSource = Enum.GetValues(typeof(DocumentType));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки документов: {ex.Message}");
            }
        }

        private void LoadStockData()
        {
            string sql = @"SELECT p.ProductName, ps.ActualQuantity, ps.ReservedQuantity
                  FROM ProductStocks ps
                  JOIN Products p ON ps.ProductID = p.ProductID";

            dataGridView1.DataSource = dataAccess.ExecuteQuery(sql);
        }

        // Загрузка списка контрагентов в ComboBox
        private void LoadCounterparties()
        {
            try
            {
                string sql = "SELECT CounterpartyID, Name FROM Counterparties ORDER BY Name";
                comboBox1.DataSource = dataAccess.ExecuteQuery(sql);
                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "CounterpartyID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки контрагентов: {ex.Message}");
            }
        }

        // Загрузка списка товаров в ComboBox
        private void LoadProducts()
        {
            try
            {
                string sql = "SELECT ProductID, ProductName FROM Products ORDER BY ProductName";
                comboBox2.DataSource = dataAccess.ExecuteQuery(sql);
                comboBox2.DisplayMember = "ProductName";
                comboBox2.ValueMember = "ProductID";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }

        // Создание нового документа
        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null)
            {
                MessageBox.Show("Выберите контрагента");
                return;
            }

            if (comboBox3.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип документа");
                return;
            }

            int counterpartyId = Convert.ToInt32(comboBox1.SelectedValue);
            string docNumber = textBox1.Text.Trim();
            string docType = Convert.ToString((DocumentType)comboBox3.SelectedItem);
            DateTime docDate = dateTimePicker1.Value;

            if (string.IsNullOrEmpty(docNumber))
            {
                MessageBox.Show("Введите номер документа");
                return;
            }

            string sql = @"INSERT INTO DocumentHeaders 
                      (DocumentNumber, CounterpartyID, DocumentDate, TotalSum, DocumentType, DocumentStatus) 
                      VALUES (@Number, @CounterpartyID, @Date, 0, @Type, 'Черновик')";

            SQLiteParameter[] parameters = {
            new SQLiteParameter("@Number", docNumber),
            new SQLiteParameter("@CounterpartyID", counterpartyId),
            new SQLiteParameter("@Date", docDate),
            new SQLiteParameter("@Type", docType)
        };

            try
            {
                int result = dataAccess.ExecuteNonQuery(sql, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Документ создан");
                    LoadDocuments();
                    ClearDocumentFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания документа: {ex.Message}");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            string result = documentsHandler.ProcessIncomeDocument(documentId);
            MessageBox.Show(result);

            // Обновляем данные
            LoadDocuments();
            LoadDocumentSpecifications(documentId);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            string result = documentsHandler.ProcessCancelIncomeDocument(documentId);
            MessageBox.Show(result);

            // Обновляем данные
            LoadDocuments();
            LoadDocumentSpecifications(documentId);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            var (success, message) = documentsHandler.ProcessReservationDocument(documentId);
            MessageBox.Show(message);

            if (success)
            {
                // Обновляем данные в интерфейсе
                LoadDocuments();
                LoadDocumentSpecifications(documentId);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            var (success, message) = documentsHandler.ProcessCancelReservationDocument(documentId);
            MessageBox.Show(message);

            if (success)
            {
                // Обновляем данные в интерфейсе
                LoadDocuments();
                LoadDocumentSpecifications(documentId);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            var (success, message) = documentsHandler.ProcessOutcomeDocument(documentId);
            MessageBox.Show(message);

            if (success)
            {
                // Обновляем данные в интерфейсе
                LoadDocuments();
                LoadDocumentSpecifications(documentId);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            var result = MessageBox.Show("Вы уверены, что хотите отменить списание этого документа?",
                                       "Подтверждение отмены",
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var (success, message) = documentsHandler.CancelWriteOffDocument(documentId);
                MessageBox.Show(message);

                if (success)
                {
                    // Обновляем данные в интерфейсе
                    LoadDocuments();
                    LoadDocumentSpecifications(documentId);
                }
            }
        }

        // Добавление товара в документ
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите документ");
                return;
            }

            if (comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            if (!int.TryParse(textBox2.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                return;
            }

            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);
            int productId = Convert.ToInt32(comboBox2.SelectedValue);
            decimal price = GetProductPrice(productId);
            decimal discount = GetDiscount();

            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Добавляем позицию в спецификацию
                        string sql = @"INSERT INTO DocumentSpecifications 
                                  (DocumentID, ProductID, Quantity, ReservedQuantity, Price, Discount)
                                  VALUES (@DocID, @ProdID, @Qty, 0, @Price, @Discount)";

                        SQLiteParameter[] parameters = {
                        new SQLiteParameter("@DocID", documentId),
                        new SQLiteParameter("@ProdID", productId),
                        new SQLiteParameter("@Qty", quantity),
                        new SQLiteParameter("@Price", price),
                        new SQLiteParameter("@Discount", discount)
                    };

                        using (var cmd = new SQLiteCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddRange(parameters);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Обновляем сумму документа
                        UpdateDocumentTotal(documentId, connection, transaction);

                        transaction.Commit();
                        MessageBox.Show("Товар добавлен в документ");
                        LoadDocumentSpecifications(documentId);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка добавления товара: {ex.Message}");
                    }
                }
            }
        }

        // Обновление общей суммы документа
        private void UpdateDocumentTotal(int documentId, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            string sql = @"UPDATE DocumentHeaders 
                  SET TotalSum = (
                      SELECT COALESCE(SUM(Quantity * Price * (1 - Discount/100)), 0)
                      FROM DocumentSpecifications
                      WHERE DocumentID = @DocID
                  )
                  WHERE DocumentID = @DocID";

            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@DocID", documentId);
                cmd.ExecuteNonQuery();
            }
        }

        // Получение цены товара
        private decimal GetProductPrice(int productId)
        {
            string sql = "SELECT Price FROM Products WHERE ProductID = @ProdID";
            object result = dataAccess.ExecuteScalar(sql,
                new SQLiteParameter[] { new SQLiteParameter("@ProdID", productId) });

            return Convert.ToDecimal(result);
        }


        private decimal GetDiscount()
        {
            if (decimal.TryParse(textBox3.Text, out decimal discount))
                return Math.Clamp(discount, 0, 100);
            return 0;
        }

        // Загрузка спецификации документа
        private void LoadDocumentSpecifications(int documentId)
        {
            try
            {
                string sql = @"SELECT s.SpecificationID, p.ProductName, 
                          s.Quantity, s.Price, s.Discount, s.ReservedQuantity,
                          s.Quantity * s.Price * (1 - s.Discount/100) as Sum
                          FROM DocumentSpecifications s
                          JOIN Products p ON s.ProductID = p.ProductID
                          WHERE s.DocumentID = @DocID";

                SQLiteParameter[] parameters = {
                new SQLiteParameter("@DocID", documentId)
            };

                dataGridView2.DataSource = dataAccess.ExecuteQuery(sql, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки спецификации: {ex.Message}");
            }
        }

        // Очистка полей документа
        private void ClearDocumentFields()
        {
            textBox1.Text = "";
            dateTimePicker1.Value = DateTime.Now;
            comboBox1.SelectedIndex = -1;
        }

        // Обработчик выбора документа
        private void dataGridViewDocuments_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 && dataGridView1.SelectedRows[0].DataBoundItem != null)
            {
                int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);
                LoadDocumentSpecifications(documentId);

                var row = dataGridView1.SelectedRows[0];
                string docType = row.Cells["DocumentType"].Value.ToString();
                string docStatus = row.Cells["DocumentStatus"].Value.ToString();

                button4.Enabled = (docType == "Приход" && docStatus != "Оприходовано");
                button5.Enabled = (docType == "Приход" && docStatus == "Оприходовано");
                button6.Enabled = (docType == "Резерв" && docStatus != "Зарезервировано");
                button7.Enabled = (docType == "Резерв" && docStatus == "Зарезервировано");
                button8.Enabled = (docType == "Расход" && docStatus != "Списано");
                button9.Enabled = (docType == "Расход" && docStatus == "Списано");
            }
            else
            {
                button4.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
            }
        }

        // Удаление позиции из спецификации
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите позицию для удаления");
                return;
            }

            int specId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["SpecificationID"].Value);
            int documentId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DocumentID"].Value);

            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Удаляем позицию
                        string deleteSql = "DELETE FROM DocumentSpecifications WHERE SpecificationID = @SpecID";
                        using (var cmd = new SQLiteCommand(deleteSql, connection, transaction))
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@SpecID", specId));
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Обновляем сумму документа
                        UpdateDocumentTotal(documentId, connection, transaction);

                        transaction.Commit();
                        MessageBox.Show("Позиция удалена");
                        LoadDocumentSpecifications(documentId);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }
    }
}
