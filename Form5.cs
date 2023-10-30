using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RadSystemsIndividualka
{

    public partial class Form5 : Form
    {
        string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=12345;Database=VegetableShop;";
        DataTable dtReceived = new DataTable();
        DataTable dtScrapped = new DataTable();
        DataTable dtSold = new DataTable();

        public Form5(DataGridView dataGridView_Form1, DateTime dateTime1, DateTime dateTime2)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            InitializeComponent();
            loadTables(dataGridView_Form1, dateTime1, dateTime2);
            makeExportToXML(dataGridView1);
        }

        private void loadTables(DataGridView dataGridView_Form1, DateTime dateTime1, DateTime dateTime2)
        {
            fetchGoods(dataGridView1, dtReceived, dataGridView_Form1, "goods_received", "date_receive", dateTime1, dateTime2);
            fetchGoods(dataGridView2, dtScrapped, dataGridView_Form1, "goods_scrapped", "date_write_off", dateTime1, dateTime2);
            fetchGoods(dataGridView3, dtSold, dataGridView_Form1, "goods_sold", "date_sale", dateTime1, dateTime2);
        }

        private void fetchGoods(DataGridView dgv, DataTable dt, DataGridView dgv_form1, string table, string dateString, DateTime dateTime1, DateTime dateTime2)
        {
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            ArrayList providers = new ArrayList();
            string providersString = "(";

            for (int i = 0; i < dgv_form1.Rows.Count; i++)
            {
                var f = dgv_form1.Rows[i].Cells[0].Value;
                if (f == null)
                    f = false;
                bool f1 = (bool)f;
                if (f1 == true)
                {
                    providersString += "'";
                    providersString += dgv_form1.Rows[i].Cells[1].Value.ToString();
                    providersString += "'";
                    if (i != dgv_form1.Rows.Count - 1)
                        providersString += ", ";
                }

            }
            providersString = providersString.Remove(providersString.Length - 2);
            providersString += ")";
            string columns = table + ".goods_id, goods.goods_name, provider.provider_id, cost_of_one_unit_of_goods, goods_count, goods_amount, " + dateString;
            string sql = "SELECT " + columns + " FROM provider INNER JOIN goods_providers ON provider.provider_id = goods_providers.provider_id INNER JOIN goods ON goods_providers.goods_id = goods.goods_id INNER JOIN " + table + " ON goods.goods_id = " + table + ".goods_id";
            sql += " WHERE provider.provider_name IN " + providersString;
            sql += " AND " + dateString + " >= @date_1 AND " + dateString + " <= @date_2";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            dataAdapter.SelectCommand = command;
            dataAdapter.SelectCommand.Parameters.AddWithValue("date_1", dateTime1);
            dataAdapter.SelectCommand.Parameters.AddWithValue("date_2", dateTime2);
            dataAdapter.Fill(dt);
            dgv.DataSource = dt;
            connection.Close();
        }

        private void makeExportToXML(DataGridView dgv)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            XmlWriter writer = XmlWriter.Create("exportWritter.xml", settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("data1");
            for (int j = 0; j < dataGridView1.RowCount - 1; j++)
            {
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    writer.WriteStartElement(dataGridView1.Columns[i].HeaderText);
                    writer.WriteString(dataGridView1.Rows[j].Cells[i].Value.ToString());
                    writer.WriteEndElement();
                }
            }

            for (int j = 0; j < dataGridView2.RowCount - 1; j++)
            {
               
                for (int i = 0; i < dataGridView2.ColumnCount; i++)
                {
                    writer.WriteStartElement(dataGridView2.Columns[i].HeaderText);
                    writer.WriteString(dataGridView3.Rows[j].Cells[i].Value.ToString());
                    writer.WriteEndElement();
                }
            }

            for (int j = 0; j < dataGridView3.RowCount - 1; j++)
            {
                for (int i = 0; i < dataGridView3.ColumnCount; i++)
                {
                    writer.WriteStartElement(dataGridView3.Columns[i].HeaderText);
                    writer.WriteString(dataGridView3.Rows[j].Cells[i].Value.ToString());
                    writer.WriteEndElement();
                }
            }

            writer.WriteEndElement();

            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("Данные были экспортированны в xml файл", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form5_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
