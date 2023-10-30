using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Npgsql;

namespace RadSystemsIndividualka
{
    public partial class Form1 : Form
    {
        NpgsqlConnection connection;
        string connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=12345;Database=VegetableShop;";
        DataTable dataTable = new DataTable();
        NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
        TextBox[] textBoxesAddDelivery;
        int delivery_id = 0;
        public Form1()
        {
            connection = new NpgsqlConnection(connectionString); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            connection.Open();                                   //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            InitializeComponent();
            initComboboxProviderAndGoods();
            fetchDeliveries();
            loadProvidersMultipleSelect();
            loadDeliveriesToDGV();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void initTextBoxesAddDelivery()
        {
            textBoxesAddDelivery.Append(textBox1);
            textBoxesAddDelivery.Append(textBox2);
        }

        private void initComboboxProviderAndGoods()
        {
            //comboBox1.Items[0] = "Выбрать...";
            fillComboboxProvider();
            fillComboboxGoods();
        }

        private void fillComboboxProvider()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sqlQuerySelect = "SELECT * FROM provider";
            NpgsqlCommand command = new NpgsqlCommand(sqlQuerySelect, connection);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    string string_for_provider = dataReader.GetValue(0).ToString() + ")" + dataReader.GetValue(1).ToString();

                    this.comboBox1.Items.Add((string_for_provider));
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();
        }

        private void fillComboboxGoods()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sqlQuerySelect = "SELECT * FROM goods";
            NpgsqlCommand command = new NpgsqlCommand(sqlQuerySelect, connection);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    string string_for_good = dataReader.GetValue(0).ToString() + ")" + dataReader.GetValue(1).ToString();

                    this.comboBox2.Items.Add((string_for_good));
                    this.comboBox3.Items.Add((string_for_good));
                    this.comboBox4.Items.Add((string_for_good));
                    this.comboBox5.Items.Add((string_for_good));
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 1)
            {
                this.delivery_id = findLastInsertDeliveryId();
                if (this.delivery_id != 0)
                    this.delivery_id += 1;
                else
                    this.delivery_id = 1;
            }
                
            string sqlInsert = "INSERT INTO delivery_component (goods_id, provider_id, delivery_id, goods_name, provider_name, quantity, price) VALUES (@goods_id, @provider_id, @delivery_id, @goods_name, @provider_name, @quantity, @price)";
            NpgsqlCommand command = new NpgsqlCommand(sqlInsert, connection);
            command.Parameters.AddWithValue("goods_id", Int32.Parse(this.comboBox2.Text.Split(')')[0]));
            command.Parameters.AddWithValue("provider_id", Int32.Parse(this.comboBox1.Text.Split(')')[0]));
            command.Parameters.AddWithValue("delivery_id", this.delivery_id);
            command.Parameters.AddWithValue("goods_name", this.comboBox2.Text.Split(')')[1]);
            command.Parameters.AddWithValue("provider_name", this.comboBox1.Text.Split(')')[1]);
            command.Parameters.AddWithValue("quantity", Int32.Parse(this.textBox1.Text));
            command.Parameters.AddWithValue("price", Int32.Parse(this.textBox2.Text));
            command.Prepare();

            command.ExecuteNonQuery();
            this.textBox1.Text = "";
            this.textBox2.Text = "";
            /*comboBox1.SelectedItem = -1;
            comboBox2.SelectedItem = -1;*/

            dataTable.Clear();
            string sqlForUpdate = "SELECT * FROM delivery_component WHERE delivery_id = @delivery_id";
            dataAdapter.SelectCommand = new NpgsqlCommand(sqlForUpdate, connection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("delivery_id", this.delivery_id);
            dataAdapter.Fill(dataTable);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = dataTable;

            dataGridView1.DataSource = bindingSource;
        }

        private int findLastInsertDeliveryId()
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            int delivery_id = 0;
            string sql = "SELECT delivery_id FROM delivery_component ORDER BY delivery_id DESC LIMIT 1";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    delivery_id = Int32.Parse(dataReader.GetValue(0).ToString());
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();

            return delivery_id;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            string sqlInsert = "INSERT INTO delivery(delivery_date) VALUES (@delivery_date)";
            NpgsqlCommand command = new NpgsqlCommand(sqlInsert, connection);
            command.Parameters.AddWithValue("delivery_date", DateTime.Now);
            command.Prepare();
            command.ExecuteNonQuery();

