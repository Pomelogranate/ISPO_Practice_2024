using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;


namespace СавинаУП
{
    public partial class Form2 : Form
    {
        //string connectionString = "Server=ADCLG1;Database=СавинаУПБытТех;Trusted_Connection=True;";
        string connectionString = "Server=192.168.188.11;Database=СавинаУПБытТех;Trusted_Connection=True;";
        //string connectionString = "Server=localhost\\SQLEXPRESS;Database=СавинаУПБытТех;Trusted_Connection=True;";
        SqlConnection connection;
        DataSet ds;
        DataTable dt;
        DataView dv;

        SqlDataAdapter adapterRequest;
        SqlCommandBuilder commandBuilderRequest;
        bool redact = false;
        int total = 0;
        int id=0;
        int id_Request;
        public Form2(int id)
        {
            InitializeComponent();
            this.id = id;
            connection = new SqlConnection(connectionString);
            connection.Open();

            string query = "SELECT DISTINCT HomeTechType FROM HomeTech";
            using (SqlCommand command1 = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader1 = command1.ExecuteReader())
                {
                    comboBox3.Items.Clear();
                    while (reader1.Read())
                    {
                        comboBox3.Items.Add(reader1["HomeTechType"].ToString());
                    }
                }
            }

            adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Client = {id}", connection);
            commandBuilderRequest = new SqlCommandBuilder(adapterRequest);
            ds = new DataSet();

            adapterRequest.Fill(ds);
            dt = ds.Tables[0];
            dv = new DataView(dt);
            dataGridView1.DataSource = dv;

            total = dv.Count;
            label3.Text = $"{total} из {total}";
            loadProfile();

