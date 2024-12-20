using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace СавинаУП
{
    public partial class SignIn : Form
    {
        Random rnd = new Random();
        int attempt = 0;
        bool block = false;
        int time = 0;
        string text1="";

        //string connectionString = "Server=ADCLG1;Database=СавинаУПБытТех;Trusted_Connection=True;";
        string connectionString = "Server=192.168.188.11;Database=СавинаУПБытТех;Trusted_Connection=True;";
        //string connectionString = "Server=localhost\\SQLEXPRESS;Database=СавинаУПБытТех;Trusted_Connection=True;";
        public SignIn()
        {
            InitializeComponent();
            timer1.Interval = 1000;
        }
        private void button1_Click(object sender, EventArgs e)//Back
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            textBox4.Text = CAPTCHA();
        }
        private void button3_Click(object sender, EventArgs e)//Sign In
        {
            string login = textBox1.Text;
            string Password = textBox2.Text;
            Log(login, Password);      
        }
        public bool Log(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand($"SELECT Id_User, role FROM Users Where login = '{login}' and password='{password}';", connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read() && (!textBox3.Visible || (textBox4.Text == textBox3.Text && textBox3.Visible))) //Если пользователь существует
                    {
                        if ((int)reader["role"] == 1)//Заказчик
                        {
                            Hide();
                            Form2 Check = new Form2((int)reader["Id_User"]);
                            if (Check.ShowDialog() == DialogResult.OK)
                                Show();
                            else
                                Close();
                        }
                        if ((int)reader["role"] == 2)//Мастер
                        {
                            Hide();
                            Form3 Check = new Form3((int)reader["Id_User"]);
                            if (Check.ShowDialog() == DialogResult.OK)
                                Show();
                            else
                                Close();
                        }
                        if ((int)reader["role"] == 3)//Менеджер
                        {
                            Hide();
                            Form5 Check = new Form5((int)reader["Id_User"]);
                            if (Check.ShowDialog() == DialogResult.OK)
                                Show();
                            else
                                Close();
                        }
                        if ((int)reader["role"] == 4)//Оператор
                        {
                            Hide();
                            Form4 Check = new Form4((int)reader["Id_User"]);
                            if (Check.ShowDialog() == DialogResult.OK)
                                Show();
                            else
                                Close();
                        }
                        LogLoginAttempt(login, true);
                        return true;
                    }
                    else if (!reader.Read())
                    {
                        MessageBox.Show("Неверный логин или пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox3.Visible = true;
                        textBox4.Visible = true;
                        button2.Visible = true;
                        textBox4.Text = CAPTCHA();
                        attempt++;
                        LogLoginAttempt(login, false);
                        return false;
                    }
                    if (!(textBox4.Text == textBox3.Text) && textBox3.Visible)
                    {
                        MessageBox.Show("Неверная капча!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textBox4.Text = CAPTCHA();
                        LogLoginAttempt(login, false);
                    }
                    reader.Close();

                }
                catch (Exception t)
                {
                    MessageBox.Show(t.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (attempt >= 3 && !block)
                {
                    time = 180;
                    block = true;
                    timer1.Enabled = true;
                }
                if (block)
                {
                    label4.Visible = true;
                    label5.Visible = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    button3.Enabled = false;
                }
                return false;
            }
        }
        public string CAPTCHA()
        {
            string text1 = String.Empty;
            string ALF = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM";
            for (int i = 0; i < 4; ++i)
            {
                text1 += ALF[rnd.Next(ALF.Length)];
            }
            pictureBox2.Image = this.CreateImage(pictureBox1.Width, pictureBox1.Height);
            this.text1 = text1;
            return text1;
        }

        private Bitmap CreateImage(int Width, int Height)
        {
            string text = String.Empty;
            Bitmap result = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage((System.Drawing.Image)result);
            g.Clear(Color.Gray);
            text = text1;
            g.DrawString(text,
                         new Font("Arial", 20),
                         Brushes.Black,
                         new PointF(0, 0));
            g.DrawLine(Pens.Black,
                       new Point(0, 0),
                       new Point(Width - 1, Height - 1));
            g.DrawLine(Pens.Black,
                       new Point(0, Height - 1),
                       new Point(Width - 1, 0));
            for (int i = 0; i < Width; ++i)
                for (int j = 0; j < Height; ++j)
                    if (rnd.Next() % 20 == 0)
                        result.SetPixel(i, j, Color.White);

            return result;
        }

        private void LogLoginAttempt(string login, bool isSuccess)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO LogHistory (Login, LogTime, Success) VALUES (@login, @logTime, @success);";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@logTime", DateTime.Now);
                    command.Parameters.AddWithValue("@success", isSuccess);
                    command.ExecuteNonQuery();
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            time--;
            label5.Text = TimeFormat(time);
            if (time <= 0)
            {
                timer1.Stop();
                button3.Enabled = true;
                block = false;
                label5.Text = string.Empty;
                label4.Visible = false;
                label5.Visible = false;
                attempt = 0;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
            }
        }
        private string TimeFormat(int sec)
        {
            int minutes = sec / 60;
            sec %= 60;
            return $"{minutes}:{sec:D2}";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.PasswordChar = (char)0;
            }
            else
            {
                textBox2.PasswordChar = '*';
            }
        }
    }
}
