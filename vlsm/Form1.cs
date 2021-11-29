using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Net;

namespace vlsm
{
    public partial class Form1 : Form
    {
        private class Rete
        {
            public IPNetwork Address { get; set; }
            public IPAddress Default_Gateway { get; set; }
            public IPAddress Subnet { get; set; }
            public IPAddress Primo_ip { get; set; }
            public IPAddress Ultimo_ip { get; set; }

            public Rete(IPNetwork Address)
            {
                this.Address = Address;
                this.Default_Gateway = Address.LastUsable;
                this.Subnet = Address.Netmask;
                this.Primo_ip = Address.FirstUsable;
                this.Ultimo_ip = Ultimoipusabile(this.Default_Gateway);
            }

            private IPAddress Ultimoipusabile(IPAddress Dg)
            {
                var ot = Dg.ToString().Split('.');
                ot[3] = (int.Parse(ot[3]) - 1).ToString();                
                return IPAddress.Parse($"{ot[0]}.{ot[1]}.{ot[2]}.{ot[3]}");
            }
        }
        private class Sottoreti
        {
            public string SottoreteN { get; set; }
            public int SottoreteNum { get; set; }

            public Sottoreti(string n, int num)
            {
                if (string.IsNullOrEmpty(n))
                    throw new Exception("E");
                this.SottoreteN = n;
                this.SottoreteNum = num;
            }
            public override string ToString()
            {
                return SottoreteN + "-" + SottoreteNum;
            }
        }

        private IPAddress Ip_rete(IPAddress Broad)
        {
            var ot = Broad.ToString().Split('.');
            if (int.Parse(ot[3]) < 255)
                ot[3] = (int.Parse(ot[3]) + 1).ToString();
            else if (int.Parse(ot[2]) < 255)
                ot[2] = (int.Parse(ot[2]) + 1).ToString();
            else if (int.Parse(ot[1]) < 255)
                ot[1] = (int.Parse(ot[1]) + 1).ToString();
            else
                ot[0] = (int.Parse(ot[0]) + 1).ToString();
            for(int t = 0; t < ot.Length; t++)
                if (ot[t] == "255")
                    ot[t] = "0";
            return IPAddress.Parse($"{ot[0]}.{ot[1]}.{ot[2]}.{ot[3]}");
        }

        char findClass(char[] str)
        {
            // storing first octet in arr[] variable
            char[] arr = new char[4];
            int i = 0;
            while (str[i] != '.')
            {
                arr[i] = str[i];
                i++;
            }
            i--;

            // converting str[] variable into number for
            // comparison
            int ip = 0, j = 1;
            while (i >= 0)
            {
                ip = ip + (str[i] - '0') * j;
                j = j * 10;
                i--;
            }

            // Class A
            if (ip >= 1 && ip <= 126)
                return 'A';

            // Class B
            else if (ip >= 128 && ip <= 191)
                return 'B';

            // Class C
            else if (ip >= 192 && ip <= 223)
                return 'C';

            // Class D
            else if (ip >= 224 && ip <= 239)
                return 'D';

            // Class E       
            else
                return 'E';
        }
        public class Utilities
        {
            public static void ResetAllControls(Control form)
            {
                foreach (Control control in form.Controls)
                {
                    if (control is TextBox)
                    {
                        TextBox textBox = (TextBox)control;
                        textBox.Text = null;
                    }

                    if (control is ListBox)
                    {
                        ListBox listBox = (ListBox)control;
                        listBox.Items.Clear();
                    }

                    if (control is DataGridView)
                    {
                        DataGridView datagrid = (DataGridView)control;
                        datagrid.DataSource = null;
                        datagrid.Rows.Clear();
                    }

                    if (control is NumericUpDown)
                    {
                        NumericUpDown nume = (NumericUpDown)control;
                        nume.Value = 0;
                    }
                }
            }
        }
        bool CheckHost(char classe, int host)
        {
            if (classe == 'A')
                if (host <= 16777214 && host > 65.534)
                    return true;
            if (classe == 'B')
                if (host <= 65.534 && host > 254)
                    return true;
            if (classe == 'C')
                if (host <= 254 && host > 0)
                    return true;
            if (classe == 'D')
                return true;
            if (classe == 'E')
                return true;
            return false;
        }

