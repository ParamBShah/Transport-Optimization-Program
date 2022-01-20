using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace DEProjectApp
{
    public partial class Form1 : Form
    {
        MySql.Data.MySqlClient.MySqlConnection conn;
        MySql.Data.MySqlClient.MySqlCommand cmd;
        MySql.Data.MySqlClient.MySqlDataReader rdr;

        int[,] graph = new int[,] { { 0, 15, 4, 7, 2, 9, 8 },
                                      { 15, 0, 0, 0, 0, 0, 0 },
                                      { 4, 0, 0, 2, 0, 0, 5 },
                                      { 7, 0, 2, 0, 0, 0, 0 },
                                      { 2, 0, 0, 0, 0, 0, 11 },
                                      { 9, 0, 0, 0, 0, 0, 0 },
                                      { 8, 0, 5, 0, 11, 0, 0 } };

        public int[] shortdist;

        public Form1()
        {
            InitializeComponent();
            db_connection();
        }

        private void db_connection()
        {
            string myConnectionString;

            myConnectionString = "server=localhost;uid=USER;pwd=PASSWORD;database=TESTdb;port=3306";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void stgrdVwBox_FillData()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            string sql23 = "SELECT pointnmbr, pointuniqnmbr, pointname, pointaddress FROM locations ORDER BY pointnmbr";

            cmd = new MySqlCommand(sql23, conn);
            rdr = cmd.ExecuteReader();

            DataTable table25 = new DataTable();

            table25.Columns.Add("Point", typeof(string));
            table25.Columns.Add("PointNmbr", typeof(string));
            table25.Columns.Add("PointName", typeof(string));
            table25.Columns.Add("PointAddress", typeof(string));

            while (rdr.Read())
            {
                table25.Rows.Add(rdr["pointnmbr"], rdr["pointuniqnmbr"], rdr["pointname"], rdr["pointaddress"]);
            }
            
            listView1.Items.Clear();
            for (int i = 0; i < table25.Rows.Count; i++)
            {
                DataRow rw = table25.Rows[i];
                ListViewItem itm = new ListViewItem(rw[0].ToString());
                for (int j = 1; j < table25.Columns.Count; j++)
                {
                    itm.SubItems.Add(rw[j].ToString());
                }
                listView1.Items.Add(itm);
            }

            rdr.Close();
            conn.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                if (comboBox1.Text == "VADODARA_VDR")
                {
                    stgrdVwBox_FillData();
                    panel1.Visible = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
            {
                if (comboBox3.SelectedItem != null && comboBox4.SelectedItem != null)
                {
                    SHORTPATH t = new SHORTPATH();
                    shortdist = t.dijkstra(graph, 6);
                    calacFinalResult();
                }
                else
                {
                    MessageBox.Show("Please select a value in all of the above dropdown lists.");
                }
            }
            else
            {
                MessageBox.Show("Please select a value in all of the above dropdown lists.");
            }
        }

        private void calacFinalResult() 
        {
            int[] ignorPoints = { 0, 6 };

            RtryCalcCode:

            Boolean ignorPointsCheck = false;
            string stationUniqNmbr = null;
            string sqlTest521 = null;
            string sqlTest523 = null;
            Double vhclRangeTemp;
            int vhclRange;

            int PayLoadW = Convert.ToInt32(comboBox4.Text);
            int TimeLimit = Convert.ToInt32(comboBox3.Text);
            int MinSpeed, shortestRoute;
            Double MinSpeedTemp;

            int lowestIndex = 1;
            int lowestNmbr = shortdist[1];

            for (int i = 0; i < shortdist.Length; i++)
            {
                ignorPointsCheck = true;

                for (int j = 0; j < ignorPoints.Length; j++)
                {
                    if (i == ignorPoints[j])
                    {
                        ignorPointsCheck = false;
                    }
                }

                if (ignorPointsCheck == true)
                {
                   if (shortdist[i] < lowestNmbr)
                   {
                        lowestIndex = i;
                        lowestNmbr = shortdist[i];
                   }
                }
            }

            shortestRoute = lowestNmbr;
            MinSpeedTemp = shortestRoute * 360;
            MinSpeedTemp = MinSpeedTemp / TimeLimit;
            MinSpeedTemp = Math.Round(MinSpeedTemp, 3);
            MinSpeedTemp = Math.Floor(MinSpeedTemp);
            MinSpeed = Convert.ToInt32(MinSpeedTemp);
            MinSpeed = MinSpeed - (MinSpeed % 10);

            sqlTest521 = "SELECT pointnmbr, pointuniqnmbr, pointname, pointaddress FROM locations WHERE pointnmbr = " + lowestIndex.ToString() + " ORDER BY pointnmbr";
            stationUniqNmbr = findDataValuesSql(sqlTest521, 1);

            sqlTest521 = "SELECT vehiclenmbr, vehiclename, vehiclerange FROM resources WHERE pointuniqnmbr = '" + stationUniqNmbr.ToString() + "' ORDER BY pointuniqnmbr";
            vhclRangeTemp = Convert.ToInt32(findDataValuesSql(sqlTest521, 2).ToString());
            vhclRangeTemp = vhclRangeTemp * 0.6;
            vhclRangeTemp = Math.Round(vhclRangeTemp, 3);
            vhclRangeTemp = Math.Floor(vhclRangeTemp);
            vhclRange = Convert.ToInt32(vhclRangeTemp);

            if ((vhclRange - shortestRoute) > 50)
            {
                sqlTest523 = "SELECT COUNT(vehiclenmbr) FROM resources WHERE pointuniqnmbr = '" + stationUniqNmbr.ToString() + "' and vehiclepayload >= " + PayLoadW.ToString() + " and vehiclespeed >= " + MinSpeed.ToString();
                if (Convert.ToInt32(countDataValuesSql(sqlTest523)) >= 1)
                {
                    sqlTest521 = "SELECT pointnmbr, pointuniqnmbr, pointname, pointaddress FROM locations WHERE pointnmbr = " + lowestIndex.ToString() + " ORDER BY pointnmbr";
                    textBox1.Text = findDataValuesSql(sqlTest521, 2).ToString();
                    textBox2.Text = findDataValuesSql(sqlTest521, 3).ToString();
                    sqlTest521 = "SELECT vehiclenmbr, vehiclename FROM resources WHERE pointuniqnmbr = '" + stationUniqNmbr.ToString() + "' ORDER BY pointuniqnmbr";
                    textBox3.Text = findDataValuesSql(sqlTest521, 1).ToString();
                    textBox4.Text = findDataValuesSql(sqlTest521, 0).ToString();

                    if (listView1.Items.Count > 0)
                    {
                        listView1.Items[lowestIndex].Selected = true;
                        listView1.Select();
                    }

                }
                else
                {
                    if (ignorPoints.Length < shortdist.Length)
                    {
                        ignorPoints = ignorPoints.Concat(new int[] { lowestIndex }).ToArray();
                        goto RtryCalcCode;
                    }

                    else
                    {
                        MessageBox.Show("No result found for the given conditions.");
                    }
                }
            }

            else
            {
                if (ignorPoints.Length < shortdist.Length)
                {
                    ignorPoints = ignorPoints.Concat(new int[] { lowestIndex }).ToArray();
                    goto RtryCalcCode;
                }

                else
                {
                    MessageBox.Show("No result found for the given conditions.");
                }
            }

        }

        private string findDataValuesSql(string sql385, int colmnNmbr)
        {
            string result385 = null;

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            cmd = new MySqlCommand(sql385, conn);
            rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                result385 = rdr[colmnNmbr].ToString();
            }

            rdr.Close();
            conn.Close();

            return result385;
        }

        private int countDataValuesSql(string sql485)
        {
            int result485;

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            cmd = new MySqlCommand(sql485, conn);

            result485 = Convert.ToInt32(cmd.ExecuteScalar().ToString());

            conn.Close();

            return result485;
        }

    }

    public class SHORTPATH
    {
        static int V = 7;
        int minDistance(int[] dist,
                        bool[] sptSet)
        {
            int min = int.MaxValue, min_index = -1;

            for (int v = 0; v < V; v++)
                if (sptSet[v] == false && dist[v] <= min)
                {
                    min = dist[v];
                    min_index = v;
                }

            return min_index;
        }

        public int[] dijkstra(int[,] graph, int src)
        {
            int[] dist = new int[V]; 

            bool[] sptSet = new bool[V];

            for (int i = 0; i < V; i++)
            {
                dist[i] = int.MaxValue;
                sptSet[i] = false;
            }

            dist[src] = 0;

            for (int count = 0; count < V - 1; count++)
            {
                int u = minDistance(dist, sptSet);

                sptSet[u] = true;

                for (int v = 0; v < V; v++)

                    if (!sptSet[v] && graph[u, v] != 0 && dist[u] != int.MaxValue && dist[u] + graph[u, v] < dist[v])
                        dist[v] = dist[u] + graph[u, v];
            }

            return dist;
        }
    }
}