namespace WinFormsApp1
{
    partial class DocumentsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dataGridView1 = new DataGridView();
            comboBox1 = new ComboBox();
            dataGridView2 = new DataGridView();
            textBox1 = new TextBox();
            dateTimePicker1 = new DateTimePicker();
            comboBox2 = new ComboBox();
            textBox2 = new TextBox();
            textBox3 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(12, 12);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(776, 131);
            dataGridView1.TabIndex = 0;
            dataGridView1.SelectionChanged += dataGridViewDocuments_SelectionChanged;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(121, 314);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(150, 23);
            comboBox1.TabIndex = 1;
            comboBox1.Text = "Контрагенты";
            // 
            // dataGridView2
            // 
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(12, 164);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(776, 125);
            dataGridView2.TabIndex = 2;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 314);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Номер документа";
            textBox1.Size = new Size(103, 23);
            textBox1.TabIndex = 3;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(12, 347);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 4;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(277, 314);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(206, 23);
            comboBox2.TabIndex = 5;
            comboBox2.Text = "Товары";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(489, 314);
            textBox2.Name = "textBox2";
            textBox2.PlaceholderText = "Количество";
            textBox2.Size = new Size(113, 23);
            textBox2.TabIndex = 6;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(608, 314);
            textBox3.Name = "textBox3";
            textBox3.PlaceholderText = "Скидка";
            textBox3.Size = new Size(97, 23);
            textBox3.TabIndex = 7;
            // 
            // button1
            // 
            button1.Location = new Point(218, 401);
            button1.Name = "button1";
            button1.Size = new Size(158, 37);
            button1.TabIndex = 8;
            button1.Text = "Добавить товар";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(382, 401);
            button2.Name = "button2";
            button2.Size = new Size(173, 37);
            button2.TabIndex = 9;
            button2.Text = "Удалить товар";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(12, 401);
            button3.Name = "button3";
            button3.Size = new Size(195, 37);
            button3.TabIndex = 10;
            button3.Text = "Создать документ";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // DocumentsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox3);
            Controls.Add(textBox2);
            Controls.Add(comboBox2);
            Controls.Add(dateTimePicker1);
            Controls.Add(textBox1);
            Controls.Add(dataGridView2);
            Controls.Add(comboBox1);
            Controls.Add(dataGridView1);
            Name = "DocumentsForm";
            Text = "DocumentsForm";
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private ComboBox comboBox1;
        private DataGridView dataGridView2;
        private TextBox textBox1;
        private DateTimePicker dateTimePicker1;
        private ComboBox comboBox2;
        private TextBox textBox2;
        private TextBox textBox3;
        private Button button1;
        private Button button2;
        private Button button3;
    }
}