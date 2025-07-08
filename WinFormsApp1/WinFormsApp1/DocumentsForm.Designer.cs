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
            comboBox3 = new ComboBox();
            button4 = new Button();
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            button8 = new Button();
            button9 = new Button();
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
            comboBox1.Location = new Point(248, 314);
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
            dateTimePicker1.Location = new Point(404, 314);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(200, 23);
            dateTimePicker1.TabIndex = 4;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(12, 355);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(206, 23);
            comboBox2.TabIndex = 5;
            comboBox2.Text = "Товары";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(224, 355);
            textBox2.Name = "textBox2";
            textBox2.PlaceholderText = "Количество";
            textBox2.Size = new Size(113, 23);
            textBox2.TabIndex = 6;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(343, 355);
            textBox3.Name = "textBox3";
            textBox3.PlaceholderText = "Скидка";
            textBox3.Size = new Size(97, 23);
            textBox3.TabIndex = 7;
            // 
            // button1
            // 
            button1.Location = new Point(446, 347);
            button1.Name = "button1";
            button1.Size = new Size(158, 37);
            button1.TabIndex = 8;
            button1.Text = "Добавить товар";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(610, 347);
            button2.Name = "button2";
            button2.Size = new Size(164, 37);
            button2.TabIndex = 9;
            button2.Text = "Удалить товар";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Location = new Point(610, 306);
            button3.Name = "button3";
            button3.Size = new Size(164, 37);
            button3.TabIndex = 10;
            button3.Text = "Создать документ";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(121, 314);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(121, 23);
            comboBox3.TabIndex = 11;
            // 
            // button4
            // 
            button4.Location = new Point(12, 393);
            button4.Name = "button4";
            button4.Size = new Size(103, 45);
            button4.TabIndex = 12;
            button4.Text = "Оприходовать";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.Location = new Point(126, 393);
            button5.Name = "button5";
            button5.Size = new Size(116, 45);
            button5.TabIndex = 13;
            button5.Text = "Отменить приход";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // button6
            // 
            button6.Location = new Point(248, 393);
            button6.Name = "button6";
            button6.Size = new Size(114, 45);
            button6.TabIndex = 14;
            button6.Text = "Зарезервировать";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button7
            // 
            button7.Location = new Point(368, 393);
            button7.Name = "button7";
            button7.Size = new Size(112, 45);
            button7.TabIndex = 15;
            button7.Text = "Отменить резервирование";
            button7.UseVisualStyleBackColor = true;
            button7.Click += button7_Click;
            // 
            // button8
            // 
            button8.Location = new Point(486, 393);
            button8.Name = "button8";
            button8.Size = new Size(101, 45);
            button8.TabIndex = 16;
            button8.Text = "Списать";
            button8.UseVisualStyleBackColor = true;
            button8.Click += button8_Click;
            // 
            // button9
            // 
            button9.Location = new Point(593, 393);
            button9.Name = "button9";
            button9.Size = new Size(116, 45);
            button9.TabIndex = 17;
            button9.Text = "Отменить списание";
            button9.UseVisualStyleBackColor = true;
            button9.Click += button9_Click;
            // 
            // DocumentsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button9);
            Controls.Add(button8);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(comboBox3);
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
        private ComboBox comboBox3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private Button button8;
        private Button button9;
    }
}