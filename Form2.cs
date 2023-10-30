using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadSystemsIndividualka
{
    public partial class Form2 : Form
    {
        NpgsqlConnection connection;
        string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=12345;Database=VegetableShop;";
        DataTable dataTable = new DataTable();
        NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
        TextBox[] textBoxesAddProvider;
        public Form2()
        {
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            InitializeComponent();

            //initTextBoxesAddProvider();
            loadProviders();
        }

        private void loadProviders()
        {
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter("SELECT * FROM provider", connection);
            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
        }

        /*private void initTextBoxesAddProvider()
        {
            textBoxesAddProvider.Append(textBox1);
            textBoxesAddProvider.Append(textBox2);
        }*/

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void fileSystemWatcher1_Changed(object sender, System.IO.FileSystemEventArgs e)
        {

        }

        private void buttonWithTask_Click(object sender, EventArgs e)
        {
            string sqlInsert = "INSERT INTO provider (provider_name, address) VALUES (@provider_name, @address)";
            NpgsqlCommand command = new NpgsqlCommand(sqlInsert, connection);
            command.Parameters.AddWithValue("provider_name", textBox1.Text);
            command.Parameters.AddWithValue("address", textBox2.Text);
            command.Prepare();

            command.ExecuteNonQuery();
            this.textBox1.Text = "";
            this.textBox2.Text = "";

            dataTable.Clear();
            string sqlForUpdate = "SELECT * FROM provider";
            dataAdapter.SelectCommand = new NpgsqlCommand(sqlForUpdate, connection);
            dataAdapter.Fill(dataTable);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = dataTable;

            dataGridView1.DataSource = bindingSource;
            this.Hide();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Hide();
        }


    }
}