            for (int i = 0; i < dataGridView1.Rows.Count - 1; ++i)
            {
                string sqlInsert2 = "INSERT INTO delivery_component_to_delivery (delivery_id, delivery_component_id) VALUES (@delivery_id, @delivery_component_id)";
                NpgsqlCommand command2 = new NpgsqlCommand(sqlInsert2, connection);
                command2.Parameters.AddWithValue("delivery_id", this.delivery_id);
                command2.Parameters.AddWithValue("delivery_component_id", Int32.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString()));
                command2.Prepare();
                command2.ExecuteNonQuery();
            }

            int countOfRows = dataGridView1.Rows.Count;
            for (int i = 0; i < countOfRows - 1; ++i)
                dataGridView1.Rows.Remove(dataGridView1.Rows[0]);
            this.delivery_id = 0;
        }

        private void fetchDeliveries()
        {
            DataTable dataTableDeliveries = new DataTable();
            string columns = "delivery_component_to_delivery.delivery_id, delivery_component_to_delivery.delivery_component_id, goods_id, provider_id, goods_name, provider_name, quantity, price";
            string sqlForFetch = "SELECT " + columns + " FROM delivery_component_to_delivery INNER JOIN delivery_component ON delivery_component_to_delivery.delivery_component_id = delivery_component.delivery_component_id"; // INNER JOIN delivery_component ON delivery_component_to_delivery.delivery_id = delivery_component.delivery_id
            dataAdapter.SelectCommand = new NpgsqlCommand(sqlForFetch, connection);
            dataAdapter.Fill(dataTableDeliveries);
            dataGridView5.DataSource = dataTableDeliveries;
        }

        private void loadDeliveriesToDGV()
        {
            DataTable dataGridViewDeliveries = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            string sql = "SELECT * FROM delivery_component LIMIT 0";
            dataAdapter.SelectCommand = new NpgsqlCommand(sql, connection);
            dataAdapter.Fill(dataGridViewDeliveries);
            dataGridView1.DataSource = dataGridViewDeliveries;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string sql = "DELETE FROM delivery_component WHERE delivery_component_id = @delivery_component_id";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("delivery_component_id", Int32.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString()));
            command.Prepare();
            command.ExecuteNonQuery();

            dataTable.Clear();
            string sqlForUpdate = "SELECT * FROM delivery_component WHERE delivery_id = @delivery_id";
            dataAdapter.SelectCommand = new NpgsqlCommand(sqlForUpdate, connection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("delivery_id", findLastInsertDeliveryId());
            dataAdapter.Fill(dataTable);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = dataTable;

            dataGridView1.DataSource = bindingSource;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            string columns = "delivery_component_to_delivery.delivery_id, delivery_component_to_delivery.delivery_component_id, goods_id, provider_id, goods_name, provider_name, quantity, price";
            string sql = "SELECT " + columns + " FROM delivery_component_to_delivery INNER JOIN delivery_component ON delivery_component_to_delivery.delivery_component_id = delivery_component.delivery_component_id WHERE delivery_component_to_delivery.delivery_id IN (SELECT delivery_id FROM delivery WHERE delivery_date >= @date1 AND delivery_date <= @date2)";

            dataAdapter.SelectCommand = new NpgsqlCommand(sql, connection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("date1", dateTimePicker4.Value);
            dataAdapter.SelectCommand.Parameters.AddWithValue("date2", dateTimePicker5.Value);

            dataAdapter.Fill(dataTable);

            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = dataTable;

            dataGridView5.DataSource = bindingSource;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();

            string sqlGetCount = "SELECT goods_count FROM goods_received WHERE goods_id = @goods_id";
            NpgsqlCommand commandGetCount = new NpgsqlCommand(sqlGetCount, connection);
            commandGetCount.Parameters.AddWithValue("goods_id", Int32.Parse(this.comboBox3.Text.Split(')')[0]));
            NpgsqlDataReader dataReader = commandGetCount.ExecuteReader();
            int goods_count = 0;
            try
            {
                while (dataReader.Read())
                {
                    goods_count = Int32.Parse(dataReader.GetValue(0).ToString());
                }
                
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }

            connection.Close();

            connection.Open();
            goods_count += Int32.Parse(this.textBox5.Text);

            string sqlGoodsReceived = "INSERT INTO goods_received (goods_id, date_receive, goods_count, goods_amount) VALUES (@goods_id, @date_receive, @goods_count, @goods_amount) \r\nON CONFLICT (goods_id) DO UPDATE\r\nSET date_receive = @date_receive, goods_count = @goods_count, goods_amount = @goods_amount";

            int goods_id = Int32.Parse(this.comboBox3.Text.Split(')')[0]);
            NpgsqlCommand commandGoodsReceived = new NpgsqlCommand(sqlGoodsReceived, connection);
            int count = Int32.Parse(this.textBox5.Text);
            commandGoodsReceived.Parameters.AddWithValue("goods_id", goods_id);
            commandGoodsReceived.Parameters.AddWithValue("date_receive", DateTime.Parse(dateTimePicker1.Value.ToString()));
            commandGoodsReceived.Parameters.AddWithValue("goods_count", goods_count);
            commandGoodsReceived.Parameters.AddWithValue("goods_amount", calculateGoodsAmount(goods_id, goods_count));
            commandGoodsReceived.Prepare();
            commandGoodsReceived.ExecuteNonQuery();

            //JOIN
            dataTable.Clear();
            string sqlSelectGoodsReceived = "SELECT goods_received.goods_id, goods_name, cost_of_one_unit_of_goods, date_receive, goods_count, goods_amount\r\nFROM goods INNER JOIN goods_received ON goods.goods_id = goods_received.goods_id";
            NpgsqlCommand commandSelectGoodsReceived = new NpgsqlCommand(sqlSelectGoodsReceived, connection);
            dataAdapter.SelectCommand = commandSelectGoodsReceived;
            dataAdapter.Fill(dataTable);
            dataGridView2.DataSource = dataTable;

            connection.Close();

            this.textBox5.Text = "";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sqlGetCount = "SELECT goods_count FROM goods_scrapped WHERE goods_id = @goods_id";
            NpgsqlCommand commandGetCount = new NpgsqlCommand(sqlGetCount, connection);
            commandGetCount.Parameters.AddWithValue("goods_id", Int32.Parse(this.comboBox4.Text.Split(')')[0]));
            NpgsqlDataReader dataReader = commandGetCount.ExecuteReader();
            int goods_count = 0;
            try
            {
                while (dataReader.Read())
                {
                    goods_count = Int32.Parse(dataReader.GetValue(0).ToString());
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }

            connection.Close();

            connection.Open();
            goods_count += Int32.Parse(this.textBox3.Text);

            string sqlGoodsReceived = "INSERT INTO goods_scrapped (goods_id, date_write_off, goods_count, goods_amount) VALUES (@goods_id, @date_write_off, @goods_count, @goods_amount) \r\nON CONFLICT (goods_id) DO UPDATE\r\nSET date_write_off = @date_write_off, goods_count = @goods_count, goods_amount = @goods_amount";

            int goods_id = Int32.Parse(this.comboBox4.Text.Split(')')[0]);
            NpgsqlCommand commandGoodsReceived = new NpgsqlCommand(sqlGoodsReceived, connection);
            int count = Int32.Parse(this.textBox3.Text);
            commandGoodsReceived.Parameters.AddWithValue("goods_id", goods_id);
            commandGoodsReceived.Parameters.AddWithValue("date_write_off", DateTime.Parse(dateTimePicker2.Value.ToString()));
            commandGoodsReceived.Parameters.AddWithValue("goods_count", goods_count);
            commandGoodsReceived.Parameters.AddWithValue("goods_amount", calculateGoodsAmount(goods_id, goods_count));
            commandGoodsReceived.Prepare();
            commandGoodsReceived.ExecuteNonQuery();

            //JOIN
            dataTable.Clear();
            string sqlSelectGoodsReceived = "SELECT goods_scrapped.goods_id, goods_name, cost_of_one_unit_of_goods, date_write_off, goods_count, goods_amount\r\nFROM goods INNER JOIN goods_scrapped ON goods.goods_id = goods_scrapped.goods_id";
            NpgsqlCommand commandSelectGoodsReceived = new NpgsqlCommand(sqlSelectGoodsReceived, connection);
            dataAdapter.SelectCommand = commandSelectGoodsReceived;
            dataAdapter.Fill(dataTable);
            dataGridView3.DataSource = dataTable;
            
            connection.Close();

            this.textBox3.Text = "";
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sqlGetCount = "SELECT goods_count FROM goods_sold WHERE goods_id = @goods_id";
            NpgsqlCommand commandGetCount = new NpgsqlCommand(sqlGetCount, connection);
            commandGetCount.Parameters.AddWithValue("goods_id", Int32.Parse(this.comboBox5.Text.Split(')')[0]));
            NpgsqlDataReader dataReader = commandGetCount.ExecuteReader();
            int goods_count = 0;
            try
            {
                while (dataReader.Read())
                {
                    goods_count = Int32.Parse(dataReader.GetValue(0).ToString());
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }

            connection.Close();

            connection.Open();
            goods_count += Int32.Parse(this.textBox4.Text);

            string sqlGoodsSold = "INSERT INTO goods_sold (goods_id, date_sale, goods_count, goods_amount) VALUES (@goods_id, @date_sale, @goods_count, @goods_amount) \r\nON CONFLICT (goods_id) DO UPDATE\r\nSET date_sale = @date_sale, goods_count = @goods_count, goods_amount = @goods_amount";

            int goods_id = Int32.Parse(this.comboBox5.Text.Split(')')[0]);
            NpgsqlCommand commandGoodsSold = new NpgsqlCommand(sqlGoodsSold, connection);
            int count = Int32.Parse(this.textBox4.Text);
            commandGoodsSold.Parameters.AddWithValue("goods_id", goods_id);
            commandGoodsSold.Parameters.AddWithValue("date_sale", DateTime.Parse(dateTimePicker3.Value.ToString()));
            commandGoodsSold.Parameters.AddWithValue("goods_count", goods_count);
            commandGoodsSold.Parameters.AddWithValue("goods_amount", calculateGoodsSoldAmount(goods_id, goods_count));
            commandGoodsSold.Prepare();
            commandGoodsSold.ExecuteNonQuery();

            //JOIN
            dataTable.Clear();
            string sqlSelectGoodsSold = "SELECT goods_sold.goods_id, goods_name, cost_of_one_unit_of_goods, date_sale, goods_count, goods_amount\r\nFROM goods INNER JOIN goods_sold ON goods.goods_id = goods_sold.goods_id";
            NpgsqlCommand commandSelectGoodsSold = new NpgsqlCommand(sqlSelectGoodsSold, connection);
            dataAdapter.SelectCommand = commandSelectGoodsSold;
            dataAdapter.Fill(dataTable);
            dataGridView4.DataSource = dataTable;

            connection.Close();

            this.textBox4.Text = "";
        }

        private int calculateGoodsAmount(int goods_id, int count)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sql = "SELECT cost_of_one_unit_of_goods FROM goods WHERE goods_id = @goods_id";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("goods_id", goods_id);
            int cost_of_one_unit_of_goods = 0;
            int goods_amount = 0;

            NpgsqlDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    cost_of_one_unit_of_goods = Int32.Parse(dataReader.GetValue(0).ToString());
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();
             
            goods_amount = cost_of_one_unit_of_goods * count;

            return goods_amount;
        }

        private int calculateGoodsSoldAmount(int goods_id, int count)
        {
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sql = "SELECT cost_of_one_unit_of_goods FROM goods WHERE goods_id = @goods_id";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("goods_id", goods_id);
            int cost_of_one_unit_of_goods = 0;
            int goods_amount = 0;

            NpgsqlDataReader dataReader = command.ExecuteReader();
            try
            {
                while (dataReader.Read())
                {
                    cost_of_one_unit_of_goods = Int32.Parse(dataReader.GetValue(0).ToString());
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();

            goods_amount = Convert.ToInt32(cost_of_one_unit_of_goods * count * 1.2);

            return goods_amount;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4();
            form4.ShowDialog();
        }

        private void loadProvidersMultipleSelect()
        {
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "";
            checkBoxColumn.Name = "";
            DataGridViewTextBoxColumn providersColumn = new DataGridViewTextBoxColumn();
            providersColumn.HeaderText = "Providers";
            providersColumn.Name = "Providers";
            this.dataGridView6.Columns.AddRange(new DataGridViewColumn[] { checkBoxColumn, providersColumn });

            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            string sql = "SELECT provider_name FROM provider";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            NpgsqlDataReader dataReader = command.ExecuteReader();
            ArrayList providers = new ArrayList();

            try
            {
                while (dataReader.Read())
                {
                    providers.Add(dataReader.GetValue(0).ToString());
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
            connection.Close();

            for (int i = 0; i < providers.Count; i++)
            {
                dataGridView6.Rows.Add();
                dataGridView6.Rows[i].Cells[1].Value = providers[i].ToString();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            /*DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();
            ArrayList providers = new ArrayList();
            string providersString = "(";

            for (int i = 0; i < dataGridView6.Rows.Count; i++)
            {
                var f = dataGridView6.Rows[i].Cells[0].Value;
                if (f == null)
                    f = false;
                bool f1 = (bool) f;
                if (f1 == true)
                {
                    providersString += "'";
                    providersString += dataGridView6.Rows[i].Cells[1].Value.ToString();
                    providersString += "'";
                    if (i != dataGridView6.Rows.Count - 1)
                        providersString += ", ";
                }
                    //providers.Add(dataGridView6.Rows[i].Cells[1]);
                    
            }
            providersString = providersString.Remove(providersString.Length - 2);
            providersString += ")";

            string sql = "SELECT * FROM provider WHERE provider_name IN " + providersString;
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            dataAdapter.SelectCommand = command;
            //dataAdapter.SelectCommand.Parameters.AddWithValue("temp", temp);
            dataAdapter.Fill(dataTable);
            dataGridView7.DataSource = dataTable;

            connection.Close();*/

            Form5 form5 = new Form5(dataGridView6, dateTimePicker6.Value, dateTimePicker7.Value);
            form5.ShowDialog();

        }

        private void button10_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            string columns = "goods_sold.goods_id, goods_name, cost_of_one_unit_of_goods, date_sale, goods_count, goods_amount";
            string sql = "SELECT " + columns + " FROM goods INNER JOIN goods_sold ON goods.goods_id = goods_sold.goods_id WHERE date_sale >= @date_1 AND date_sale <= @date_2";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("date_1", dateTimePicker9.Value);
            command.Parameters.AddWithValue("date_2", dateTimePicker8.Value);
            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataTable);
            dataGridView8.DataSource = dataTable;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            XmlWriter writer = XmlWriter.Create("exportWritterSale.xml", settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("data");
            for (int j = 0; j < dataGridView8.RowCount - 1; j++)
            {
                writer.WriteStartElement("rowid");
                writer.WriteString(j.ToString());
                for (int i = 0; i < dataGridView8.ColumnCount; i++)
                {
                    writer.WriteStartElement(dataGridView8.Columns[i].HeaderText);
                    writer.WriteString(dataGridView8.Rows[j].Cells[i].Value.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("Данные были экспортированны в xml файл", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter();
            string columns = "goods_scrapped.goods_id, goods_name, cost_of_one_unit_of_goods, date_write_off, goods_count, goods_amount";
            string sql = "SELECT " + columns + " FROM goods INNER JOIN goods_scrapped ON goods.goods_id = goods_scrapped.goods_id WHERE date_write_off >= @date_1 AND date_write_off <= @date_2";
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("date_1", dateTimePicker11.Value);
            command.Parameters.AddWithValue("date_2", dateTimePicker10.Value);
            dataAdapter.SelectCommand = command;
            dataAdapter.Fill(dataTable);
            dataGridView9.DataSource = dataTable;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            XmlWriter writer = XmlWriter.Create("exportWritterScrapped.xml", settings);
            writer.WriteStartDocument();
            writer.WriteStartElement("data");
            for (int j = 0; j < dataGridView9.RowCount - 1; j++)
            {
                writer.WriteStartElement("rowid");
                writer.WriteString(j.ToString());
                for (int i = 0; i < dataGridView9.ColumnCount; i++)
                {
                    writer.WriteStartElement(dataGridView9.Columns[i].HeaderText);
                    writer.WriteString(dataGridView9.Rows[j].Cells[i].Value.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
            MessageBox.Show("Данные были экспортированны в xml файл", "Выполнено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       
    }
}
