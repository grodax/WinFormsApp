using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    internal class DocumentsHandler
    {
        public (bool Success, string Message) CancelWriteOffDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string docType = GetDocumentType(connection, transaction, documentId);
                        string docStatus = GetDocumentStatus(connection, transaction, documentId);

                        if (docType != "Расход")
                            return (false, "Ошибка: Документ не является расходным");

                        if (docStatus != "Списано")
                            return (false, "Ошибка: Можно отменять только списанные документы");

                        // 2. Получаем все позиции документа
                        var items = GetDocumentItems(connection, transaction, documentId);

                        // 3. Возвращаем товары на склад
                        foreach (var item in items)
                        {
                            ReturnItemsToStock(connection, transaction, item);
                        }

                        // 4. Обновляем статус документа
                        UpdateDocumentStatus(connection, transaction, documentId, "Черновик");

                        transaction.Commit();
                        return (true, "Списание успешно отменено, товары возвращены на склад");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Ошибка отмены списания: {ex.Message}");
                    }
                }
            }
        }

        private void ReturnItemsToStock(SQLiteConnection connection, SQLiteTransaction transaction, DocumentItem item)
        {
            string sql = @"
    UPDATE ProductStocks 
    SET ActualQuantity = ActualQuantity + @quantity
    WHERE ProductID = @productId";

            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                cmd.ExecuteNonQuery();
            }
        }

        public (bool Success, string Message) ProcessOutcomeDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string docType = GetDocumentType(connection, transaction, documentId);
                        string docStatus = GetDocumentStatus(connection, transaction, documentId);

                        if (docType != "Расход")
                            return (false, "Ошибка: Документ не является расходным");

                        if (docStatus == "Списано")
                            return (false, "Ошибка: Документ уже списан");

                        // 2. Получаем все позиции документа
                        var items = GetDocumentItems(connection, transaction, documentId);

                        // 3. Проверяем возможность списания
                        foreach (var item in items)
                        {
                            if (!CanWriteOffItem(connection, transaction, item))
                            {
                                transaction.Rollback();
                                return (false, $"Недостаточно товара ID {item.ProductId} для списания");
                            }
                        }

                        // 4. Выполняем списание
                        foreach (var item in items)
                        {
                            WriteOffItem(connection, transaction, item);
                        }

                        // 5. Обновляем статус документа
                        UpdateDocumentStatus(connection, transaction, documentId, "Списано");

                        transaction.Commit();
                        return (true, "Товары успешно списаны");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Ошибка списания: {ex.Message}");
                    }
                }
            }
        }

        private bool CanWriteOffItem(SQLiteConnection connection, SQLiteTransaction transaction, DocumentItem item)
        {
            string sql = @"
    SELECT 
        ActualQuantity >= @qty,
        ReservedQuantity >= @reservedQty
    FROM ProductStocks 
    WHERE ProductID = @productId";

            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.Parameters.AddWithValue("@qty", item.Quantity);
                cmd.Parameters.AddWithValue("@reservedQty", item.ReservedQuantity);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        bool hasEnoughActual = reader.GetBoolean(0);
                        bool hasEnoughReserved = reader.GetBoolean(1);
                        return hasEnoughActual && hasEnoughReserved;
                    }
                }
            }
            return false;
        }

        private void WriteOffItem(SQLiteConnection connection, SQLiteTransaction transaction, DocumentItem item)
        {
            string sql = @"
    UPDATE ProductStocks 
    SET 
        ActualQuantity = ActualQuantity - @qty,
        ReservedQuantity = ReservedQuantity - @reservedQty
    WHERE ProductID = @productId";

            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.Parameters.AddWithValue("@qty", item.Quantity);
                cmd.Parameters.AddWithValue("@reservedQty", item.ReservedQuantity);
                cmd.ExecuteNonQuery();
            }
        }

        public string ProcessIncomeDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string docType = GetDocumentType(connection, transaction, documentId);
                        string docStatus = GetDocumentStatus(connection, transaction, documentId);

                        if (docType != "Приход")
                            return "Ошибка: Документ не является приходным";
                        if (docStatus == "Оприходовано")
                            return "Ошибка: Документ уже оприходован";

                        // 2. Обновление остатков
                        string updateStockSql = @"
                UPDATE ProductStocks 
                SET ActualQuantity = ActualQuantity + ds.Quantity
                FROM DocumentSpecifications ds
                WHERE ds.DocumentID = @documentId 
                AND ProductStocks.ProductID = ds.ProductID";

                        using (var cmd = new SQLiteCommand(updateStockSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@documentId", documentId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Обновление статуса (с правильным значением)
                        string updateStatusSql = @"
                UPDATE DocumentHeaders 
                SET DocumentStatus = 'Оприходовано'
                WHERE DocumentID = @documentId";

                        using (var cmd = new SQLiteCommand(updateStatusSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@documentId", documentId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Документ успешно оприходован";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return $"Ошибка: {ex.Message}";
                    }
                }
            }
        }
        public string ProcessCancelIncomeDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string docType = GetDocumentType(connection, transaction, documentId);
                        string docStatus = GetDocumentStatus(connection, transaction, documentId);

                        if (docType != "Приход")
                            return "Ошибка: Документ не является приходным";
                        if (docStatus != "Оприходовано")
                            return "Ошибка: Документ не оприходован";


                        // 2. Обновление остатков
                        string updateStockSql = @"
                UPDATE ProductStocks 
                SET ActualQuantity = ActualQuantity - ds.Quantity
                FROM DocumentSpecifications ds
                WHERE ds.DocumentID = @documentId 
                AND ProductStocks.ProductID = ds.ProductID";

                        using (var cmd = new SQLiteCommand(updateStockSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@documentId", documentId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Обновление статуса (с правильным значением)
                        string updateStatusSql = @"
                UPDATE DocumentHeaders 
                SET DocumentStatus = 'Черновик'
                WHERE DocumentID = @documentId";

                        using (var cmd = new SQLiteCommand(updateStatusSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@documentId", documentId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return "Приход успешно отменён";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return $"Ошибка: {ex.Message}";
                    }
                }
            }
        }
        public (bool Success, string Message) ProcessReservationDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Проверка типа документа
                        string docType = GetDocumentType(connection, transaction, documentId);
                        if (docType != "Резерв")
                        {
                            return (false, "Ошибка: Документ не является резервным");
                        }

                        // 2. Получаем все позиции документа
                        var items = GetDocumentItems(connection, transaction, documentId);

                        // 3. Обрабатываем каждую позицию
                        foreach (var item in items)
                        {
                            ProcessReservationItem(connection, transaction, documentId, item);
                        }

                        // 4. Обновляем статус документа
                        UpdateDocumentStatus(connection, transaction, documentId, "Зарезервировано");

                        transaction.Commit();
                        return (true, "Товары успешно зарезервированы");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Ошибка резервирования: {ex.Message}");
                    }
                }
            }
        }

        public (bool Success, string Message) ProcessCancelReservationDocument(int documentId)
        {
            using (var connection = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. Проверка типа документа
                        string docType = GetDocumentType(connection, transaction, documentId);
                        string docStatus = GetDocumentStatus(connection, transaction, documentId);
                        if (docType != "Резерв")
                        {
                            return (false, "Ошибка: Документ не является резервным");
                        }

                        if (docStatus != "Зарезервировано")
                        {
                            return (false, "Ошибка: Документ не зарезервирован");
                        }

                        // 2. Получаем все позиции документа
                        var items = GetDocumentItems(connection, transaction, documentId);

                        // 3. Обрабатываем каждую позицию
                        foreach (var item in items)
                        {
                            ProcessCancelReservationItem(connection, transaction, documentId, item);
                        }

                        // 4. Обновляем статус документа
                        UpdateDocumentStatus(connection, transaction, documentId, "Черновик");

                        transaction.Commit();
                        return (true, "Резервирование успешно отменено");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"Ошибка резервирования: {ex.Message}");
                    }
                }
            }
        }

        private string GetDocumentStatus(SQLiteConnection connection, SQLiteTransaction transaction, int documentId)
        {
            string sql = "SELECT DocumentStatus FROM DocumentHeaders WHERE DocumentID = @documentId";
            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@documentId", documentId);
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private string GetDocumentType(SQLiteConnection connection, SQLiteTransaction transaction, int documentId)
        {
            string sql = "SELECT DocumentType FROM DocumentHeaders WHERE DocumentID = @documentId";
            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@documentId", documentId);
                return cmd.ExecuteScalar()?.ToString();
            }
        }

        private List<DocumentItem> GetDocumentItems(SQLiteConnection connection, SQLiteTransaction transaction, int documentId)
        {
            var items = new List<DocumentItem>();
            string sql = @"SELECT ProductID, Quantity, ReservedQuantity 
                  FROM DocumentSpecifications 
                  WHERE DocumentID = @documentId";

            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@documentId", documentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new DocumentItem
                        {
                            ProductId = reader.GetInt32(0),
                            Quantity = reader.GetInt32(1),
                            ReservedQuantity = reader.GetInt32(2)
                        });
                    }
                }
            }
            return items;
        }

        private void ProcessReservationItem(SQLiteConnection connection, SQLiteTransaction transaction,
                                          int documentId, DocumentItem item)
        {
            // Вычисляем доступное для резервирования количество
            string availableSql = @"
    SELECT ActualQuantity - ReservedQuantity 
    FROM ProductStocks 
    WHERE ProductID = @productId";

            int available = 0;
            using (var cmd = new SQLiteCommand(availableSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                available = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            // Вычисляем сколько можем зарезервировать
            int toReserve = Math.Min(
                item.Quantity - item.ReservedQuantity, // Сколько нужно документу
                available                             // Сколько доступно на складе
            );

            if (toReserve <= 0) return;

            // 1. Обновляем остатки на складе
            string updateStockSql = @"
    UPDATE ProductStocks 
    SET ReservedQuantity = ReservedQuantity + @toReserve
    WHERE ProductID = @productId";

            using (var cmd = new SQLiteCommand(updateStockSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@toReserve", toReserve);
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.ExecuteNonQuery();
            }

            // 2. Обновляем резерв в документе
            string updateDocSql = @"
    UPDATE DocumentSpecifications 
    SET ReservedQuantity = ReservedQuantity + @toReserve
    WHERE DocumentID = @documentId AND ProductID = @productId";

            using (var cmd = new SQLiteCommand(updateDocSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@toReserve", toReserve);
                cmd.Parameters.AddWithValue("@documentId", documentId);
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.ExecuteNonQuery();
            }
        }

        private void ProcessCancelReservationItem(SQLiteConnection connection, SQLiteTransaction transaction,
                                          int documentId, DocumentItem item)
        {
            int toReserve = 0;

            // 1. Обновляем остатки на складе
            string updateStockSql = @"
    UPDATE ProductStocks 
    SET ReservedQuantity = @toReserve
    WHERE ProductID = @productId";

            using (var cmd = new SQLiteCommand(updateStockSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@toReserve", toReserve);
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.ExecuteNonQuery();
            }

            // 2. Обновляем резерв в документе
            string updateDocSql = @"
    UPDATE DocumentSpecifications 
    SET ReservedQuantity = @toReserve
    WHERE DocumentID = @documentId AND ProductID = @productId";

            using (var cmd = new SQLiteCommand(updateDocSql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@toReserve", toReserve);
                cmd.Parameters.AddWithValue("@documentId", documentId);
                cmd.Parameters.AddWithValue("@productId", item.ProductId);
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdateDocumentStatus(SQLiteConnection connection, SQLiteTransaction transaction,
                                        int documentId, string status)
        {
            string sql = "UPDATE DocumentHeaders SET DocumentStatus = @status WHERE DocumentID = @documentId";
            using (var cmd = new SQLiteCommand(sql, connection, transaction))
            {
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@documentId", documentId);
                cmd.ExecuteNonQuery();
            }
        }

        public class DocumentItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public int ReservedQuantity { get; set; }
        }
    }
}
