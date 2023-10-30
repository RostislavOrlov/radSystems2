using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RadSystemsIndividualka
{
    public partial class Form4 : Form
    {
        NpgsqlConnection connection;
        string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=12345;Database=VegetableShop;";
        DataTable dataTable = new DataTable();
        NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
        TextBox[] textBoxesAddProvider;

        public Form4()
        {
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            InitializeComponent();

            loadGoods();
        }

        private void loadGoods()
        {
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter("SELECT * FROM goods", connection);
            adapter.Fill(dataTable);
            dataGridView1.DataSource = dataTable;
        }

        private void buttonWithTask_Click(object sender, EventArgs e)
        {
            string sqlInsert = "INSERT INTO goods (goods_name, cost_of_one_unit_of_goods) VALUES (@goods_name, @cost_of_one_unit_of_goods)";
            NpgsqlCommand command = new NpgsqlCommand(sqlInsert, connection);
            command.Parameters.AddWithValue("goods_name", this.textBox1.Text);
            command.Parameters.AddWithValue("cost_of_one_unit_of_goods", Int32.Parse(this.textBox2.Text));
            command.Prepare();

            command.ExecuteNonQuery();
            this.textBox1.Text = "";
            this.textBox2.Text = "";

            dataTable.Clear();
            string sqlForUpdate = "SELECT * FROM goods";
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
        private void Form4_Load(object sender, EventArgs e)
        {

        }
    }
}
