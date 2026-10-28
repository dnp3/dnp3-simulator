using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3.Components
{
    public partial class MasterDialog : Form
    {
        public MasterDialog()
        {
            InitializeComponent();

            this.linkConfigControl.Configuration = new LinkConfig(true, false);
        }

        public MasterDialog(string alias, MasterStackConfig config) : this()
        {
            Text = "Master settings";
            buttonAdd.Text = "Apply";
            textBoxID.Text = alias;
            linkConfigControl.Configuration = config.link;
            masterConfigControl.Configuration = config.master;
        }

        private void buttonADD_Click(object sender, EventArgs e)
        {            
            if (string.IsNullOrWhiteSpace(textBoxID.Text))
            {
                MessageBox.Show(this, "Specify a master alias.", "Master settings");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public String SelectedAlias
        {
            get
            {
                return textBoxID.Text;
            }
        }

        public MasterStackConfig Configuration
        {
            get
            {
                var config = new MasterStackConfig();
                config.link = this.linkConfigControl.Configuration;
                config.master = this.masterConfigControl.Configuration;
                return config;
            }
        }
    }
}
