using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Aftership.Enums;
using Newtonsoft.Json.Linq;

namespace Aftership
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void btnXem_Click(object sender, EventArgs e)
        {
            if (cbbType.Text == "Tra điểm đến")
            {
                ConvertJsonToDataTable();
            }
            else if (cbbType.Text == "Tra toàn bộ")
            {
                ConvertJsonToDataTableAll();
            }
            
        }

        public void ConvertJsonToDataTable()
        {
            try
            {
                int count = dgvPromotion.Rows.Count;
                string[] arrayi = new string[count];
                for (int i = 0; i < count-1; i++)
                {
                    arrayi[i] = dgvPromotion.Rows[i].Cells[0].Value.ToString();
                }
                dgvPromotion.Rows.Clear();
                ConnectionAPI connection = new ConnectionAPI("e3ddaecb-2ea2-4b2f-8fee-9f4e53c69704");
                for (int i = 0; i < count - 1; i++)
                {
                    Tracking tracking = new Tracking(arrayi[i]) { slug = txtCourier.Text };
                    connection.getTrackingByNumber(tracking);


                    string jsonString = connection.json_response;
                    JToken token = JObject.Parse(jsonString);

                    string parameter;
                    if (tracking.id != null && tracking.id.Equals(""))
                    {
                        parameter = tracking.id;
                    }
                    else
                    {
                        String paramRequiredFields = connection.replaceFirst(tracking.getQueryRequiredFields(), "&", "?");
                        parameter = tracking.slug + "/" + tracking.trackingNumber + paramRequiredFields;
                    }

                    JObject response = connection.request("GET", "/last_checkpoint/" + parameter, null);

                    JObject checkpointJSON = (JObject)response["data"]["checkpoint"];
                    //List<Checkpoint> check = new List<Checkpoint>();
                    Checkpoint checkpoint = null;
                    if (checkpointJSON.Count != 0)
                    {
                        checkpoint = new Checkpoint(checkpointJSON);
                    }


                    string[] row = new string[] { arrayi[i], checkpoint.city, checkpoint.countryName, checkpoint.checkpointTime };
                    dgvPromotion.Rows.Add(row);
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void ConvertJsonToDataTableAll()
        {
            try
            {
                ConnectionAPI connection = new ConnectionAPI("e3ddaecb-2ea2-4b2f-8fee-9f4e53c69704");
                Tracking tracking = new Tracking(txtID.Text) { slug = txtCourier.Text };
                connection.getTrackingByNumber(tracking);

                string jsonString = connection.json_response;
                JToken token = JObject.Parse(jsonString);

                String parametersExtra;
                if (tracking.id != null && !tracking.id.Equals(""))
                {
                    parametersExtra = tracking.id;

                }
                else
                {
                    //get the require fields if any (postal_code, tracking_account etc..)
                    String paramRequiredFields = connection.replaceFirst(tracking.getQueryRequiredFields(), "&", "?");
                    parametersExtra = tracking.slug + "/" + tracking.trackingNumber +
                        paramRequiredFields;
                }
                JObject response = connection.request("GET", "/trackings/" + parametersExtra, null);
                JObject trackingJSON = (JObject)response["data"]["tracking"];
                Tracking tracking1 = null;
                if (trackingJSON.Count != 0)
                {
                    tracking1 = new Tracking(trackingJSON);
                }

                var rs = tracking1.checkpoints.ToList();
                foreach (var item in rs)
                {
                    string[] row = new string[] { item.city, item.countryName, item.checkpointTime };
                    dgvPromotion.Rows.Add(row);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbbType.Items.Add(new { Text = "Tra toàn bộ", Value = "1" });
            cbbType.Items.Add(new { Text = "Tra điểm đến", Value = "2" });

            cbbType.DisplayMember = "Text";
            cbbType.ValueMember = "Value";

        }

        private void cbbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbbType.Text == "Tra điểm đến")
            {
                label1.Visible = false;
                txtID.Visible = false;
                //label2.Visible = false;
                //txtCourier.Visible = false;

                dgvPromotion.ColumnCount = 4;
                dgvPromotion.Columns[0].Name = "Tracking Number";
                dgvPromotion.Columns[1].Name = "City";
                dgvPromotion.Columns[2].Name = "Country Name";
                dgvPromotion.Columns[3].Name = "Checkpoint Time";

            }
            else if (cbbType.Text == "Tra toàn bộ")
            {
                label1.Visible = true;
                txtID.Visible = true;
                //label2.Visible = true;
                //txtCourier.Visible = true;
                dgvPromotion.ColumnCount = 3;
                dgvPromotion.Columns[0].Name = "City";
                dgvPromotion.Columns[1].Name = "Country Name";
                dgvPromotion.Columns[2].Name = "Checkpoint Time";
            }
        }

        private void dgvPromotion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                try
                {
                    string s = Clipboard.GetText();
                    string[] lines = s.Split('\n');
                    int tong = 0;
                    int iFail = 0, iRow = dgvPromotion.CurrentCell.RowIndex;
                    int iCol = dgvPromotion.CurrentCell.ColumnIndex;
                    DataGridViewCell oCell;
                    foreach (string line in lines)
                    {
                        tong += 1;
                        //string item = line.Split('\r');
                        this.dgvPromotion.Rows.Add(line.Replace("\r", ""));
                        if (iFail > 0)
                            MessageBox.Show(string.Format("{0} updates failed due" +
                                            " to read only column setting", iFail));
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("The data you pasted is in the wrong format for the cell");
                    return;
                }
            }
        }

    }
}
