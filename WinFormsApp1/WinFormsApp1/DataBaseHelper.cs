﻿using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

public static class DatabaseHelper
{
    private static string connectionString = "Data Source=WarehouseDB.sqlite;Version=3;";

    public static string GetConnectionString()
    {
        return connectionString;
    }

    public static void InitializeDatabase()
    {
        if (!File.Exists("WarehouseDB.sqlite"))
        {
            SQLiteConnection.CreateFile("WarehouseDB.sqlite");

            using (var connection = GetConnection()) // Используем наш метод
            {
                connection.Open();

                // Создание таблиц
                string[] createTables = new string[]
                {
                    // 1. Справочник товаров
                    @"CREATE TABLE Products (
                        ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductName TEXT NOT NULL,
                        Price REAL NOT NULL)",
                        
                    // 2. Справочник контрагентов
                    @"CREATE TABLE Counterparties (
                        CounterpartyID INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Address TEXT,
                        Phone TEXT,
                        Email TEXT)",
                        
                    // 3. Шапки документов
                    @"CREATE TABLE IF NOT EXISTS DocumentHeaders (
                    DocumentID INTEGER PRIMARY KEY AUTOINCREMENT,
                    DocumentNumber TEXT NOT NULL,
                    CounterpartyID INTEGER NOT NULL,
                    DocumentDate TEXT NOT NULL,
                    TotalSum REAL NOT NULL DEFAULT 0,
                    DocumentType TEXT NOT NULL CHECK(DocumentType IN ('Приход', 'Резерв', 'Расход')),
                    DocumentStatus TEXT NOT NULL DEFAULT 'Черновик' 
                        CHECK(DocumentStatus IN ('Черновик', 'Зарезервировано', 'Списано', 'Оприходовано')),
                    FOREIGN KEY (CounterpartyID) REFERENCES Counterparties(CounterpartyID)
                    );",
                        
                    // 4. Спецификации документов
                    @"CREATE TABLE DocumentSpecifications (
                        SpecificationID INTEGER PRIMARY KEY AUTOINCREMENT,
                        DocumentID INTEGER NOT NULL,
                        ProductID INTEGER NOT NULL,
                        Quantity INTEGER NOT NULL,
                        ReservedQuantity INTEGER NOT NULL DEFAULT 0,
                        Price REAL NOT NULL,
                        Discount REAL NOT NULL DEFAULT 0,
                        FOREIGN KEY (DocumentID) REFERENCES DocumentHeaders(DocumentID),
                        FOREIGN KEY (ProductID) REFERENCES Products(ProductID))",
                        
                    // 5. Остатки товара на складе
                    @"CREATE TABLE ProductStocks (
                        StockID INTEGER PRIMARY KEY AUTOINCREMENT,
                        ProductID INTEGER NOT NULL UNIQUE,
                        ActualQuantity INTEGER NOT NULL,
                        ReservedQuantity INTEGER NOT NULL DEFAULT 0,
                        FOREIGN KEY (ProductID) REFERENCES Products(ProductID))"
                };

                foreach (var sql in createTables)
                {
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Триггер для обновления остатков при добавлении спецификации
                string triggerSql = @"
                CREATE TRIGGER IF NOT EXISTS UpdateStockAfterInsertSpec
                AFTER INSERT ON DocumentSpecifications
                BEGIN
                    INSERT OR REPLACE INTO ProductStocks (ProductID, ActualQuantity, ReservedQuantity)
                    VALUES (
                        NEW.ProductID,
                        COALESCE((SELECT ActualQuantity FROM ProductStocks WHERE ProductID = NEW.ProductID), 0),
                        COALESCE((SELECT ReservedQuantity FROM ProductStocks WHERE ProductID = NEW.ProductID), 0) + NEW.ReservedQuantity
                    );
                END;";

                using (var command = new SQLiteCommand(triggerSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                string checkSql = @"SELECT 1 FROM sqlite_master 
                          WHERE type = 'procedure' AND name = 'ProcessIncomeDocument'";

                var cmdCheck = new SQLiteCommand(checkSql, connection);
                var exists = cmdCheck.ExecuteScalar();

                if (exists == null)
                {
                    // Создаем процедуру
                    string createProcedureSql = @"
                    CREATE PROCEDURE ProcessIncomeDocument(
                        IN documentId INTEGER,
                        OUT resultMessage TEXT
                    )
                    BEGIN
                        DECLARE docType TEXT;
                        DECLARE docStatus TEXT;
                        DECLARE processedRows INTEGER DEFAULT 0;
    
                        -- Получаем тип и статус документа
                        SELECT DocumentType, DocumentStatus INTO docType, docStatus
                        FROM DocumentHeaders WHERE DocumentID = documentId;
    
                        -- Проверяем условия для приходования
                        IF docType != 'Приход' THEN
                            SET resultMessage = 'Ошибка: Документ не является приходным';
                        ELSEIF docStatus = 'Оприходовано' THEN
                            SET resultMessage = 'Ошибка: Документ уже оприходован';
                        ELSE
                            -- Оприходование каждой позиции
                            UPDATE ProductStocks ps
                            JOIN DocumentSpecifications ds ON ps.ProductID = ds.ProductID
                            SET ps.ActualQuantity = ps.ActualQuantity + ds.Quantity
                            WHERE ds.DocumentID = documentId;
        
                            -- Получаем количество обработанных позиций
                            SELECT ROW_COUNT() INTO processedRows;
        
                            -- Обновляем статус документа
                            UPDATE DocumentHeaders
                            SET DocumentStatus = 'Оприходовано'
                            WHERE DocumentID = documentId;
        
                            SET resultMessage = CONCAT('Успешно оприходовано ', processedRows, ' позиций');
                        END IF;
                    END;";

                    //var cmdCreate = new SQLiteCommand(createProcedureSql, connection);
                    //cmdCreate.ExecuteNonQuery();

                    Debug.WriteLine("Хранимая процедура ProcessIncomeDocument создана");
                }

            }
        }
    }

    public static SQLiteConnection GetConnection()
    {
        return new SQLiteConnection(connectionString);
    }
}