            checkedListBox1.Items.Clear();
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                if (column.Visible)
                    checkedListBox1.Items.Add(column.HeaderText);
            }

        }
        void loadProfile()
        {
            SqlCommand command = new SqlCommand($"SELECT fio, [Role].[role] FROM Users JOIN [Role] on Users.[role] = [Role].Id_Role WHERE Id_User = {id}", connection);
            SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                textBox2.Text = $"{reader["fio"]}";
                textBox4.Text = $"{reader["role"]}";
            }
            reader.Close();
            int activeRequest = 0;
            command = new SqlCommand($"\r\nSELECT count(id_Client) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Id_client = {id} AND Request.requestStatus = 1;", connection);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                activeRequest = reader.GetInt32(0);
            }
            reader.Close();
            int doneRequest = 0;
            command = new SqlCommand($"\r\nSELECT count(id_Client) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Id_client = {id} AND Request.requestStatus = 2;", connection);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                doneRequest = reader.GetInt32(0);
            }
            reader.Close();
            textBox5.Text = $"Всего заявок: {total}\r\nЗаявки в работе: {activeRequest}\r\nГотовых заявок: {doneRequest}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!redact)
            {
                redact = true;
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Client = {id} AND Request.requestStatus = 3", connection);
                ds = new DataSet();
                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";

                string query = "SELECT DISTINCT HomeTechType FROM HomeTech";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["HomeTechType"].ToString());
                        }
                    }
                }
                comboBox1.SelectedIndex = 0;
                comboBox1.Visible=true;
                comboBox2.Visible=true;
                button4.Visible=true;
            }
            else if (redact)
            {
                redact = false;
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Client = {id}", connection);

                ds = new DataSet();

                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";
                comboBox1.Visible=false;
                comboBox2.Visible=false;
                button4.Visible=false;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedType = comboBox1.SelectedItem.ToString();
            string query = "SELECT HomeTechModel FROM HomeTech WHERE HomeTechType = @Type";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Type", selectedType);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    comboBox2.Items.Clear();
                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader["HomeTechModel"].ToString());
                    }
                }
            }
            comboBox2.SelectedIndex = 0;

        }
        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows[rowIndex].Cells[2].Value = (comboBox1.SelectedItem).ToString();
                dataGridView1.Rows[rowIndex].Cells[3].Value = (comboBox2.SelectedItem).ToString();
                BDUpdate();
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        void BDUpdate()
        {
            try 
            {
                int id_HomeTechModel = 1;
                SqlCommand command1 = new SqlCommand($"select Id_HomeTech\r\nfrom HomeTech\r\nwhere HomeTechModel Like '%{comboBox2.SelectedItem}%'", connection);
                SqlDataReader reader1 = command1.ExecuteReader();
                if (reader1.Read())
                {
                    id_HomeTechModel = reader1.GetInt32(0);
                }
                reader1.Close();

                string query = "UPDATE Request\r\nSET\r\n    startDate = @startDate,\r\n    Id_HomeTech = @Id_HomeTech\r\nWHERE Id_Request = @Id_Request; ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Id_HomeTech", id_HomeTechModel);
                    command.Parameters.AddWithValue("@Id_Request", id_Request);
                    command.ExecuteNonQuery();
                }
                
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            filter();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            filter();
        }

        private void filter()
        {
            string filt = string.Empty;
            if (textBox6.Text.Length > 0)
            {
                var selectedColumns = new List<string>();
                foreach (var item in checkedListBox1.CheckedItems)
                {
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        if (col.HeaderText == (string)item)
                            selectedColumns.Add(col.DataPropertyName);
                    }
                }

                if (selectedColumns.Count > 0)
                {
                    filt = "false";
                    foreach (var columnName in selectedColumns)
                    {
                        filt += $" OR Convert([{columnName}], 'System.String') LIKE '%{textBox6.Text}%'";
                    }

                }
                else
                {
                    filt = "false";
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        filt += $" OR CONVERT([{col.HeaderCell.Value}], System.String) LIKE '*{textBox6.Text}*'";
                    }
                }
            }
            dv.RowFilter = filt;
            label3.Text = $"{dv.Count} из {total}";
        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

            string selectedType = comboBox3.SelectedItem.ToString();
            string query = "SELECT HomeTechModel FROM HomeTech WHERE HomeTechType = @Type";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Type", selectedType);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    comboBox4.Items.Clear();
                    while (reader.Read())
                    {
                        comboBox4.Items.Add(reader["HomeTechModel"].ToString());
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox4.SelectedItem != null && textBox1.Text != "")
                {
                    int id_HomeTechModel = 1;
                    SqlCommand command1 = new SqlCommand($"select Id_HomeTech\r\nfrom HomeTech\r\nwhere HomeTechModel Like '%{comboBox4.SelectedItem}%'", connection);
                    SqlDataReader reader1 = command1.ExecuteReader();
                    if (reader1.Read())
                    {
                        id_HomeTechModel = reader1.GetInt32(0);
                    }
                    reader1.Close();

                    string query = "INSERT INTO Request (startDate, Id_HomeTech, problemDescription, Id_Client, requestStatus) VALUES (@startDate, @Id_HomeTech,@problemDescription, @Id_Client,3); ";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@startDate", DateTime.Now);
                        command.Parameters.AddWithValue("@Id_Client", id);
                        command.Parameters.AddWithValue("@Id_HomeTech", id_HomeTechModel);
                        command.Parameters.AddWithValue("@problemDescription", textBox1.Text);
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("Заявка создана");

                    textBox6.Text = "1a20";
                    filter();
                    textBox6.Text = "";
                    loadProfile();
                }
                else 
                {
                    MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //if (dataGridView1.SelectedRows.Count > 0)
            
                DialogResult dialogResult = MessageBox.Show("Вы точно хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    string query = "DELETE FROM Request WHERE Id_Request = @Id_Request";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id_Request", id_Request);
                            try
                            {
                                conn.Open();
                                cmd.ExecuteNonQuery();
                                MessageBox.Show("Заявка успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (SqlException ex)
                            {
                                MessageBox.Show("Ошибка при удалении заявки: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    textBox6.Text = "1a20";
                    filter();
                    textBox6.Text = "";
                    loadProfile();
                }
            
            else
            {
                MessageBox.Show("Выберите заявку для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].DataBoundItem is DataRowView selectedRow)
                {

                    id_Request = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
                    comboBox1.SelectedItem = selectedRow.Row["HomeTechType"]?.ToString();
                    comboBox2.SelectedItem = selectedRow.Row["HomeTechModel"]?.ToString();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string qrtext = "https://docs.google.com/spreadsheets/d/1WfPVTqLY6EKSZhXll9o9mZR0kYYH_EiYkf7yceugJ4E/edit?gid=274168655#gid=274168655"; //
            QRCodeEncoder encoder = new QRCodeEncoder();
            Bitmap qrcode = encoder.Encode(qrtext);
            pictureBox3.Image = qrcode as Image;
        }
    }
}
