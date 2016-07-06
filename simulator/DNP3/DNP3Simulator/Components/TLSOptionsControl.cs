using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Automatak.DNP3.Interface;

namespace Automatak.Simulator.DNP3
{
    public partial class TLSOptionsControl : UserControl
    {
        public TLSOptionsControl()
        {
            InitializeComponent();
        }

        public TLSConfig Configuration
        {
            get
            {
                var config = new TLSConfig(
                    this.trustedCertTextBox.Text,
                    this.localCertificateTextBox.Text,
                    this.privateKeyTextBox.Text,
                    Convert.ToInt32(this.maxDepthNumericUpDown.Value),
                    this.checkBoxV1_0.Checked,
                    this.checkBoxV1_1.Checked,
                    this.checkBoxV1_2.Checked,
                    this.cipherListTextBox.Text
                );

                ValidateConfig(config);

                return config;
            }
        }

        private void ValidateConfig(TLSConfig config)
        {
            
            if (string.IsNullOrWhiteSpace(config.peerCertFilePath) || string.IsNullOrWhiteSpace(Path.GetFullPath(config.peerCertFilePath)))
            {
                throw new Exception("Invalid Trusted Certificate File Path");                    
            }            
            
            if (string.IsNullOrWhiteSpace(config.localCertFilePath) || string.IsNullOrWhiteSpace(Path.GetFullPath(config.localCertFilePath)))
            {
                throw new Exception("Invalid Certificate File Path");
            }
                      
            if (string.IsNullOrWhiteSpace(config.privateKeyFilePath) || string.IsNullOrWhiteSpace(Path.GetFullPath(config.privateKeyFilePath)))
            {
                throw new Exception("Invalid Private Key File Path");                    
            }

            if (!(config.allowTLSv10 || config.allowTLSv11 || config.allowTLSv12))
            {
                throw new Exception("At least one TLS version must be selected");
            }            
        }

        private void browseCertButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.localCertificateTextBox.Text = openFileDialog.FileName;
            }
        }

        private void browsePrivateKeyButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.privateKeyTextBox.Text = openFileDialog.FileName;
            }
        }

        private void browseTrustedCertButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.trustedCertTextBox.Text = openFileDialog.FileName;
            }
        }
    }
}
