using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace СавинаУП
{
    public partial class Form5 : Form
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
        int id = 0;
        int id_Request;
        int chosenRow = 0;
        public Form5(int id)
        {
            InitializeComponent();
            this.id = id;
            connection = new SqlConnection(connectionString);
            connection.Open();

            adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User", connection);
            commandBuilderRequest = new SqlCommandBuilder(adapterRequest);
            ds = new DataSet();
            adapterRequest.Fill(ds);
            dt = ds.Tables[0];
            dv = new DataView(dt);
            dataGridView1.DataSource = dv;

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
            command = new SqlCommand($"\r\nSELECT count(id_Client) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Request.requestStatus = 1;", connection);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                activeRequest = reader.GetInt32(0);
            }
            reader.Close();
            int doneRequest = 0;
            command = new SqlCommand($"\r\nSELECT count(id_Client) AS cou FROM Request JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status WHERE Request.requestStatus = 2;", connection);
            reader = command.ExecuteReader();
            if (reader.Read())
            {
                doneRequest = reader.GetInt32(0);
            }
            reader.Close();
            total = dv.Count;
            label3.Text = $"{total} из {total}";
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
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User", connection);
                ds = new DataSet();
                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";

                masterfio();
                comboBox1.SelectedIndex = 0;
                comboBox1.Visible = true;

                dateTimePicker1.Visible = true;
            }
            else if (redact)
            {
                redact = false;
                adapterRequest = new SqlDataAdapter($"SELECT Id_Request,startDate, HomeTech.HomeTechType, HomeTech.HomeTechModel,problemDescription, RequestStatus.[Status], completionDate, repairParts, Users.fio AS Master \r\nFROM (([Request] JOIN HomeTech on Request.Id_HomeTech=HomeTech.Id_HomeTech)\r\nleft JOIN RequestStatus on Request.requestStatus = RequestStatus.Id_Status)\r\nleft JOIN Users on Request.Id_Master = Users.Id_User", connection);

                ds = new DataSet();

                adapterRequest.Fill(ds);
                dt = ds.Tables[0];
                dv = new DataView(dt);
                dataGridView1.DataSource = dv;
                label3.Text = $"{dv.Count} из {total}";
                comboBox1.Visible = false;
                dateTimePicker1.Visible = false;
            }
        }

        void masterfio()
        {
            string query3 = "SELECT fio FROM Users WHERE role = 2";
            using (SqlCommand command3 = new SqlCommand(query3, connection))
            {
                using (SqlDataReader reader3 = command3.ExecuteReader())
                {
                    comboBox1.Items.Clear();
                    while (reader3.Read())
                    {
                        comboBox1.Items.Add(reader3["fio"].ToString());
                    }
                }
            }
        }
        private void comboBox1_CellContentClick(object sender, EventArgs e)
        {
            try
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows[rowIndex].Cells[8].Value = (comboBox1.SelectedItem).ToString();
                BDUpdate();
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)//////////////////////
        {
            try
            {
                int rowIndex = dataGridView1.CurrentCell.RowIndex;
                dataGridView1.Rows[rowIndex].Cells[6].Value = dateTimePicker1.Value;////////////
                BDUpdate();
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView1.Rows[e.RowIndex].DataBoundItem is DataRowView selectedRow)
                {
                    chosenRow = e.RowIndex;
                    id_Request = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
                    comboBox1.SelectedItem = selectedRow.Row["Master"]?.ToString();
                    if (selectedRow.Row["completionDate"] != DBNull.Value)
                    {
                        dateTimePicker1.Value = Convert.ToDateTime(selectedRow.Row["completionDate"]);
                    }
                    masterfio();
                }
            }
        }

        void BDUpdate()
        {
            try
            {
                //int rowIndex = dataGridView1.CurrentCell.RowIndex; ВЗЛОМ ПЕНТАГОНА

                //Console.WriteLine(dateTimePicker1.Value);
                //Console.WriteLine(Convert.ToDateTime(dataGridView1.Rows[chosenRow].Cells[1].Value));


                //if (dateTimePicker1.Value > Convert.ToDateTime(dataGridView1.Rows[chosenRow].Cells[1].Value)) { ВЗЛОМ ПЕНТАГОНА 2
                //    int Id_Master = 0;
                //    string query = $"SELECT Id_User FROM Users WHERE fio Like '%{comboBox1.SelectedItem}%'";
                //    using (SqlCommand command = new SqlCommand(query, connection))
                //    {
                //        using (SqlDataReader reader = command.ExecuteReader())
                //        {
                //            comboBox1.Items.Clear();
                //            while (reader.Read())
                //            {
                //                Id_Master = reader.GetInt32(0);
                //            }
                //        }
                //    }


                //    query = "UPDATE Request\r\nSET\r\n Id_Master=@Id_Master,completionDate=@completionDate \r\nWHERE Id_Request = @Id_Request; ";
                //    using (SqlCommand command = new SqlCommand(query, connection))
                //    {
                //        command.Parameters.AddWithValue("@Id_Master", Id_Master);
                //        command.Parameters.AddWithValue("@Id_Request", id_Request);
                //        command.Parameters.AddWithValue("@completionDate", dateTimePicker1.Value);
                //        command.ExecuteNonQuery();
                //    }
                //}
                //else
                //{
                //    MessageBox.Show("Дата окончания должна быть после даты начала", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
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
    }
}