using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace СавинаУП
{
    public partial class Form3 : Form
    {
        string connectionString = "Server=ADCLG1;Database=СавинаУПБытТех;Trusted_Connection=True;";
        //string connectionString = "Server=localhost\\SQLEXPRESS;Database=СавинаУПБытТех;Trusted_Connection=True;";
        SqlConnection connection;
        DataSet ds;
        DataTable dt;
        DataView dv;

        SqlDataAdapter adapterRequest;
        SqlCommandBuilder commandBuilderRequest;
        bool redact = false;
        int total = 0;
        int id = 0;
        int id_Request;
        public Form3(int id)
        {
            InitializeComponent();
            this.id = id;
            connection = new SqlConnection(connectionString);
            connection.Open();

            

            string query = $"select Id_Request from Request where Id_Master = {id}";
            using (SqlCommand command1 = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader1 = command1.ExecuteReader())
                {
                    comboBox3.Items.Clear();
                    while (reader1.Read())
                    {
                        comboBox3.Items.Add(reader1["Id_Request"].ToString());
                    }
                }
            }

            adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Master = {id}", connection);
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
            command = new SqlCommand($"SELECT count(id_Master) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Id_Master = {id} AND Request.requestStatus = 1;", connection);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                activeRequest = reader.GetInt32(0);
            }
            reader.Close();
            int doneRequest = 0;
            command = new SqlCommand($"SELECT count(id_Master) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Id_Master = {id} AND Request.requestStatus = 2;", connection);
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
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Master = {id} AND Request.requestStatus = 1", connection);
                ds = new DataSet();
                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";

                string query = "SELECT Status FROM RequestStatus WHERE NOT Id_Status = 3";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["Status"].ToString());
                        }
                    }
                }
                comboBox1.SelectedIndex = 0;
                comboBox1.Visible = true;
            }
            else if (redact)
            {
                redact = false;
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User\r\nWHERE Id_Master = {id}", connection);
                ds = new DataSet();
                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";
                comboBox1.Visible = false;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows[rowIndex].Cells[5].Value = (comboBox1.SelectedItem).ToString();
                BDUpdate();
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].DataBoundItem is DataRowView selectedRow)
                {
                    id_Request = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
                    comboBox1.SelectedItem = selectedRow.Row["Status"]?.ToString();
                }
            }
        }

        void BDUpdate()
        {
            try
            {
                int id_Status = 1;
                SqlCommand command1 = new SqlCommand($"select Id_Status\r\nfrom RequestStatus\r\nwhere Status Like '%{comboBox1.SelectedItem}%'", connection);
                SqlDataReader reader1 = command1.ExecuteReader();
                if (reader1.Read())
                {
                    id_Status = reader1.GetInt32(0);
                }
                reader1.Close();

                string query = "UPDATE Request\r\nSET\r\n requestStatus = @id_Status, completionDate=@completionDate\r\nWHERE Id_Request = @Id_Request; ";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_Status", id_Status);
                    command.Parameters.AddWithValue("@Id_Request", id_Request);
                    if (id_Status == 2)
                    {
                        command.Parameters.AddWithValue("@completionDate", DateTime.Now);
                    }
                    else 
                    {
                        command.Parameters.AddWithValue("@completionDate", DBNull.Value);
                    }
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

        private void button3_Click(object sender, EventArgs e)//////////////////////////
        {
            try
            {
                if (comboBox3.SelectedItem != null && textBox1.Text != "")
                {
                    string query = "INSERT INTO Comments (message, Id_Master, Id_Request) VALUES (@message, @Id_Master, @Id_Request);";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@message", textBox1.Text);
                        command.Parameters.AddWithValue("@Id_Master", id);
                        command.Parameters.AddWithValue("@Id_Request", comboBox3.SelectedItem);
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("Комментарий создан");
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
    }
}