        public Form1()
        {
            InitializeComponent();
        }
        private List<Sottoreti> eleS = new List<Sottoreti>();
        private List<Rete> eleR = new List<Rete>();
        private void BtnCalc_Click(object sender, EventArgs e)
        {
            BtnCalc.Enabled = false;
            BtnInsSot.Enabled = false;
            BtnModif.Enabled = false;
            BtnEli.Enabled = false;
            IPAddress ipIns = IPAddress.Parse($"{numericUpDown1.Value}.{numericUpDown2.Value}.{numericUpDown3.Value}.{numericUpDown4.Value}");
            char classe = findClass(ipIns.ToString().ToCharArray());

            List<Sottoreti> eleSOrd = eleS.OrderByDescending(i => i.SottoreteNum).ToList();
            lbSottoreti.Items.Clear();
            foreach (var item in eleSOrd)
            {
                lbSottoreti.Items.Add(item);
            }

            var host = eleSOrd.Select(g => g.SottoreteNum);
            int Host = 0;
            foreach (var x in host)
                Host = Host += x;

            bool check = CheckHost(classe, Host);
            if (check)
            {
                IPNetwork ip = default;
                for (int i = 0; i < eleSOrd.Count(); i++)
                {
                    int next = int.Parse(Math.Ceiling(Math.Log(eleSOrd[i].SottoreteNum, 2)).ToString());
                    Byte cidr = Byte.Parse((32 - next).ToString());
                    
                    if (i > 0)
                    {
                        ipIns = Ip_rete(ip.Broadcast);
                        ip = IPNetwork.Parse($"{ipIns}/{cidr}");
                        Rete r2 = new Rete(ip);
                        eleR.Add(r2);
                    }
                    else
                    {
                        ip = IPNetwork.Parse($"{ipIns}/{cidr}");
                        Rete r = new Rete(ip);
                        eleR.Add(r);
                    }
                }
                dataGridViewReti.DataSource = eleR;
            }
            else
            {
                MessageBox.Show("Non valido");
                return;
            }
        }


        private void BtnInsSot_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtNomSot.Text) || int.TryParse(TxtNumSot.Text, out int Num))
            {
                Sottoreti s = new Sottoreti(txtNomSot.Text, int.Parse(TxtNumSot.Text) + 3);
                eleS.Add(s);
                MessageBox.Show("Inserita");
            }
            BtnVisuaSot.PerformClick();
        }

        private void BtnVisuaSot_Click(object sender, EventArgs e)
        {
            lbSottoreti.Items.Clear();
            foreach(var item in eleS)
            {
                lbSottoreti.Items.Add(item);
            }
        }

        private void lbSottoreti_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbSottoreti.SelectedIndex < 0)
                return;
            Sottoreti sotselez = lbSottoreti.SelectedItem as Sottoreti;
            txtNomSot.Text = sotselez.SottoreteN;
            TxtNumSot.Text = sotselez.SottoreteNum.ToString();
        }

        private void BtnModif_Click(object sender, EventArgs e)
        {
            if (lbSottoreti.SelectedIndex < 0)
                return;
            if (string.IsNullOrEmpty(txtNomSot.Text) || string.IsNullOrEmpty(TxtNumSot.Text))
                MessageBox.Show("Inserisci qualcosa");
            var eleP = eleS.Where(i => i.SottoreteN == txtNomSot.Text).FirstOrDefault();
            if (eleP == null)
                return;
            if (int.TryParse(TxtNumSot.Text, out int Num))
            {
                eleP.SottoreteN = txtNomSot.Text;
                eleP.SottoreteNum = Num;
                MessageBox.Show("Modificato");
            }
            BtnVisuaSot.PerformClick();
        }

        private void BtnEli_Click(object sender, EventArgs e)
        {
            Sottoreti eleSelez = lbSottoreti.SelectedItem as Sottoreti;
            eleS.RemoveAll(i => i.SottoreteN == eleSelez.SottoreteN);
            BtnVisuaSot.PerformClick();
            MessageBox.Show("Eliminato");
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            Utilities.ResetAllControls(this);
            Utilities.ResetAllControls(this.groupBoxSot);
            eleR.Clear();
            eleS.Clear();
            BtnCalc.Enabled = true;
            BtnInsSot.Enabled = true;
            BtnModif.Enabled = true;
            BtnEli.Enabled = true;
        }
    }
}
