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

        public TlsConfiguration SavedConfiguration
        {
            get
            {
                return new TlsConfiguration {
                    PeerCertificate = trustedCertTextBox.Text, LocalCertificate = localCertificateTextBox.Text,
                    PrivateKey = privateKeyTextBox.Text, CipherList = cipherListTextBox.Text,
                    Tls10 = checkBoxV1_0.Checked, Tls11 = checkBoxV1_1.Checked, Tls12 = checkBoxV1_2.Checked
                };
            }
            set
            {
                trustedCertTextBox.Text = value.PeerCertificate;
                localCertificateTextBox.Text = value.LocalCertificate;
                privateKeyTextBox.Text = value.PrivateKey;
                cipherListTextBox.Text = value.CipherList;
                checkBoxV1_0.Checked = value.Tls10;
                checkBoxV1_1.Checked = value.Tls11;
                checkBoxV1_2.Checked = value.Tls12;
            }
        }

        public TLSConfig Configuration
        {
            get
            {
                var config = new TLSConfig(
                    this.trustedCertTextBox.Text,
                    this.localCertificateTextBox.Text,
                    this.privateKeyTextBox.Text,
                    this.checkBoxV1_0.Checked,
                    this.checkBoxV1_1.Checked,
                    this.checkBoxV1_2.Checked,
                    false,
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
